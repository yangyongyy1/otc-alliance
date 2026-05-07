using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class NECaptchaValidateHelper
    {
        private NECaptchaValidateOptions _options;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public NECaptchaValidateHelper(IOptions<NECaptchaValidateOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// 验证前端图片拖拽结果
        /// </summary>
        /// <param name="validate"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> Verify(string validate)
        {
            return await Verify(validate, "");
        }

        /// <summary>
        /// 验证前端图片拖拽结果
        /// </summary>
        /// <param name="validate"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> Verify(string validate, string user)
        {
            if (string.IsNullOrEmpty(validate))
            {
                return false;
            }
            if (string.IsNullOrEmpty(user))
            {
                user = "";
            }
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            string time = curr.ToString();
            parameters.Add("captchaId", _options.CaptchaID);
            parameters.Add("validate", validate);
            parameters.Add("user", user);

            parameters.Add("secretId", _options.ApiKey);
            parameters.Add("version", "v2");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());
            parameters.Add("signature", GenSignature(_options.ApiSecret, parameters));
            var result = await DoPostNEApi(_options.VerifyApi, parameters);
            return result;
        }

        /// <summary>
        /// 加密参数
        /// </summary>
        /// <param name="apiSecret"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string GenSignature(string apiSecret, Dictionary<string, string> parameters)
        {
            parameters = parameters.OrderBy(o => o.Key, StringComparer.Ordinal).ToDictionary(o => o.Key, p => p.Value);
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in parameters)
            {
                builder.Append(kv.Key).Append(kv.Value);
            }
            builder.Append(apiSecret);
            string tmp = builder.ToString();
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(tmp));
            builder.Clear();
            foreach (byte b in result)
            {
                builder.Append(b.ToString("x2").ToLower());
            }
            return builder.ToString();
        }

        /// <summary>
        /// 调用网易易盾接口验证前端
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private async Task<bool> DoPostNEApi(string url, Dictionary<string, string> parameters)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Connection.Add("keep-alive");
                HttpContent content = new FormUrlEncodedContent(parameters);
                var apiResponse = await client.PostAsync(url, content);
                if (apiResponse.IsSuccessStatusCode)
                {
                    var response = await apiResponse.Content.ReadAsStringAsync();
                    JObject j = JObject.Parse(response);
                    return j.GetValue("result").ToObject<bool>();

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }
    }

    /// <summary>
    /// 配置参数
    /// </summary>
    public class NECaptchaValidateOptions
    {
        /// <summary>
        /// 网易易盾验证网关
        /// </summary>
        public string VerifyApi { get; set; }
        /// <summary>
        /// 应用CaptchaID
        /// </summary>
        public string CaptchaID { get; set; }
        /// <summary>
        /// 应用Key
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// 应用密钥
        /// </summary>
        public string ApiSecret { get; set; }
    }
}
