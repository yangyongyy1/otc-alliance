using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Timing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.Kyc;
using ClientPlatform.Kyc.Dto;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Entities;
using ClientPlatform.UserManagement;
using ClientPlatformUser.Kyc.Dto;

namespace ClientPlatformUser.Kyc
{
    /// <summary>
    /// KYC 应用服务
    /// 提供给 Web/App 前端使用的 KYC 相关接口
    /// </summary>
    [AbpAuthorize]
    public class KycAppService : AppServiceBase
    {
        private readonly KycManager _kycManager;
        private readonly IRepository<KycApplicant, Guid> _kycApplicantRepository;
        private readonly IRepository<KycApplicantDocument, Guid> _kycApplicantDocumentRepository;
        private readonly IRepository<KycApplicantBeneficiary, Guid> _kycApplicantBeneficiaryRepository;
        private readonly IRepository<UserPayChannelCustomer, Guid> _payChannelCustomerRepository;
        private readonly IRepository<ClientUser, int> _clientUserRepository;
        /// <summary>
        /// 构造函数
        /// </summary>
        public KycAppService(
            KycManager kycManager,
            IRepository<KycApplicant, Guid> kycApplicantRepository,
            IRepository<KycApplicantDocument, Guid> kycApplicantDocumentRepository,
            IRepository<KycApplicantBeneficiary, Guid> kycApplicantBeneficiaryRepository,
            IRepository<UserPayChannelCustomer, Guid> payChannelCustomerRepository,
            IRepository<ClientUser, int> clientUserRepository)
        {
            _kycManager = kycManager;
            _kycApplicantRepository = kycApplicantRepository;
            _kycApplicantDocumentRepository = kycApplicantDocumentRepository;
            _kycApplicantBeneficiaryRepository = kycApplicantBeneficiaryRepository;
            _payChannelCustomerRepository = payChannelCustomerRepository;
            _clientUserRepository = clientUserRepository;
        }

