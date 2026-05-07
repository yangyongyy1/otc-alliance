using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;
using System.Linq;

namespace ClientPlatform.Authorization;

public class ClientPlatformAuthorizationProvider : AuthorizationProvider
{
    public override void SetPermissions(IPermissionDefinitionContext context)
    {
       
        ConfigurationUserManagement(context);

        ConfigurationVABusiness(context);

         ConfigurationOrderManagement(context);

        ConfigurationAllianceManagement(context);

        ConfigurationBasicData(context);

        ConfigurationSystemConfiguration(context);

       

    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, ClientPlatformConsts.LocalizationSourceName);
    }


    /// <summary>
    /// 联盟管理
    /// </summary>
    /// <param name="context"></param>
    public static void ConfigurationAllianceManagement(IPermissionDefinitionContext context)
    {
        var allianceManagement = context.CreatePermission(PermissionNames.Pages_AllianceManagement, L("Pages_AllianceManagement"));
        var allianceList = allianceManagement.CreateChildPermission(PermissionNames.Pages_AllianceManagement_AllianceList, L("Pages_AllianceManagement_AllianceList"));
        var allianceListBtns = typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_AllianceManagement_AllianceList_Btn"));
        foreach (var item in allianceListBtns)
        {
            allianceList.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }

        var merchantList= allianceManagement.CreateChildPermission(PermissionNames.Pages_AllianceManagement_MerchantList, L("Pages_AllianceManagement_MerchantList"));

        var merchantListBtns = typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_AllianceManagement_MerchantList_Btn"));

        foreach (var item in merchantListBtns)
        {
            merchantList.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }
    }

    /// <summary>
    /// 配置VA
    /// </summary>
    /// <param name="context"></param>
    public static void ConfigurationVABusiness(IPermissionDefinitionContext context)
    {
        var vaBusiness = context.CreatePermission(PermissionNames.Pages_VABusiness, L("Pages_VABusiness"));
        var accountManagement = vaBusiness.CreateChildPermission(PermissionNames.Pages_VABusiness_AccountManagement, L("Pages_VABusiness_AccountManagement"));
        var accountManagementBtns = typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_VABusiness_AccountManagement_Btn"));
        foreach (var item in accountManagementBtns)
        {
            accountManagement.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }
        var applicationManagement = vaBusiness.CreateChildPermission(PermissionNames.Pages_VABusiness_ApplicationManagement, L("Pages_VABusiness_ApplicationManagement"));
        var applicationManagementBtns = typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_VABusiness_ApplicationManagement_Btn"));
        foreach (var item in applicationManagementBtns)
        {
            applicationManagement.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }
    }




    /// <summary>
    /// 配置用户管理
    /// </summary>
    /// <param name="context"></param>
    public static void ConfigurationUserManagement(IPermissionDefinitionContext context)
    {
        var userManagement = context.CreatePermission(PermissionNames.Pages_UserManagement, L("Pages_UserManagement"));
       
        var  userList = userManagement.CreateChildPermission(PermissionNames.Pages_UserManagement_UserList, L("Pages_UserManagement_UserList"));

        var userListBtns = typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_UserManagement_UserList_Btn"));

        foreach (var item in userListBtns)
        {
            userList.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }

        var authenticationManagement = userManagement.CreateChildPermission(PermissionNames.Pages_UserManagement_AuthenticationManagement, L("Pages_UserManagement_AuthenticationManagement"));

        var authenticationManagementBtns = typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_UserManagement_AuthenticationManagement_Btn"));

        foreach (var item in authenticationManagementBtns)
        {
            authenticationManagement.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }
    }


    /// <summary>
    /// 配置系管理
    /// </summary>
    /// <param name="context"></param>
    public static void ConfigurationSystemConfiguration (IPermissionDefinitionContext context)
    {
        var systemConfiguration = context.CreatePermission(PermissionNames.Pages_SystemConfiguration, L("Pages_SystemConfiguration"));

        var adminAccountSettings = systemConfiguration.CreateChildPermission(PermissionNames.Pages_SystemConfiguration_AdminAccountSettings, L("Pages_SystemConfiguration_AdminAccountSettings"));

        var adminAccountSettingsBtns = typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_SystemConfiguration_AdminAccountSettings_Btn"));

        foreach (var item in adminAccountSettingsBtns)
        {
            adminAccountSettings.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }

        var adminRoleManagement = systemConfiguration.CreateChildPermission(PermissionNames.Pages_SystemConfiguration_RoleManagement, L("Pages_SystemConfiguration_RoleManagement"));

        var adminRoleManagementBtns = typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_SystemConfiguration_RoleManagement_Btn"));

        foreach (var item in adminRoleManagementBtns)
        {
            adminRoleManagement.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }
    }


    /// <summary>
    /// 基础数据
    /// </summary>
    /// <param name="context"></param>

    public static void ConfigurationBasicData(IPermissionDefinitionContext context)
    {
        var baseData = context.CreatePermission(PermissionNames.Pages_BasicData,L("Pages_BasicData"));

        var countriesAndRegions = baseData.CreateChildPermission(PermissionNames.Pages_BasicData_CountriesAndRegions, L("Pages_BasicData_CountriesAndRegions"));

        var btncountriesAndRegions= typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_BasicData_CountriesAndRegions_Btn"));

        foreach (var item in btncountriesAndRegions)
        {
            countriesAndRegions.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }

        var channelsManagement = baseData.CreateChildPermission(PermissionNames.Pages_BasicData_DataDictionary, L("Pages_BasicData_DataDictionary"));

        var btnchannelsManagement = typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_BasicData_DataDictionary_Btn"));

        foreach (var item in btnchannelsManagement)
        {
            channelsManagement.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }
    }

    /// <summary>
    /// 订单管理
    /// </summary>
    /// <param name="context"></param>
    public static void ConfigurationOrderManagement(IPermissionDefinitionContext context)
    {
        var orderManagement = context.CreatePermission(PermissionNames.Pages_OrderManagement, L("Pages_OrderManagement"));

        var depositOrders = orderManagement.CreateChildPermission(PermissionNames.Pages_OrderManagement_DepositOrders, L("Pages_OrderManagement_DepositOrders"));
        var depositBtns = typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_OrderManagement_DepositOrders_Btn"));
        foreach (var item in depositBtns)
        {
            depositOrders.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }

        var vaOrders = orderManagement.CreateChildPermission(PermissionNames.Pages_OrderManagement_VAOrders, L("Pages_OrderManagement_VAOrders"));
        var vaBtns = typeof(PermissionNames).GetFields().Where(n => n.Name.StartsWith("Pages_OrderManagement_VAOrders_Btn"));
        foreach (var item in vaBtns)
        {
            vaOrders.CreateChildPermission(item.GetValue(item).ToString(), L(item.Name));
        }
    }
}
