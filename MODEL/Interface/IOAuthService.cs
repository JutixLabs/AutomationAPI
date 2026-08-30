//using Google.Apis.Auth.OAuth2.Responses;

using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.MODEL.Interface
{
    public interface IOAuthService
    {
        string Google();
        Task<TokenResponse> GoogleCallBack(string code);
        string Slack();
        Task SlackCallback(string code, string userId);
        string GitHub();
        Task GitHubCallback(string code, string state);
        string Discord();
        Task DiscordCallbackAsync(string code, string userId);
        string Notion();
        Task NotionCallbackAsync(string code, string userId);
        string Trello();
        Task ConnectTrelloAsync(string userId, string token);
        string Stripe();
        Task StripeCallbackAsync(string code, string userId);
    }
}
