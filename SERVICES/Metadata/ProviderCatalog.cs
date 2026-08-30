using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.SERVICES.Metadata
{
    public class ProviderCatalog
    {
        public static List<ProviderMetadataDTO> Providers =>
        new()
        {
            new()
            {
                Name = "gmail",
                Label = "Gmail",
                Icon = "📧"
            },

            new()
            {
                Name = "slack",
                Label = "Slack",
                Icon = "💬"
            },

            new()
            {
                Name = "github",
                Label = "GitHub",
                Icon = "🐙"
            },

            //new()
            //{
            //    Name = "googleforms",
            //    Label = "Google Forms",
            //    Icon = "📋"
            //},

            //new()
            //{
            //    Name = "googlesheets",
            //    Label = "Google Sheets",
            //    Icon = "📊"
            //},

            //new()
            //{
            //    Name = "teams",
            //    Label = "Microsoft Teams",
            //    Icon = "👥"
            //},

            new()
            {
                Name = "notion",
                Label = "Notion",
                Icon = "📝"
            },

            new()
            {
                Name = "trello",
                Label = "Trello",
                Icon = "📌"
            },

            //new()
            //{
            //    Name = "shopify",
            //    Label = "Shopify",
            //    Icon = "🛒"
            //},

            new()
            {
                Name = "stripe",
                Label = "Stripe",
                Icon = "💳"
            },

            new()
            {
                Name = "discord",
                Label = "Discord",
                Icon = "🎮"
            },

            //new()
            //{
            //    Name = "hubspot",
            //    Label = "HubSpot",
            //    Icon = "📈"
            //},

            //new()
            //{
            //    Name = "googledrive",
            //    Label = "Google Drive",
            //    Icon = "☁️"
            //},

            //new()
            //{
            //    Name = "onedrive",
            //    Label = "OneDrive",
            //    Icon = "☁️"
            //},

            //new()
            //{
            //    Name = "openai",
            //    Label = "OpenAI",
            //    Icon = "🤖"
            //},

            //new()
            //{
            //    Name = "claude",
            //    Label = "Claude",
            //    Icon = "🧠"
            //}
        };
    }
}