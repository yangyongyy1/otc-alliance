using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Extensions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ClientPlatform.Authorization.Users;

public class User : AbpUser<User>
{
    /// <summary>初始化种子数据使用的默认密码（仅首次建库生效，生产环境部署后请修改）</summary>
    public const string DefaultPassword = "V@Pay#Init2025!Sec";


    public SystemUserType UserType { get; set; }

    public int? TimeZoneValue { get; set; }
    public static string CreateRandomPassword()
    {
        const int requiredLength = 6;

        const string digits = "0123456789";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string special = "!@#$%^&*()_-+=<>?{}[]|";

        string all = digits + lower + upper + special;

        var password = new StringBuilder();
        var rng = RandomNumberGenerator.Create();

        // 确保至少包含各类字符
        password.Append(GetRandomChar(digits, rng));
        password.Append(GetRandomChar(lower, rng));
        password.Append(GetRandomChar(upper, rng));
        password.Append(GetRandomChar(special, rng));

        // 补齐长度
        while (password.Length < requiredLength)
        {
            password.Append(GetRandomChar(all, rng));
        }

        // 打乱顺序
        return Shuffle(password.ToString(), rng);
    }


    private static string Shuffle(string input, RandomNumberGenerator rng)
    {
        var array = input.ToCharArray();

        for (int i = array.Length - 1; i > 0; i--)
        {
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            int j = Math.Abs(BitConverter.ToInt32(bytes, 0) % (i + 1));

            (array[i], array[j]) = (array[j], array[i]);
        }

        return new string(array);
    }

    private static char GetRandomChar(string source, RandomNumberGenerator rng)
    {
        byte[] bytes = new byte[4];
        rng.GetBytes(bytes);
        int index = Math.Abs(BitConverter.ToInt32(bytes, 0) % source.Length);
        return source[index];
    }

    public static User CreateTenantAdminUser(int tenantId, string emailAddress)
    {
        var user = new User
        {
            TenantId = tenantId,
            UserName = AdminUserName,
            Name = AdminUserName,
            Surname = AdminUserName,
            EmailAddress = emailAddress,
            Roles = new List<UserRole>()
        };

        user.SetNormalizedNames();

        return user;
    }
}
