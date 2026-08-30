using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.SERVICES.Metadata
{
    public class ActionCatalog
    {
        public static List<ActionMetadataDto> GetActions(string provider)
        {
            return provider.ToLower() switch
            {
                "github" => GitHubActions(),

                "gmail" => GmailActions(),

                "slack" => SlackActions(),

                "discord" => DiscordActions(),

                "notion" => NotionActions(),

                "trello" => TrelloActions(),

                "stripe" => StripeActions(),

                //"googleforms" => GoogleFormsActions(),

                //"googlesheets" => GoogleSheetsActions(),

                _ => new()
            };
        }

        private static List<ActionMetadataDto> GitHubActions()
        {
            return new()
            {
                new()
                {
                    Name = "github.create_issue",

                    Label = "Create Issue",

                    Fields =
                    {
                        new()
                        {
                            Key = "repo",
                            Label = "Repository",
                            Type = "resource",
                            ResourceType = "repository"
                        },

                        new()
                        {
                            Key = "title",
                            Label = "Issue Title",
                            Type = "text"
                        },

                        new()
                        {
                            Key = "description",
                            Label = "Description",
                            Type = "textarea"
                        }
                    }
                },

                new()
                {
                    Name = "github.create_branch",

                    Label = "Create Branch",

                    Fields =
                    {
                        new()
                        {
                            Key = "repo",
                            Label = "Repository",
                            Type = "resource",
                            ResourceType = "repository"
                        },

                        new()
                        {
                            Key = "branchName",
                            Label = "Branch Name",
                            Type = "text"
                        }
                    }
                }
            };
        }

        private static List<ActionMetadataDto> GmailActions()
        {
            return new()
            {
                new()
                {
                    Name = "send_email",

                    Label = "Send Email",

                    Fields =
                    {
                        new()
                        {
                            Key = "to",
                            Label = "Recipient",
                            Type = "email"
                        },

                        new()
                        {
                            Key = "subject",
                            Label = "Subject",
                            Type = "text"
                        },

                        new()
                        {
                            Key = "body",
                            Label = "Message",
                            Type = "textarea"
                        }
                    }
                }
            };
        }

        private static List<ActionMetadataDto> SlackActions()
        {
            return new()
            {
                new()
                {
                    Name = "slack.send_message",

                    Label = "Send Message",

                    Fields =
                    {
                        new()
                        {
                            Key = "channel",
                            Label = "Channel",
                            Type = "resource",
                            ResourceType = "channel"
                        },

                        new()
                        {
                            Key = "message",
                            Label = "Message",
                            Type = "textarea"
                        }
                    }
                }
            };
        }

        private static List<ActionMetadataDto> DiscordActions()
        {
            return new()
            {
                // send message
                new()
                {
                    Name = "discord.send_message",

                    Label = "Send Message",

                    Fields = new()
                    {
                        new()
                        {
                            Key = "serverId",
                            Label = "Server",
                            Type = "resource",
                            ResourceType = "server",
                        },

                        new()
                        {
                            Key = "channelId",
                            Label = "Channel",
                            Type = "resource",
                            ResourceType = "channel",
                        },

                        new()
                        {
                            Key = "message",
                            Label = "Message",
                            Type = "textarea"
                        }
                    }
                },

                // create thread
                new()
                {
                    Name = "discord.create_thread",

                    Label = "Create Thread",

                    Fields = new()
                    {
                        new()
                        {
                            Key = "serverId",
                            Label = "Server",
                            Type = "resource",
                            ResourceType = "server"
                        },

                        new()
                        {
                            Key = "channelId",
                            Label = "Channel",
                            Type = "resource",
                            ResourceType = "channel",
                            DependsOn = "serverId"
                        },

                        new()
                        {
                            Key = "threadName",
                            Label = "Thread Name",
                            Type = "text"
                        }
                    }
                },

                // delete message
                new()
                {
                    Name = "discord.delete_message",

                    Label = "Delete Message",

                    Fields = new()
                    {
                        new()
                        {
                            Key = "serverId",
                            Label = "Server",
                            Type = "server"
                        },

                        new()
                        {
                            Key = "chanelId",
                            Label = "Channel",
                            Type = "channel",
                            DependsOn = "serverId",
                        },

                        new()
                        {
                            Key = "messageId",
                            Label = "Message ID"
                        }
                    }
                }
            };
        }

        private static List<ActionMetadataDto> NotionActions()
        {
            return new()
            {
                new ActionMetadataDto
                {
                    Name = "notion.create_page",
                    Label = "Create Page",
                    Fields = new List<FieldMetadataDto>
                    {
                        new()
                        {
                            Key = "databaseId",
                            Label = "Database",
                            Type = "resource",
                            ResourceType = "database"
                        },
                        new()
                        {
                            Key = "title",
                            Label = "Page Title",
                            Type = "text"
                        }
                    }
                }
            };
        }

        private static List<ActionMetadataDto> TrelloActions()
        {
            return new()
            {
                new ActionMetadataDto
                {
                    Name = "trello.create_card",
                    Label = "Create Card",
                    Fields = new List<FieldMetadataDto>
                    {
                        new() { Key = "boardId", Label = "Board", Type = "resource", ResourceType = "board" },
                        new() { Key = "listId", Label = "List", Type = "resource", ResourceType = "list", DependsOn = "boardId" },
                        new() { Key = "name", Label = "Card Name", Type = "text" },
                        new() { Key = "description", Label = "Description", Type = "textarea", Required = false }
                    }
                },
                new ActionMetadataDto
                {
                    Name = "trello.add_comment",
                    Label = "Add Comment",
                    Fields = new List<FieldMetadataDto>
                    {
                        new() { Key = "cardId", Label = "Card ID", Type = "text" },
                        new() { Key = "text", Label = "Comment", Type = "textarea" }
                    }
                }
            };
        }

        private static List<ActionMetadataDto> StripeActions()
        {
            return new()
            {
                new ActionMetadataDto
                {
                    Name = "stripe.create_customer",
                    Label = "Create Customer",
                    Fields = new List<FieldMetadataDto>
                    {
                        new() { Key = "email", Label = "Email", Type = "text" },
                        new() { Key = "name", Label = "Name", Type = "text", Required = false }
                    }
                },
                new ActionMetadataDto
                {
                    Name = "stripe.create_refund",
                    Label = "Refund Charge",
                    Fields = new List<FieldMetadataDto>
                    {
                        new() { Key = "chargeId", Label = "Charge ID", Type = "text" }
                    }
                }
            };
        }


        //private static List<ActionMetadataDto> GoogleFormsActions()
        //{
        //    return new()
        //    {
        //        new()
        //        {
        //            Name = "create_form",

        //            Label = "Create Form",

        //            Fields =
        //            {
        //                new()
        //                {
        //                    Key = "formId",
        //                    Label = "Form",
        //                    Type = "resource",
        //                    ResourceType = "form"
        //                }
        //            }
        //        }
        //    };
        //}

        //private static List<ActionMetadataDto> GoogleSheetsActions()
        //{
        //    return new()
        //    {
        //        new()
        //        {
        //            Name = "",

        //            Label = "",

        //            Fields =
        //            {
        //                new()
        //                {
        //                    Key = "channel",

        //                    Label = "Channel",

        //                    Type = "resource",

        //                    ResourceType = "channel" 
        //                }
        //            }
        //        }
        //    };
        //}
    }
}