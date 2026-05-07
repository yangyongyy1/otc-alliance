namespace ClientPlatform
{
    public enum PayChannelServiceRequestStatus
    {
        PendingCustomer = 0,
        CustomerProcessing = 1,
        /// <summary>
        /// 客户已创建/存在，等待提交账户申请 (原 CustomerCreated)
        /// </summary>
        PendingAccount = 2,
        AccountProcessing = 3,
        Completed = 4,
        Failed = 5
    }
}
