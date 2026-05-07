using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform
{
    public enum GoogleBindStatus
    {
        Close,
        Open,
        Reset
    }

    public class EnvironmentConsts
    {
        public const string Development = "Development";
        public const string Production = "Production";
        public const string Staging = "Staging";
        public const string Test = "Test";
        public const string Sandbox = "Sandbox";
    }

    public enum SystemUserType
    {
        /// <summary>
        /// 平台用户
        /// </summary>
        PlatformUser = 1,
        /// <summary>
        /// 业务用户
        /// </summary>
        BusinessUser = 2
    }


    //认证方式

    public enum AuthType
    {
        //自动通过
        AutoPass,
        //人工审核
        ManualReview,
        //SumSub
        SumSub
    }

    /// <summary>
    /// 认证标准
    /// </summary>
    public enum AuthStandardLevel
    {
        /// <summary>
        /// 用户基础认证
        /// </summary>
        [AmbientValue("Test-kyc&liveness")]
        Level1 = 0
    }


    /// <summary>
    /// 账户用户类型
    /// </summary>
    public enum AccountUserType
    {
        Individual,

        Enterprise
    }


    /// <summary>
    /// 账户状态
    /// </summary>
    public enum AccountStatus
    {

        Active,

        Disable
    }


    public enum ApplicationStatus
    {
        /// <summary>
        /// 审核中
        /// </summary>
        Pending,

        /// <summary>
        /// 通过
        /// </summary>
        Approved,

        /// <summary>
        /// 拒绝
        /// </summary>
        Rejected
    }

    /// <summary>
    /// 用户认证状态
    /// </summary>

    public enum UserAuthStatus
    {
        /// <summary>
        /// 未认证
        /// </summary>
        Unauthenticated,

        /// <summary>
        /// 已认证
        /// </summary>
        Authenticated,

        //认证中

        Authenticating,

        //认证失败

        AuthenticationFailed,

        //需要补充材料

        NeedSupplementaryMaterials
    }

    /// <summary>
    /// 用户状态
    /// </summary>
    public enum UserStatus
    {

        /// <summary>
        /// 启用
        /// </summary>
        Active,


        /// <summary>
        /// 冻结
        /// </summary>
        Freeze,


        /// <summary>
        /// 关闭
        /// </summary>
        Close

    }

    /// <summary>
    /// 证件类型
    /// </summary>

    public enum DocumentType
    {
        Passport = 1,
        NationalID = 2,
        DriverLicense = 3
    }

    /// <summary>
    /// 进件状态
    /// </summary>
    public enum IdentityStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }


    public enum FormJosnType
    {
        /// <summary>
        /// 用户进件
        /// </summary>
        UserIdentity = 1,

        /// <summary>
        /// 账户进件
        /// </summary>
        VAAccountIdentity = 2
    }

    public enum OpenClose
    {
        Open = 0,
        Close = 1
    }


    public enum DataDicType
    {
        /// <summary>
        /// 
        /// </summary>
        Text,

        /// <summary>
        /// 
        /// </summary>
        Json
    }


    public enum CollectionOrderStatus
    {
        /// <summary>
        /// 待支付
        /// </summary>
        Pending,

        //成功
        Success,

        //失败
        Failed,

        //取消
        Cancelled
    }

    public enum CollectionOrderType
    {
        Direct = 1,
        VA
    }
     
     public enum MerchantPaySettingType
     {
        Direct = 1,
        VA = 2  
    }
}
