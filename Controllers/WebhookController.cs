using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IWebhookService _webhookService;
        public WebhookController(IWebhookService webhookService)
        {
            _webhookService = webhookService;
        }

        [HttpPost("{key}")]
        public async Task<IActionResult> ReceiveWebhook(string key, [FromBody] JsonElement payload)
        {
            await _webhookService.ReceiveAsync(key, payload);
            return Ok(new
            {
                success = true,
                recievedAt = DateTime.Now,
            });
        }
    }
}
