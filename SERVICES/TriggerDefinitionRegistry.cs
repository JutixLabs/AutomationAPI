using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.SERVICES
{
    public class TriggerDefinitionRegistry
    {
        public List<TriggerDefinition> GetTriggers()
        {
            return new List<TriggerDefinition>
            {
                // For system
                new TriggerDefinition
                {
                    Name = "user_registered",
                    DisplayName = "User Registered",
                    Provider = "system",
                    Description = "Triggered when a new user signs up.",
                    RequiresConnection = false,
                    PayloadFields =new List<TriggerField>
                    {
                        new TriggerField { Name = "email", Label = "Email", Type = "string" },
                        new TriggerField { Name = "fullName", Label = "Full Name", Type = "string" }
                    }
                },

                // For Gmail
                new TriggerDefinition
                {
                    Name = "gmail.new_email",
                    DisplayName = "New Gmail Email",
                    Provider = "gmail",
                    Description = "Triggered when a new email is received in Gmail.",
                    RequiresConnection = true,
                    PayloadFields = new List<TriggerField>
                    {
                        new TriggerField { Name= "from", Label = "Sender", Type = "string" },
                        new TriggerField { Name= "subject", Label = "Subject", Type = "string" },
                        new TriggerField { Name= "body", Label = "Body", Type = "string" }
                    }
                },

                // For Slack
                new TriggerDefinition
                {
                    Name = "slack.new_message",
                    DisplayName = "New Slack Message",
                    Provider = "slack",
                    Description = "Triggered when a new Slack message is received.",
                    RequiresConnection= true,
                    PayloadFields = new List<TriggerField>
                    {
                        new TriggerField { Name = "channel", Label = "Channel", Type = "string" },
                        new TriggerField { Name = "message", Label = "Message", Type = "string" },
                        new TriggerField { Name = "user", Label = "User", Type = "string" }
                    }
                },

                // For GitHub — names match GitHub's real webhook payload shape exactly
                // (issue.* and repository.* are nested objects, not top-level fields),
                // so the resolver's dot-path lookup can actually find them.
                new TriggerDefinition
                {
                    Name = "github.issue_created",
                    DisplayName = "GitHub Issue Created",
                    Provider = "github",
                    Description = "Triggered when a new issue is created in GitHub.",
                    RequiresConnection = true,
                    PayloadFields = new List<TriggerField>
                    {
                        new TriggerField { Name = "issue.number", Label = "Issue Number", Type = "string" },
                        new TriggerField { Name = "issue.title", Label = "Issue Title", Type = "string" },
                        new TriggerField { Name = "issue.html_url", Label = "Issue URL", Type = "string" },
                        new TriggerField { Name = "repository.full_name", Label = "Repository", Type = "string" },
                    }
                },

                // For Google Forms
                new TriggerDefinition
                {
                    Name = "google_forms.new_response",
                    DisplayName = "New Form Response",
                    Provider = "google_forms",
                    Description = "Triggered when form is submitted.",
                    RequiresConnection = true,
                    PayloadFields = new List<TriggerField>
                    {
                        new TriggerField { Name = "responseId", Label = "Response Id", Type = "string" },
                        new TriggerField { Name = "formId", Label = "Form Id", Type = "string" },
                        new TriggerField { Name = "submittedAt", Label = "Submitted At", Type = "string" }
                    }
                },

                // Scheduler triggers
                new TriggerDefinition
                {
                    Name = "schedule.every_day",
                    DisplayName = "Every Day",
                    Provider = "schedule",
                    Description = "Runs every day at a specified time.",
                    RequiresConnection = false,
                    PayloadFields = new List<TriggerField>()
                },

                new TriggerDefinition
                {
                    Name = "stripe.payment_succeeded",
                    DisplayName = "Payment Succeeded",
                    Provider = "stripe",
                    Description = "Fires when a payment completes successfully.",
                    RequiresConnection = true,
                    PayloadFields = new List<TriggerField>
                    {
                        new TriggerField { Name = "data.object.id", Label = "Payment Intent ID", Type = "string" },
                        new TriggerField { Name = "data.object.amount", Label = "Amount", Type = "string" },
                        new TriggerField { Name = "data.object.currency", Label = "Currency", Type = "string" },
                        new TriggerField { Name = "data.object.customer", Label = "Customer ID", Type = "string" },
                    }
                },

                new TriggerDefinition
                {
                    Name = "notion.page_updated",
                    DisplayName = "Notion Page Updated",
                    Provider = "notion",
                    Description = "Fires when a page's content or properties change.",
                    RequiresConnection = true,
                    PayloadFields = new List<TriggerField>
                    {
                        new TriggerField { Name = "entity.id", Label = "Page ID", Type = "string" },
                        new TriggerField { Name = "type", Label = "Event Type", Type = "string" },
                        new TriggerField { Name = "workspace_name", Label = "Workspace", Type = "string" },
                    }
                },
                new TriggerDefinition
                {
                    Name = "trello.card_created",
                    DisplayName = "Trello Card Created",
                    Provider = "trello",
                    Description = "Fires when a new card is added to a watched board.",
                    RequiresConnection = true,
                    PayloadFields = new List<TriggerField>
                    {
                        new TriggerField { Name = "action.data.card.name", Label = "Card Name", Type = "string" },
                        new TriggerField { Name = "action.data.card.id", Label = "Card ID", Type = "string" },
                        new TriggerField { Name = "action.data.list.name", Label = "List", Type = "string" },
                    }
                },
                new TriggerDefinition
                {
                    Name = "trello.card_moved",
                    DisplayName = "Trello Card Moved",
                    Provider = "trello",
                    Description = "Fires when a card is moved to a different list.",
                    RequiresConnection = true,
                    PayloadFields = new List<TriggerField>
                    {
                        new TriggerField { Name = "action.data.card.name", Label = "Card Name", Type = "string" },
                        new TriggerField { Name = "action.data.listBefore.name", Label = "From List", Type = "string" },
                        new TriggerField { Name = "action.data.listAfter.name", Label = "To List", Type = "string" },
                    }
                },
                new TriggerDefinition
                {
                    Name = "trello.comment_added",
                    DisplayName = "Trello Comment Added",
                    Provider = "trello",
                    Description = "Fires when someone comments on a card.",
                    RequiresConnection = true,
                    PayloadFields = new List<TriggerField>
                    {
                        new TriggerField { Name = "action.data.text", Label = "Comment Text", Type = "string" },
                        new TriggerField { Name = "action.data.card.name", Label = "Card Name", Type = "string" },
                    }
                },

                new TriggerDefinition
                {
                    Name = "notion.comment_added",
                    DisplayName = "Notion Comment Added",
                    Provider = "notion",
                    Description = "Fires when a comment is added to a page.",
                    RequiresConnection = true,
                    PayloadFields = new List<TriggerField>
                    {
                        new TriggerField { Name = "entity.id", Label = "Page ID", Type = "string" },
                        new TriggerField { Name = "workspace_name", Label = "Workspace", Type = "string" },
                    }
                },

                new TriggerDefinition
                {
                    Name = "stripe.charge_refunded",
                    DisplayName = "Charge Refunded",
                    Provider = "stripe",
                    Description = "Fires when a charge is refunded.",
                    RequiresConnection = true,
                    PayloadFields = new List<TriggerField>
                    {
                        new TriggerField { Name = "data.object.id", Label = "Charge ID", Type = "string" },
                        new TriggerField { Name = "data.object.amount_refunded", Label = "Amount Refunded", Type = "string" },
                        new TriggerField { Name = "data.object.customer", Label = "Customer ID", Type = "string" },
                    }
                },

                new TriggerDefinition
                {
                    Name = "schedule.every_hour",
                    DisplayName = "Every Hour",
                    Provider = "schedule",
                    Description = "Runs once every hour.",
                    RequiresConnection = false,
                    PayloadFields = new List<TriggerField>()
                }
            };
        }
    }
}
