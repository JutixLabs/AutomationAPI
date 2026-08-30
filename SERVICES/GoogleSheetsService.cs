//using AutomationAPI.DATA;
//using AutomationAPI.MODEL.Interface;
//using AutomationAPI.SERVICES.Persistence;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Services;
//using Google.Apis.Sheets.v4;
//using Microsoft.EntityFrameworkCore;

//namespace AutomationAPI.SERVICES
//{
//    public class GoogleSheetsService : IGoogleSheetsService
//    {
//        private readonly AppDbContext _dbContext;
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        private readonly ILogger<GoogleSheetsService> _logger;
//        public GoogleSheetsService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor,
//            ILogger<GoogleSheetsService> logger)
//        {
//            _dbContext = dbContext;
//            _httpContextAccessor = httpContextAccessor;
//            _logger = logger;
//        }
//        public async Task<IList<IList<object>>> GetRowsAsync(string spreadsheetId, string range)
//        {
//            try
//            {
//                var service = CreateService();

//                var request = service
//                    .Spreadsheets.Values.Get(spreadsheetId, range);

//                var response = await request.ExecuteAsync();

//                return response.Values;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"[ERROR]: {ex.Message}");
//                throw;
//            }
//        }

//        public async Task<IList<string>> GetSpreadsheetsAsync()
//        {
//            return new List<string>
//            {
//                "Google forms response"
//            };
//        }

//        private string GetAccessToken()
//        {
//            try
//            {
//                var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
//                var google = _dbContext.ConnectedApps
//                    .FirstOrDefault(
//                        g => g.UserId == userId && g.Provider == "google");

//                if (google == null)
//                    throw new Exception("Google Not Connected.");

//                return google.AccessToken;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"[ERROR]: {ex.Message}");
//                throw;
//            }
//        }
//        private SheetsService CreateService()
//        {
//            var credentials = GoogleCredential.FromAccessToken(GetAccessToken());

//            return new SheetsService(
//                new BaseClientService
//                    .Initializer
//                {
//                    HttpClientInitializer = credentials,

//                    ApplicationName = "JUTIX Automation"
//                });
//        }
//    }
//}
