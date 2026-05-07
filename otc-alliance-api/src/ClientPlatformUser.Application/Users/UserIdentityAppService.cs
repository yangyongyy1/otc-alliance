using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Dto.Response;
using ClientPlatform.UserManagement;
using ClientPlatformUser.Users.Dto;

namespace ClientPlatformUser.Users
{
    /// <summary>
    /// 用户认证服务
    /// </summary>
    /// 
    [AbpAuthorize]
    public class UserIdentityAppService: AppServiceBase
    {
        private readonly IRepository<ClientUser, int> _clientUserRepository;

        private readonly IRepository<UserIdentity, int> _userIdentityRepository;

        private readonly MerchantPayClient _merchantPayClient;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientUserRepository"></param>
        /// <param name="userIdentityRepository"></param>
        public UserIdentityAppService(
            IRepository<ClientUser, int> clientUserRepository,
            IRepository<UserIdentity, int> userIdentityRepository,
            MerchantPayClient merchantPayClient
            )
        {
            _clientUserRepository = clientUserRepository;
            _userIdentityRepository = userIdentityRepository;
            _merchantPayClient = merchantPayClient;
        }

        /// <summary>
        /// 提交用户认证
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task SubmitAuthentication(SubmitAuthenticationDto input)
        {
            var userInfo = await _clientUserRepository.GetAllIncluding(n => n.Alliance, n => n.Merchant).Where(u => u.AbpUserId == AbpSession.UserId).FirstOrDefaultAsync();

            userInfo.FirstName = input.FirstName;
            userInfo.LastName = input.LastName;
            userInfo.MiddleName = input.MiddleName;
            await _clientUserRepository.UpdateAsync(userInfo);

            var existingIdentity = await _userIdentityRepository.FirstOrDefaultAsync(u => u.UserId == userInfo.Id);
            if (existingIdentity != null)
            {
                var mapper = ObjectMapper.Map(input, existingIdentity);
                existingIdentity.AuthType = userInfo.Merchant.AuthType;
                existingIdentity.Status = userInfo.Merchant.AuthType == ClientPlatform.AuthType.AutoPass ? ClientPlatform.IdentityStatus.Approved : ClientPlatform.IdentityStatus.Pending;
                await _userIdentityRepository.UpdateAsync(existingIdentity);
            }
            else
            {
                UserIdentity userIdentity = new UserIdentity
                {
                    UserId = userInfo.Id,
                    AllianceId = userInfo.AllianceId,
                    MerchantId = userInfo.MerchantId,
                    Email = userInfo.Email,
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    MiddleName = input.MiddleName,
                    City = input.City,
                    PostalCode = input.PostalCode,
                    Address = input.Address,
                    DocumentType = input.DocumentType,
                    DocumentNumber = input.DocumentNumber,
                    DocumentPhotoFrontUrl = input.DocumentPhotoFrontUrl,
                    DocumentPhotoBackUrl = input.DocumentPhotoBackUrl,
                    AuthType = userInfo.Merchant.AuthType,
                    Status = userInfo.Merchant.AuthType == ClientPlatform.AuthType.AutoPass ? ClientPlatform.IdentityStatus.Approved : ClientPlatform.IdentityStatus.Pending,
                };
                await _userIdentityRepository.InsertAsync(userIdentity);


            }
        }

        /// <summary>
        /// 获取创建用户身份所需的表单项 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Abp.UI.UserFriendlyException"></exception>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<List<ChannelRequiredFieldResponse>> GetUserIdentityFormItems()
        { 
            var currentUser = await _clientUserRepository.GetAllIncluding(n => n.Merchant).Where(u => u.AbpUserId == AbpSession.UserId).FirstOrDefaultAsync();
            if (currentUser != null)
            {
                GetCustomerRequiredFieldsInput getCustomerRequiredFieldsInput = new GetCustomerRequiredFieldsInput
                {
                    CustomerType = currentUser.UserType.ToString(),
                    CountryCode = currentUser.CountryCode,
                    MerchantKey = currentUser.Merchant.BusinessID,
                    MerchantSecret = currentUser.Merchant.Key,
                };

                var response = await _merchantPayClient.GetCustomerRequiredFieldsAsync(getCustomerRequiredFieldsInput);
                if (response.IsSuccess)
                {
                    return response.Data;
                }
                else
                { 
                   throw new Abp.UI.UserFriendlyException(response.Msg);
                }
               
            }

            throw new UserFriendlyException("User not found");

        }
    }
}
