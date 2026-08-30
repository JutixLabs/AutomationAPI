using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.MODEL.Interface
{
    public interface ISlackProvider
    {
        Task<List<SlackChannelDto>> GetChannelsAsync();
    }
}
