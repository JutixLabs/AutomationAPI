using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AutomationAPI.MODEL.Interface
{
    public interface IWebhookService
    {
        Task ReceiveAsync(string key, JsonElement payload);
        Task ExecuteRuleAsync(int ruleId, JsonElement payload);
    }
}
