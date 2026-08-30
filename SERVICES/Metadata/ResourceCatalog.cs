using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.SERVICES.Metadata
{
    public class ResourceCatalog
    {
        public static List<ResourceMetadataDto> GetResources(string provider)
        {
            return provider.ToLower() switch
            {
                "github" => GitHubResources(),

                "slack" => SlackResources(),


                _ => new()
            };
        }

        private static List<ResourceMetadataDto> GitHubResources()
        {
            return new()
            {
                new()
                {
                    Name = "repository",

                    Label = "Repository"
                },

                new()
                {
                    Name = "branch",

                    Label = "Branch"
                },

                new()
                {
                    Name = "issue",

                    Label = "Issue"
                }
            };
        }

        private static List<ResourceMetadataDto> SlackResources()
        {
            return new()
            {
                new()
                {
                    Name = "channel",

                    Label = "Channel"
                }
            };
        }

        //private static List<ResourceMetadataDto> GoogleFormsResources()
        //{
        //    return new()
        //    {
        //        new()
        //        {
        //            Name = "form",

        //            Label = "Form",

        //            Endpoint =
        //            "/api/Resources/googleforms/forms"
        //        }
        //    };
        //}

        //private static List<ResourceMetadataDto> GoogleSheetsResources()
        //{
        //    return new()
        //    {
        //        new()
        //        {
        //            Name = "sheet",

        //            Label = "Spreadsheet",

        //            Endpoint =
        //            "/api/Resources/googlesheets/sheets"
        //        }
        //    };
        //}
    }
}
