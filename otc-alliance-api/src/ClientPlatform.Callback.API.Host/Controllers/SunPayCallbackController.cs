using Abp.AspNetCore.Mvc.Controllers;
using Abp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ClientPlatform.Pay.Channels;
using Castle.Core.Logging;
using Newtonsoft.Json;

namespace ClientPlatform.Callback.API.Host.Controllers
{
    [Route("api/webhooks/[controller]/[action]")]
    public class SunPayCallbackController : AbpController
    {
        private readonly SunPayChannelProvider _sunPayChannelProvider;
        private readonly SunPayLocalPayChannelProvider _sunPayLocalPayChannelProvider;

        public SunPayCallbackController(
            SunPayChannelProvider sunPayChannelProvider,
            SunPayLocalPayChannelProvider sunPayLocalPayChannelProvider)
        {
            _sunPayChannelProvider = sunPayChannelProvider;
            _sunPayLocalPayChannelProvider = sunPayLocalPayChannelProvider;
        }

        [HttpPost]
        [AllowAnonymous]
        [DontWrapResult]
        public async Task<IActionResult> VACustomerReceive()
        {
            try
            {
                Request.EnableBuffering();
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    Request.Body.Position = 0;

                    // Try to find signature in headers
                    var signature = Request.Headers["X-Signature"].FirstOrDefault()
                                    ?? Request.Headers["Signature"].FirstOrDefault()
                                    ?? string.Empty;

                    Logger.Info($"SunPayCallback=Received=Body==={body}=Signature={signature}");

                    await _sunPayChannelProvider.HandleCustomerCallbackAsync(body, signature);

                    // 返回 SunPay 要求的格式，避免重试
                    return Ok(new { is_success = "true", message = "success" });
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error("Error=handling=SunPay=callback", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [DontWrapResult]
        public async Task<IActionResult> VAAccountReceive()
        {
            try
            {
                Request.EnableBuffering();
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    Request.Body.Position = 0;

                    // Try to find signature in headers
                    var signature = Request.Headers["X-Signature"].FirstOrDefault()
                                    ?? Request.Headers["Signature"].FirstOrDefault()
                                    ?? string.Empty;

                    Logger.Info($"SunPayAccountCallback=Received=Body==={body}=Signature={signature}");

                    await _sunPayChannelProvider.HandleAccountCallbackAsync(body, signature);

                    // 返回 SunPay 要求的格式，避免重试
                    return Ok(new { is_success = "true", message = "success" });
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error("Error=handling=SunPay=Account=callback", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [DontWrapResult]
        public async Task<IActionResult> VACollectionReceive()
        {
            try
            {
                Request.EnableBuffering();
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    Request.Body.Position = 0;

                    // Try to find signature in headers
                    var signature = Request.Headers["X-Signature"].FirstOrDefault()
                                    ?? Request.Headers["Signature"].FirstOrDefault()
                                    ?? string.Empty;

                    Logger.Info($"SunPayCollectionCallback=Received=Body==={body}=Signature={signature}");

                    await _sunPayChannelProvider.HandleCollectionCallbackAsync(body, signature);

                    return Ok(new { is_success = "true", message = "success" });
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error("Error=handling=SunPay=Collection=callback", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [DontWrapResult]
        public async Task<IActionResult> VACollectionSettlementReceive()
        {
            try
            {
                Request.EnableBuffering();
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    Request.Body.Position = 0;

                    // Try to find signature in headers
                    var signature = Request.Headers["X-Signature"].FirstOrDefault()
                                    ?? Request.Headers["Signature"].FirstOrDefault()
                                    ?? string.Empty;

                    Logger.Info($"SunPayCollectionSettlementCallback=Received=Body==={body}=Signature={signature}");

                    await _sunPayChannelProvider.HandleCollectionSettlementCallbackAsync(body, signature);

                    return Ok(new { is_success = "true", message = "success" });
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error("Error=handling=SunPay=CollectionSettlement=callback", ex);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// SunPay 本地支付（PAYIN 收银台）回调
        /// 用于更新本地入金订单状态（CollectionOrder）
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [DontWrapResult]
        public async Task<IActionResult> LocalPayReceive()
        {
            try
            {
                Request.EnableBuffering();
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    Request.Body.Position = 0;

                    // Try to find signature in headers
                    var signature = Request.Headers["X-Signature"].FirstOrDefault()
                                    ?? Request.Headers["Signature"].FirstOrDefault()
                                    ?? string.Empty;

                    Logger.Info($"SunPayLocalPayCallback=Received=Body==={body}=Signature={signature}");

                    // 调用 SunPay 本地支付 ChannelProvider，内部会根据 biz_status 更新 CollectionOrder.OrderStatus
                    await _sunPayLocalPayChannelProvider.HandlePayinCallbackAsync(body, signature);

                    // 按 SunPay 要求返回固定格式，避免重复重试
                    return Ok(new { is_success = "true", message = "success" });
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error("Error=handling=SunPay=LocalPay=callback", ex);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
