using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ClientPlatform.AllianceManagement;
using ClientPlatform.Authorization;
using ClientPlatform.Kyc;
using ClientPlatform.Kyc;
using ClientPlatform;
using ClientPlatform.UserManagement.Dot;

namespace ClientPlatform.UserManagement
{
    /// <summary>
    /// 用户认证管理
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_UserManagement_AuthenticationManagement)]
    [DisableAuditing]
    public class UserIdentityAppService : AppServiceBase
    {
        private readonly IRepository<UserIdentity, int> _userIdentityRepository;
        private readonly IRepository<Alliance, int> _allianceRepository;
        private readonly IRepository<Merchant, int> _merchantRepository;
        private readonly IRepository<ClientUser, int> _clientUserRepository;
        private readonly IRepository<DynamicForm, int> _dynamicFormRepository;
        private readonly IRepository<KycApplicant, Guid> _kycApplicantRepository;
        private ILogger _logger;
        private readonly IRepository<KycApplicantDocument, Guid> _kycApplicantDocumentRepository;
        private readonly KycManager _kycManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userIdentityRepository"></param>
        /// <param name="allianceRepository"></param>
        /// <param name="merchantRepository"></param>
        /// <param name="clientUserRepository"></param>
        /// <param name="logger"></param>
        /// <param name="dynamicFormRepository"></param>
        public UserIdentityAppService(
            IRepository<UserIdentity, int> userIdentityRepository,
            IRepository<Alliance, int> allianceRepository,
            IRepository<Merchant, int> merchantRepository,
            IRepository<ClientUser, int> clientUserRepository,
            IRepository<KycApplicant, Guid> kycApplicantRepository,
            ILogger logger,
            IRepository<KycApplicantDocument, Guid> kycApplicantDocumentRepository,
            IRepository<DynamicForm, int> dynamicFormRepository,
            KycManager kycManager)
        {
            _userIdentityRepository = userIdentityRepository;
            _allianceRepository = allianceRepository;
            _merchantRepository = merchantRepository;
            _clientUserRepository = clientUserRepository;
            _logger = logger;
            _dynamicFormRepository = dynamicFormRepository;
            _logger = logger;
            _kycApplicantRepository = kycApplicantRepository;
            _dynamicFormRepository = dynamicFormRepository;
            _kycApplicantDocumentRepository = kycApplicantDocumentRepository;
            _kycManager = kycManager;
        }

        /// <summary>
        /// 获取用户认证列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_UserManagement_AuthenticationManagement_BtnQuery)]
        public async Task<PagedResultDto<KycApplicantDto>> GetAllUserIdentityListAsync(PagedUserIdentityResultRequestDto input)
        {

            if (input.Sorting.StartsWith("name"))
            {
                input.Sorting = input.Sorting.Replace("name", "firstName");
            }
            string cleanInput = string.Empty;
            if (!string.IsNullOrEmpty(input.Name))
            {
                cleanInput = string.Concat(input.Name.Where(c => !char.IsWhiteSpace(c)));
            }
            var qry = _kycApplicantRepository.GetAllIncluding(n => n.User, n => n.User.Merchant, n => n.User.Merchant.Alliance)
                       .WhereIf(input.UserId.HasValue, n => n.UserId == input.UserId.Value)
                       .WhereIf(input.AllianceId.HasValue, n => n.User.Merchant.Alliance.Id == input.AllianceId.Value)
                       .WhereIf(input.MerchantId.HasValue, n => n.User.Merchant.Id == input.MerchantId.Value)
                       .WhereIf(!input.Name.IsNullOrWhiteSpace(), n => EF.Functions.Like((n.FirstName ?? "") + (n.LastName ?? ""), $"%{cleanInput}%"))
                       .WhereIf(!input.Email.IsNullOrWhiteSpace(), n => n.Email.Contains(input.Email))
                       .WhereIf(!input.DocumentNumber.IsNullOrWhiteSpace(), n => n.IdDocNumber.Contains(input.DocumentNumber))
                       //.WhereIf(input.DocumentType.HasValue, n => n.IdDocType == input.DocumentType.Value)
                       .WhereIf(input.KycBizStatus.HasValue, n => n.KycBizStatus == input.KycBizStatus.Value)
                       .WhereIf(input.AuthType.HasValue, n => n.User.Merchant.AuthType == input.AuthType.Value)
                       .WhereIf(input.CreationTimeStart.HasValue, n => n.CreationTime >= input.CreationTimeStart.Value)
                       .WhereIf(input.CreationTimeEnd.HasValue, n => n.CreationTime <= input.CreationTimeEnd.Value)
                       .WhereIf(input.ModificationTimeStart.HasValue, n => n.LastModificationTime >= input.ModificationTimeStart.Value)
                       .WhereIf(input.ModificationTimeEnd.HasValue, n => n.LastModificationTime <= input.ModificationTimeEnd.Value)
                       .WhereIf(input.AuthStandardLevel.HasValue, n => n.AuthStandardLevel == input.AuthStandardLevel.Value)
                       .Select(n => new KycApplicantDto
                       {
                           FirstNameEn = n.FirstNameEn,
                           CreationTime = n.CreationTime,
                           LastModificationTime = n.LastModificationTime,
                           FirstName = n.FirstName,
                           Id = n.Id,
                           KycBizStatus = n.KycBizStatus,
                           LastName = n.LastName,
                           LastNameEn = n.LastName,
                           User = n.User,
                           Email = n.Email,
                           AuthStandardLevel = n.AuthStandardLevel,
                           IsClosed = n.IsClosed,
                       })
                       .OrderBy(input.Sorting);


            var count = await qry.CountAsync();

            var pagedResult = await qry.PageBy(input).ToListAsync();

            return new PagedResultDto<KycApplicantDto>(count, pagedResult);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_UserManagement_AuthenticationManagement_BtnViewDetails)]
        public async Task<KycApplicantDetailDto> GetUserIdentitiesByUserIdAsync(Guid id)
        {
            var qry = await _kycApplicantRepository.GetAsync(id);
            var mapResult = ObjectMapper.Map<KycApplicantDetailDto>(qry);
            var kycDocs = await _kycApplicantDocumentRepository.GetAllListAsync(n => n.KycApplicantId == id);
            mapResult.KycApplicantDocuments = kycDocs;
            return mapResult;
        }

        /// <summary>
        /// 重置指定 KYC 记录：
        /// 1. 将当前这条 KYC 记录标记为关闭
        /// 2. 调用领域服务重新生成 WebSDK 链接并创建一条新的 KYC 记录
        /// 3. 重置用户认证状态为待提交
        /// </summary>
        /// <param name="id">KYC 记录主键 ID（KycApplicant.Id）</param>
        /// <returns>新的 KYC WebSDK 链接</returns>
        [AbpAuthorize(PermissionNames.Pages_UserManagement_AuthenticationManagement_BtnViewDetails)]
        public async Task<string> ResetKycAsync(Guid id)
        {
            // 1. 根据 KYC 记录 ID 获取当前申请记录
            var currentApplicant = await _kycApplicantRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (currentApplicant == null)
            {
                throw new UserFriendlyException("KYC applicant not found.");
            }

            var userId = currentApplicant.UserId;

            // 如果当前记录已经是通过状态，则不允许重置，避免误操作
            if (currentApplicant.KycBizStatus == KycBizStatus.APPROVED)
            {
                throw new UserFriendlyException("Approved KYC record cannot be reset.");
            }

            // 2. 校验同一用户 + 同一认证等级是否已经存在其它激活中的记录
            // 业务规则：同一等级同时只允许存在一条未关闭的记录，避免无限新增
            var hasActiveForLevel = await _kycApplicantRepository.GetAll()
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.AuthStandardLevel == currentApplicant.AuthStandardLevel &&
                    !x.IsClosed &&
                    x.Id != currentApplicant.Id);
            if (hasActiveForLevel)
            {
                throw new UserFriendlyException("There is already an active KYC record for this level.");
            }

            // 3. 校验用户是否存在
            var clientUser = await _clientUserRepository.FirstOrDefaultAsync(x => x.Id == userId);
            if (clientUser == null)
            {
                throw new UserFriendlyException("Client user not found.");
            }

            // 4. 将当前这条 KYC 记录标记为关闭（如果尚未关闭）
            if (!currentApplicant.IsClosed)
            {
                currentApplicant.IsClosed = true;
                await _kycApplicantRepository.UpdateAsync(currentApplicant);
            }

            // 5. 生成新的 WebSDK 链接并创建一条新的 KYC 记录（认证等级沿用旧记录的等级）
            var authLevel = currentApplicant.AuthStandardLevel;
            var newLink = await _kycManager.GenerateWebSdkLinkAsync(userId, authLevel, AbpSession.TenantId);

            // 此处不修改用户整体认证状态和已完成等级，避免影响其它已通过等级

            return newLink;
        }
        

        public async Task<string> GetKycLevelNameAsync(AuthStandardLevel authLevel)
        {
            return _kycManager.GetAuthLevelName(authLevel);
        }


    }
}

