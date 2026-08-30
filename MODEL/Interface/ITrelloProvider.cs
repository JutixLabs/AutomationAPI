using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.MODEL.Interface
{
    public interface ITrelloProvider
    {
        Task<List<TrelloBoardDto>> GetBoardsAsync(string userId);
        Task<List<TrelloListDto>> GetListsAsync(string userId, string boardId);
        Task CreateCardAsync(string userId, string listId, string name, string description);
        Task AddCommentAsync(string userId, string cardId, string text);

        Task<string> CreateWebhookAsync(string userId, string boardId, string callbackUrl);
        Task DeleteWebhookAsync(string userId, string webhookId);
    }
}
