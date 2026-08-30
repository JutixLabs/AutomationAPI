using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;
using Hangfire;
using Hangfire.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace AutomationAPI.SERVICES
{
    public class AutomationService : IAutomationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly ILogger<AutomationService> _logger;
        private readonly IActionExecutorFactory _executorFactory;
        public AutomationService(AppDbContext dbContext, IEmailService emailService, IHttpContextAccessor httpContextAccessor, 
            ILogger<AutomationService> logger, IActionExecutorFactory executorFactory)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _executorFactory = executorFactory;
        }
        public async Task<AutomationRule> CreatRuleAsync(CreateWorkFlow rule)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            try
            {
                var trigger = rule.Trigger.ToLower().Trim();
                var actions = rule.Steps
                    .Select(r => r.Action.ToLower().Trim())
                    .ToList();

                // Load matching trigger rules into memory first
                var existingRules = await _dbContext.AutomationRules
                    .Include(r => r.Steps)
                    .Where(r => r.Trigger.ToLower().Trim() == trigger)
                    .ToListAsync();

                // Compare actions in memory
                var validateRule = existingRules.FirstOrDefault(r =>
                    r.Steps
                        .OrderBy(s => s.Order)
                        .Select(s => s.Action.ToLower().Trim())
                        .SequenceEqual(actions));
                if (validateRule != null)
                    throw new Exception("Rule already exists.");

                var newRule = new AutomationRule
                {
                    Trigger = rule.Trigger,
                    UserID = userId,
                    IsWorkflow = true,
                    Steps = rule.Steps.Select(s => new WorkFlowStep
                    {
                        Order = s.Order,
                        Action = s.Action,
                        Target = s.Target,
                        ConfigurationJson = s.ConfigurationJson
                    }).ToList(),

                    CreatedAt = DateTime.Now,
                    WebhookKey = Guid.NewGuid().ToString()
                };
                await _dbContext.AutomationRules.AddAsync(newRule);
                await _dbContext.SaveChangesAsync();

                return newRule;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }

        public async Task<object> GetAllRulesAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            try
            {
                var rules = await _dbContext.AutomationRules
                    .Where(u => u.UserID == userId)
                     .Select(r => new
                     {
                         id = r.ID,
                         trigger = r.Trigger,
                         isActive = r.IsActive,
                         createdAt = r.CreatedAt,
                         steps = r.Steps
                            .OrderBy(s => s.Order)
                            .Select(s => new
                            {
                                id = s.Id,
                                order = s.Order,
                                action = s.Action,
                                target = s.Target
                            }).ToList(),
                         webhookKey = r.WebhookKey
                     })
                    .ToListAsync();
                if (rules == null)
                    throw new Exception("No rules found.");
                

                return rules;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }
        
        public async Task<List<ExecutionLog>> ExecuteTrigger(string trigger)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            trigger = trigger.ToLower().Trim();
            try
            {
                var rules = await _dbContext.AutomationRules
                        .Where(
                            r => r.Trigger.ToLower() == trigger 
                            && r.UserID == userId 
                            && r.IsActive)
                        .Include(r => r.Steps)
                        .ToListAsync();

                if (!rules.Any())
                    throw new Exception("No active rules found for the trigger.");

                var logs = new List<ExecutionLog>();
                foreach (var rule in rules)
                {
                    foreach (var step in rule.Steps.OrderBy(s => s.Order))
                    {
                        try
                        {
                            var payload =
                                new Dictionary<string, object>
                                {
                                    {
                                        "message",
                                        $"Trigger executed: {trigger}"
                                    }
                                };

                            var executor =
                                _executorFactory
                                    .GetExecutor(
                                        step.Action);

                            await executor.ExecuteAsync(
                                rule.UserID,
                                step,
                                payload);

                            logs.Add(new ExecutionLog
                            {
                                UserId = rule.UserID,
                                Trigger = rule.Trigger,
                                Action = string.Join(", ", rule.Steps.Select(a => a.Action)),
                                Target = string.Join(", ", rule.Steps.Select(a => a.Target)),
                                Status = "Success"
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"[ERROR] {ex.Message}");
                            logs.Add(new ExecutionLog
                            {
                                UserId = rule.UserID,
                                Trigger = rule.Trigger,
                                Action = string.Join(", ", rule.Steps.Select(a => a.Action)),
                                Target = string.Join(", ", rule.Steps.Select(a => a.Target)),
                                Status = "Failed",
                                ErrorMessage = ex.Message
                            });
                        } 
                    }

                }
                try
                {
                    await _dbContext.ExecutionLogs.AddRangeAsync(logs);
                    var rows = await _dbContext.SaveChangesAsync();

                    _logger.LogInformation($"[INFO] {rows} ExecutionLog(s) added to the database.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[DATABASE ERROR]  {ex.Message}");
                }

                _logger.LogInformation("[INFO] ExecutionLog added successfully");

                return logs; 
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }

        public async Task ExecuteAction(int ruleId)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            var user = await _dbContext.Users.FindAsync(userId);
            try
            {
                int maxRetries = 3;
                int attempt = 0;
                bool success = false;
                string errorMessage = null;

                var rule = await _dbContext.AutomationRules.FindAsync(ruleId);
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "SERVICES\\Email Service\\Templates", "Notification.html");
                var emailBody = _emailService.LoadEmailTemplate(templatePath, new Dictionary<string, string>
                {
                   { "FullName",  user.FullName},
                    { "TriggerName", rule.Trigger},
                     { "ActionName", string.Join(", ", rule.Steps.Select(a => a.Action)) },
                    {"Message", "Your automation executed successfully."}
                });
                
                while (attempt < maxRetries && !success)
                {
                    try
                    {
                        attempt++;
                        foreach (var step in rule.Steps.OrderBy(s => s.Order))
                        {
                            switch (step.Action)
                            {
                                case "send_email":
                                    await _emailService.SendEmailAsync(step.Target, "Smart workflow notifications", emailBody);
                                    break;

                                case "webhook":
                                    using (var client = new HttpClient())
                                    {
                                        var payload = new
                                        {
                                            Trigger = rule.Trigger,
                                            Action = step.Action,
                                            Timestamp = DateTime.UtcNow
                                        };

                                        var json = JsonSerializer.Serialize(payload);

                                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                                        var response = await client.PostAsync(step.Target, content);

                                        response.EnsureSuccessStatusCode();
                                    }
                                    break;

                                case "log":
                                    _logger.LogInformation($"[LOG] {step.Target}");
                                    break;
                            }
                        }

                        success = true;
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.Message;
                        _logger.LogError($"Retry {attempt} failed: {ex.Message}");

                        if (attempt < maxRetries)
                        {
                            await Task.Delay(2000); // wait before retry
                        }
                    }
                }

                await _dbContext.ExecutionLogs.AddAsync(new ExecutionLog
                {
                    Trigger = rule.Trigger,
                    Action = string.Join(", ", rule.Steps.Select(a => a.Action)),
                    Target = string.Join(", ", rule.Steps.Select(a => a.Target)),
                    Status = success ? "Success" : "Failed",
                    RetryCount = attempt,
                    ErrorMessage = errorMessage
                });

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }

        public async Task<List<AutomationRule>> DeleteAllRulesAsync()
        {
            try
            {
                var rules = await _dbContext.AutomationRules.ToListAsync();
                if (rules == null || rules.Count == 0)
                    throw new Exception("No rules to delete.");

                _dbContext.AutomationRules.RemoveRange(rules);
                await _dbContext.SaveChangesAsync();

                return rules;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteByIdAsync(int id)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            try
            {
                var rule = await _dbContext.AutomationRules.FirstOrDefaultAsync(r => r.ID == id && r.UserID == userId);
                if (rule == null)
                    throw new Exception("Rule Not Found.");

                _dbContext.AutomationRules.Remove(rule);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ToggleRuleAsync(int id)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            try
            {
                var rule = await _dbContext.AutomationRules.FirstOrDefaultAsync(r => r.ID == id && r.UserID == userId);

                if (rule == null)
                    throw new Exception("Rule Not Found.");

                rule.IsActive = !rule.IsActive;

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }
    }
}
