using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace AutomationAPI.Controllers
{
    [Route("api/Webhooks/stripe")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ITriggerEngineService _triggerEngine;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(ITriggerEngineService triggerEngine, AppDbContext dbContext,
            IConfiguration config, ILogger<StripeWebhookController> logger)
        {
            _triggerEngine = triggerEngine;
            _dbContext = dbContext;
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            if (!Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeader))
                return BadRequest("Missing Stripe-Signature header.");

            if (!VerifySignature(payload, signatureHeader.ToString()))
            {
                _logger.LogError("[ERROR] Stripe webhook signature verification failed.");
                return BadRequest("Invalid signature.");
            }

            dynamic evt = JsonConvert.DeserializeObject(payload);
            string eventType = evt.type;
            string connectedAccountId = evt.account; // present on Connect events

            var connectedApp = await _dbContext.ConnectedApps
                .FirstOrDefaultAsync(x => x.Provider == "stripe" && x.ExternalAccountId == connectedAccountId);

            if (connectedApp == null)
            {
                _logger.LogInformation("[INFO] Stripe webhook received for unknown account: {Account}", connectedAccountId);
                return Ok(); // Still 200 — Stripe retries on non-2xx.
            }

            var triggerName = eventType switch
            {
                "payment_intent.succeeded" => "stripe.payment_succeeded",
                "charge.refunded" => "stripe.charge_refunded",
                _ => null
            };

            if (triggerName != null)
            {
                await _triggerEngine.ExecuteTriggerAsync(new TriggerEvent
                {
                    TriggerName = triggerName,
                    UserId = connectedApp.UserId,
                    Payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(payload)
                });
            }

            return Ok();
        }

        private bool VerifySignature(string payload, string signatureHeader)
        {
            var webhookSecret = _config["StripeConnect:WebhookSecret"];

            var parts = signatureHeader.Split(',')
                .Select(p => p.Split('='))
                .ToDictionary(p => p[0], p => p[1]);

            if (!parts.TryGetValue("t", out var timestamp) || !parts.TryGetValue("v1", out var signature))
                return false;

            var signedPayload = $"{timestamp}.{payload}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
            var computedSignature = Convert.ToHexString(computedHash).ToLower();

            return computedSignature == signature;
        }
    }
}