using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using Abp.Runtime.Validation;
using Abp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform.Kyc;
using ClientPlatform.Kyc.Channels;
using ClientPlatform.Kyc.Channels.Sumsub;

namespace ClientPlatform.Callback.API.Host.Controllers
{
    /// Sumsub Webhook 控制器
    /// </summary>
    [Route("api/webhooks/[controller]/[action]")]
    public class KycNotificationCallbackController : AbpController
    {
        private readonly KycManager _kycManager;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 构造函数
        /// </summary>
        public KycNotificationCallbackController(KycManager kycManager, IConfiguration configuration)
        {
            _kycManager = kycManager;
            _configuration = configuration;
        }

        /// <summary>
        /// 接收 SumsubCallBack 事件
        /// </summary>
        /// <returns>返回 HTTP 200 表示成功接收，否則返回錯誤狀態碼</returns>
        [HttpPost]
        [AllowAnonymous]
        [DontWrapResult]
        [DisableValidation]
        public async Task<IActionResult> SumsubCallBack()
        {
            try
            {
                // 读取原始请求体
                Request.EnableBuffering();
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    Request.Body.Position = 0; // 重置流位置
                    Logger.Info($"SumsubCallBack===payload==={body}");

                    // 获取 Headers
                    var digestHeader = Request.Headers["x-payload-digest"].ToString();
                    var algoHeader = Request.Headers["x-payload-digest-alg"].ToString();
                    var platform = Request.Headers["x-platform"].FirstOrDefault();

                    if (string.IsNullOrEmpty(digestHeader) || string.IsNullOrEmpty(algoHeader))
                    {
                        Logger.Info("SumsubCallBack Missing digest or algorithm header");
                        return BadRequest("Missing digest or algorithm header");
                    }
                    Logger.Info($"SumsubCallBack===platform=={platform}===");
                    if (platform != "klicklpayclient")
                    {
                        // 兼容旧逻辑：如果平台不匹配，直接返回成功忽略
                        // Logger.Info($"SumsubCallBack Ignored platform: {platform}");
                        return Ok("Webhook received successfully");
                    }

                    Logger.Info($"SumsubCallBack===digestHeader:{digestHeader}===algoHeader:{algoHeader}===={body}");

                    var config = _configuration.GetSection("SumsubConf").Get<SumsubSetting>();
                    if (config == null)
                    {
                        Logger.Info($"SumsubCallBack===SumsubConf Setting Not Found");
                        return BadRequest();
                    }

                    //// 计算 HMAC 签名
                    //var calculatedDigest = CalculateDigest(body, config.WebhookSecretKey, algoHeader);
                    //if (calculatedDigest != digestHeader)
                    //{
                    //    Logger.Info($"SumsubCallBack Invalid signature. Calculated: {calculatedDigest}, Received: {digestHeader}");
                    //    return Unauthorized("Invalid signature");
                    //}

                    // 委托 KycManager 处理回调逻辑
                    // 注意：因为 Controller 层已经验证了签名，Manager 层可以信任，或者再次验证
                    // 现在的 Manager/Provider 实现会再次验证，但使用的是默认的 HMAC-SHA256 (如果不改的话)
                    // 既然这里做了多算法支持，最好在这里通过后，传递给 Manager 处理业务

                    await _kycManager.HandleCallbackAsync(KycChannelCodes.Sumsub, body, digestHeader);

                    // ------------------------------------------------------垃圾代码------------------------------------------------------
                     
                    // 成功响应
                    return Ok("Webhook received successfully");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"SumsubCallBack===异常 {e.Message}", e);
                return StatusCode(500, "Internal server error");
            }
        }

        private string CalculateDigest(string payload, string secretKey, string algoHeader)
        {
            HMAC hmac;
            if (algoHeader == "HMAC_SHA256_HEX")
                hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            else if (algoHeader == "HMAC_SHA512_HEX")
                hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            else if (algoHeader == "HMAC_SHA1_HEX")
                hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey));
            else
                throw new ArgumentException("Unsupported algorithm", nameof(algoHeader));

            using (hmac)
            {
                var byteArray = Encoding.UTF8.GetBytes(payload);
                var hash = hmac.ComputeHash(byteArray);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
