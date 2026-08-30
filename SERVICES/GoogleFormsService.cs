//using System.Net.Http.Headers;

//namespace AutomationAPI.SERVICES
//{
//    public class GoogleFormsService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly ILogger<GoogleFormsService> _logger;
//        public GoogleFormsService(HttpClient httpClient, ILogger<GoogleFormsService> logger)
//        {
//            _httpClient = httpClient;
//            _logger = logger;
//        }

//        public async Task<string> GetResponsesRawAsync(string formId, string accessToken)
//        {
//            try
//            {
//                _httpClient.DefaultRequestHeaders.Authorization
//                        = new AuthenticationHeaderValue(
//                            "Bearer",
//                            accessToken);

//                return await _httpClient.GetStringAsync(
//                    $"https://forms.googleapis.com/v1/forms/{formId}/responses");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"[ERROR]: {ex.Message}");
//                throw;
//            }
//        }
//    }
//}
