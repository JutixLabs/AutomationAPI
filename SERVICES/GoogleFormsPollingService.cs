//using AutomationAPI.DATA;
//using AutomationAPI.MODEL.DTO;
//using AutomationAPI.MODEL.Entity;
//using AutomationAPI.MODEL.Interface;
//using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//namespace AutomationAPI.SERVICES
//{
//    public class GoogleFormsPollingService
//    {
//        private readonly AppDbContext _dbContext;
//        private readonly ITriggerEngineService _triggerEngine;
//        private readonly GoogleFormsService _formService;
//        private readonly ILogger<GoogleFormsPollingService> _logger;
//        public GoogleFormsPollingService(AppDbContext dbContext, ITriggerEngineService triggerEngine, GoogleFormsService formService, ILogger<GoogleFormsPollingService> logger)
//        {
//            _dbContext = dbContext;
//            _triggerEngine = triggerEngine;
//            _formService = formService;
//            _logger = logger;
//        }

//        public async Task PollAsync()
//        {
//            try
//            {
//                var formConnections = await _dbContext.ConnectedApps
//                        .Where(c => c.Provider == "google_forms")
//                        .ToListAsync();

//                foreach (var connection in formConnections)
//                {
//                    await ProcessConnection(connection);
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"[ERROR]: {ex.Message}");
//            }
//        }

//        private async Task ProcessConnection(ConnectedApp connection)
//        {
//            try
//            {
//                var metaData = JsonConvert.DeserializeObject
//                        <Dictionary<string, string>>(connection.MetaDataJson);

//                var formId = metaData["formId"];

//                var json = await _formService.GetResponsesRawAsync(connection.AccessToken, formId);

//                var root = JObject.Parse(json);

//                var responses = root["responses"];

//                if (responses == null)
//                    return;

//                foreach (var response in responses)
//                {
//                    var responseId = response["responseId"]?.ToString();

//                    if (responseId == connection.LastSyncCursor)
//                        break;

//                    await _triggerEngine.ExecuteTriggerAsync(
//                        new TriggerEvent
//                        {
//                            TriggerName = "google_forms.new_response",
//                            UserId = connection.UserId,
//                            Payload = new Dictionary<string, object>
//                            {
//                            { "formId", formId },
//                            { "responseId", responseId },
//                            { "submittedAt", response["lastSubmittedTime"] }
//                            }
//                        });
//                }

//                var newest = responses.FirstOrDefault();

//                if (newest != null)
//                {
//                    connection.LastSyncCursor = newest["responseId"]?.ToString();
//                    await _dbContext.SaveChangesAsync();
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"[ERROR]: {ex.Message}");
//            }
//        }
//    }
//}