        /// <summary>
        /// 获取当前用户的 KYC 状态信息（包含验证链接和支付客户状态）
        /// 如果没有KYC记录，自动生成验证链接
        /// </summary>
        /// <param name="firstName">名</param>
        /// <param name="lastName">姓</param>
        /// <param name="middleName">中间名</param>
        /// <param name="idDocType">证件类型（身份证/护照等）</param>
        /// <param name="idDocNumber">证件号码</param>
        /// <returns></returns>
        /// <exception cref="Abp.UI.UserFriendlyException"></exception>
        public async Task<GetKycStatusOutput> GetKycStatus(
            string firstName = "",
            string lastName = "",
            string middleName = "",
            string idDocType = "",
            string idDocNumber = "")
        {
            var abpUserId = AbpSession.UserId;
            if (!abpUserId.HasValue)
            {
                throw new Abp.UI.UserFriendlyException("No user logged in");
            }

            // 1. 获取 ClientUser（关键：获取正确的 UserId）
            var clientUser = await _clientUserRepository.GetAll().Include(x=>x.Merchant)
                .FirstOrDefaultAsync(x => x.AbpUserId == abpUserId.Value);

            if (clientUser == null)
            {
                throw new Abp.UI.UserFriendlyException("User or Merchant not found.");
            }

            var userId = clientUser.Id;  // ClientUser.Id（int）用于 KYC 和 Pay 表

            var defaultLevel = AuthStandardLevel.Level1;

            // 如果用户已在系统中标记为通过，则直接返回通过状态（避免再次走后续校验）
            if (clientUser.UserAuthStatus == KycBizStatus.APPROVED)
            {
                return new GetKycStatusOutput
                {
                    KycStatus = KycBizStatus.APPROVED,
                    LevelName = defaultLevel.ToString(),
                    VerificationLink = "",
                    CreatedAt = Clock.Now
                };
            }

            // 2. 获取用户最新的、未关闭的 KYC 申请记录（只看当前有效记录）
            var applicant = await _kycApplicantRepository.GetAll()
                .Where(x => x.UserId == userId && !x.IsClosed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (applicant != null) 
            {
                // 5. 返回现有 KYC 状态
                return new GetKycStatusOutput
                {
                    KycStatus = applicant.KycBizStatus,
                    RejectReason = applicant.ClientComment,
                    RejectLabels = applicant.RejectLabels,
                    LevelName = applicant.AuthStandardLevel.ToString(),
                    VerificationLink = applicant.KycVerificationLink,
                    CreatedAt = applicant.CreatedAt
                };
            }

            // 没有姓名：不生成link
            var hasName =
                !firstName.IsNullOrWhiteSpace() ||
                !lastName.IsNullOrWhiteSpace() ||
                !middleName.IsNullOrWhiteSpace();

            if (!hasName)
            {
                return new GetKycStatusOutput
                {
                    KycStatus = KycBizStatus.NOTSTARTED,
                    LevelName = defaultLevel.ToString(),
                    VerificationLink = "",
                    CreatedAt = Clock.Now
                };
            }

            // 2.2 规则：按姓名 + 证件信息复用 Sumsub 已通过的 KYC 记录
            var normalizedFullName = $"{firstName} {middleName} {lastName}"
                .Trim()
                .Replace("  ", " ");

            // 先按姓名查询所有已通过记录（跨联盟通用）
            var approvedByName = await _kycApplicantRepository.GetAll()
                .Where(x => x.KycBizStatus == KycBizStatus.APPROVED)
                .Where(x =>
                    (x.FixedFirstName + " " + x.FixedMiddleName + " " + x.FixedLastName)
                        .Replace("  ", " ")
                        .Trim() == normalizedFullName)
                .ToListAsync();

            // 2.1：姓名不存在任何通过记录，直接执行下一步认证（生成新的 KYC 流程）
            if (!approvedByName.Any())
            {
                var verificationLinkWhenNoHistory = await _kycManager.GenerateWebSdkLinkAsync(
                    userId,
                    defaultLevel,
                    AbpSession.TenantId);

                return new GetKycStatusOutput
                {
                    KycStatus = KycBizStatus.NOTSTARTED,
                    LevelName = defaultLevel.ToString(),
                    VerificationLink = verificationLinkWhenNoHistory
                };
            }

            // 2.2：姓名存在，通过 证件类型 + 证件号码 判断 Sumsub 是否已认证通过
            // - 若用户尚未填写证件信息：先把证件类型返回前端，引导用户补填证件号码（此时不生成链接）
            // - 若证件信息已填写：匹配到已通过记录则自动通过，并为当前用户补一条 KYC 记录；否则执行下一步认证（生成链接）
            var idDocTypeForFront = approvedByName.First().IdDocType;

            var hasDocInfo =
                !idDocType.IsNullOrWhiteSpace() &&
                !idDocNumber.IsNullOrWhiteSpace();

            if (!hasDocInfo)
            {
                return new GetKycStatusOutput
                {
                    KycStatus = KycBizStatus.NOTSTARTED,
                    LevelName = defaultLevel.ToString(),
                    VerificationLink = "",
                    CreatedAt = Clock.Now,
                    IdDocType = idDocTypeForFront
                };
            }

            var matched = approvedByName.FirstOrDefault(x =>
                x.IdDocType == idDocType &&
                x.IdDocNumber == idDocNumber);

            // Sumsub 已通过，自动通过，不再触发新的认证流程：委托给内部方法处理同步与克隆逻辑
            if (matched != null)
            {
                await SyncApprovedKycForCurrentUserAsync(clientUser, matched, firstName, middleName, lastName);

                return new GetKycStatusOutput
                {
                    KycStatus = KycBizStatus.APPROVED,
                    LevelName = matched.AuthStandardLevel.ToString(),
                    VerificationLink = "",
                    CreatedAt = Clock.Now
                };
            }

            // 证件信息不匹配：执行下一步认证（重新发起 KYC 流程）
            var verificationLink = await _kycManager.GenerateWebSdkLinkAsync(
                userId,
                defaultLevel,
                AbpSession.TenantId);

            return new GetKycStatusOutput
            {
                KycStatus = KycBizStatus.NOTSTARTED,
                LevelName = defaultLevel.ToString(),
                VerificationLink = verificationLink,
                IdDocType = idDocTypeForFront
            };

        }

        /// <summary>
        /// 当通过历史 Sumsub 记录命中已通过 KYC 时，
        /// 为当前用户同步认证状态，并在本系统中补全一套 KYC 主表 + 文档数据
        /// </summary>
        /// <param name="clientUser">当前客户端用户</param>
        /// <param name="matched">匹配到的历史通过 KYC 记录</param>
        /// <param name="firstName">当前请求的名</param>
        /// <param name="middleName">当前请求的中间名</param>
        /// <param name="lastName">当前请求的姓</param>
        private async Task SyncApprovedKycForCurrentUserAsync(
            ClientUser clientUser,
            KycApplicant matched,
            string firstName,
            string middleName,
            string lastName)
        {
            // 1. 更新 ClientUser 认证状态
            clientUser.UserAuthStatus = matched.KycBizStatus;
            clientUser.KycLevelCompleted = ((int)matched.AuthStandardLevel).ToString();
            clientUser.FirstName = firstName;
            clientUser.MiddleName = middleName;
            clientUser.LastName = lastName;
            await _clientUserRepository.UpdateAsync(clientUser);

            // 2. 如果当前用户已经有通过且未关闭的记录，则无需重复创建
            var existingUserKyc = await _kycApplicantRepository.GetAll()
                .Where(x => x.UserId == clientUser.Id && x.KycBizStatus == KycBizStatus.APPROVED && !x.IsClosed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (existingUserKyc != null)
            {
                return;
            }

            // 3. 为当前 UserId 克隆一条通过的 KYC 主记录
            var newApplicant = new KycApplicant
            {
                UserId = clientUser.Id,
                KycProvider = matched.KycProvider,
                KycChannelProductTypes = KycChannelProductTypes.Pay,
                KycType = matched.KycType,
                AuthStandardLevel = matched.AuthStandardLevel,
                KycVerificationLink = string.Empty,
                KycBizStatus = KycBizStatus.APPROVED,
                KycChannelStatus = matched.KycChannelStatus,
                ApplicantId = matched.ApplicantId,
                InspectionId = matched.InspectionId,
                ExternalUserId = matched.ExternalUserId,
                ClientId = matched.ClientId,
                ApiKey = matched.ApiKey,
                ApplicantType = matched.ApplicantType,
                ApplicantPlatform = matched.ApplicantPlatform,
                IpCountry = matched.IpCountry,
                FirstName = matched.FirstName,
                FirstNameEn = matched.FirstNameEn,
                MiddleName = matched.MiddleName,
                MiddleNameEn = matched.MiddleNameEn,
                LastName = matched.LastName,
                LastNameEn = matched.LastNameEn,
                DateOfBirth = matched.DateOfBirth,
                Country = matched.Country,
                FixedFirstName = matched.FixedFirstName ?? matched.FirstName ?? firstName,
                FixedFirstNameEn = matched.FixedFirstNameEn,
                FixedMiddleName = matched.FixedMiddleName ?? matched.MiddleName ?? middleName,
                FixedMiddleNameEn = matched.FixedMiddleNameEn,
                FixedLastName = matched.FixedLastName ?? matched.LastName ?? lastName,
                FixedLastNameEn = matched.FixedLastNameEn,
                FixedDob = matched.FixedDob,
                Gender = matched.Gender,
                Nationality = matched.Nationality,
                TaxResidenceCountry = matched.TaxResidenceCountry,
                Street = matched.Street,
                State = matched.State,
                BuildingNumber = matched.BuildingNumber,
                Town = matched.Town,
                PostCode = matched.PostCode,
                FormattedAddress = matched.FormattedAddress,
                CompanyName = matched.CompanyName,
                CompanyRegistrationNumber = matched.CompanyRegistrationNumber,
                CompanyCountry = matched.CompanyCountry,
                CompanyLegalAddress = matched.CompanyLegalAddress,
                CompanyEmail = matched.CompanyEmail,
                OwnershipStructureDepth = matched.OwnershipStructureDepth,
                SkippedBeneficiaryTypes = matched.SkippedBeneficiaryTypes,
                Email = clientUser.Email ?? matched.Email,
                ReviewStatus = matched.ReviewStatus,
                ReviewAnswer = matched.ReviewAnswer,
                ReviewRejectType = matched.ReviewRejectType,
                ModerationComment = matched.ModerationComment,
                ReviewLevel = matched.ReviewLevel,
                ReviewAttemptCount = matched.ReviewAttemptCount,
                Priority = matched.Priority,
                ReviewDate = matched.ReviewDate,
                ClientComment = matched.ClientComment,
                RejectLabels = matched.RejectLabels,
                AgreementAcceptedAt = matched.AgreementAcceptedAt,
                AgreementSource = matched.AgreementSource,
                EmploymentStatus = matched.EmploymentStatus,
                AnnualIncome = matched.AnnualIncome,
                IdDocType = matched.IdDocType,
                IdDocCountry = matched.IdDocCountry,
                IdDocNumber = matched.IdDocNumber,
                IdDocValidUntil = matched.IdDocValidUntil,
                HasSelfie = matched.HasSelfie,
                HasVideo = matched.HasVideo,
                HasProofOfAddress = matched.HasProofOfAddress,
                FileInfos = matched.FileInfos,
                QuestionnaireJson = matched.QuestionnaireJson,
                RequiredIdDocsJson = matched.RequiredIdDocsJson,
                RawJson = matched.RawJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedTime = DateTime.UtcNow
            };

            await _kycApplicantRepository.InsertAsync(newApplicant);

            // 4. 复制关联的证件记录到当前用户的新 KYC 记录下
            var matchedDocuments = await _kycApplicantDocumentRepository.GetAll()
                .Where(d => d.KycApplicantId == matched.Id)
                .ToListAsync();

            foreach (var doc in matchedDocuments)
            {
                var clonedDoc = new KycApplicantDocument
                {
                    KycApplicantId = newApplicant.Id,
                    InspectionId = doc.InspectionId,
                    IdDocType = doc.IdDocType,
                    ImageId = doc.ImageId,
                    ImageSlot = doc.ImageSlot,
                    Country = doc.Country,
                    ReviewAnswer = doc.ReviewAnswer,
                    ModerationComment = doc.ModerationComment,
                    ClientComment = doc.ClientComment,
                    Url = doc.Url
                };

                await _kycApplicantDocumentRepository.InsertAsync(clonedDoc);
            }

            // 5. 复制关联的受益人/代表记录到当前用户的新 KYC 记录下（企业 KYB 场景）
            var matchedBeneficiaries = await _kycApplicantBeneficiaryRepository.GetAll()
                .Where(b => b.KycApplicantId == matched.Id)
                .ToListAsync();

            foreach (var ben in matchedBeneficiaries)
            {
                var clonedBen = new KycApplicantBeneficiary
                {
                    KycApplicantId = newApplicant.Id,
                    BeneficiaryApplicantId = ben.BeneficiaryApplicantId,
                    BeneficiaryId = ben.BeneficiaryId,
                    Role = ben.Role,
                    Submitted = ben.Submitted,
                    FirstName = ben.FirstName,
                    LastName = ben.LastName,
                    MiddleName = ben.MiddleName,
                    DateOfBirth = ben.DateOfBirth
                };

                await _kycApplicantBeneficiaryRepository.InsertAsync(clonedBen);
            }
        }
    }
}
