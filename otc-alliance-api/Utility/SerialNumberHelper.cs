using System;
using System.Security.Cryptography;
using System.Text;

public static class SerialNumberHelper
{
    /// <summary>
    /// 生成指定长度的序列号，不包含 '-' ，返回结果自动每4位插入 '-'
    /// </summary>
    /// <param name="length">原始字符长度（不含分隔符）</param>
    /// <returns>格式化后的序列号</returns>
    public static string GenerateSerial(int length)
    {
        string raw = GenerateRandomString(length);
        return InsertDashEvery4(raw);
    }

    /// <summary>
    /// 生成随机字符串（大写字母+数字）
    /// </summary>
    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        StringBuilder sb = new StringBuilder(length);

        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] buffer = new byte[1];

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                int index = buffer[0] % chars.Length;
                sb.Append(chars[index]);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 每4个字符插入一个 '-'
    /// </summary>
    private static string InsertDashEvery4(string input)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            if (i > 0 && i % 4 == 0)
                sb.Append('-');

            sb.Append(input[i]);
        }

        return sb.ToString();
    }
}
