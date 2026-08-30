namespace AutomationAPI.MODEL.DTO
{
    public class SlackChannelsResponse
    {
        public bool Ok { get; set; }

        public List<SlackChannelResponse> Channels { get; set; }
    }

    public class SlackChannelResponse
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
