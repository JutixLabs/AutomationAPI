using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.MODEL.Interface
{
    public interface IDiscordProvider
    {
        Task<List<DiscordGuildDto>> GetServersAsync();

        Task<List<DiscordChannelDto>> GetChannelsAsync(
                string guildId);

        Task SendMessageAsync(string channelId,string message);

        Task CreateThreadAsync(string channelId, string title);

        Task DeleteMessageAsync(string channelId, string messageId);
    }
}