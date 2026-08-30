using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;

namespace AutomationAPI.SERVICES.Resources
{
    public class TrelloResourceProvider : IResourceProvider
    {
        private readonly ITrelloProvider _trelloProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TrelloResourceProvider(ITrelloProvider trelloProvider, IHttpContextAccessor httpContextAccessor)
        {
            _trelloProvider = trelloProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public string ProviderName() => "trello";

        public async Task<List<ResourceOptionDto>> GetResourcesAsync(string resourceType, Dictionary<string, string> filters = null)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            switch (resourceType)
            {
                case "board":
                    {
                        var boards = await _trelloProvider.GetBoardsAsync(userId);
                        return boards.Select(b => new ResourceOptionDto { Id = b.Id, Name = b.Name }).ToList();
                    }

                case "list":
                    {
                        if (filters == null || !filters.TryGetValue("boardId", out var boardId))
                            return new List<ResourceOptionDto>();

                        var lists = await _trelloProvider.GetListsAsync(userId, boardId);
                        return lists.Select(l => new ResourceOptionDto { Id = l.Id, Name = l.Name }).ToList();
                    }

                default:
                    return new List<ResourceOptionDto>();
            }
        }
    }
}