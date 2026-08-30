//using AutomationAPI.DATA;
//using AutomationAPI.MODEL.Interface;
//using Microsoft.EntityFrameworkCore;

//namespace AutomationAPI.SERVICES
//{
//    public class FormWatcherService : BackgroundService
//    {
//        private readonly IServiceScopeFactory _scopeFactory;
//        private readonly ILogger<FormWatcherService> _logger;
//        public FormWatcherService(IServiceScopeFactory scopeFactory, ILogger<FormWatcherService> logger)
//        {
//            _scopeFactory = scopeFactory;            
//            _logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {

//                    using var scope =
//                        _scopeFactory.CreateScope();

//                    var workflowExecutionService =
//                        scope.ServiceProvider
//                            .GetRequiredService<
//                                IWorkflowExecutionService>();
//                    var context =
//                        scope.ServiceProvider
//                            .GetRequiredService<AppDbContext>();

//                    var googleSheets =
//                        scope.ServiceProvider
//                            .GetRequiredService<
//                                IGoogleSheetsService>();

//                    var watches =
//                        await context.FormWatches
//                            .ToListAsync();

//                    foreach (var watch in watches)
//                    {
//                        try
//                        {
//                            var rows =
//                                await googleSheets
//                                    .GetRowsAsync(
//                                        watch.SpreadSheetId,
//                                        watch.SheetName);

//                            if (rows == null)
//                                continue;

//                            var headers = rows[0]
//                                .Select(h => h.ToString())
//                                .ToList();

//                            var totalRows =
//                                rows.Count;

//                            if (totalRows > watch.LastProcessedRow)
//                            {
//                                for (int i = watch.LastProcessedRow; i < rows.Count; i++)
//                                {
//                                    var row = rows[i];

//                                    var payload =
//                                        new Dictionary<string, object>();

//                                    for (int j = 0;
//                                         j < headers.Count;
//                                         j++)
//                                    {
//                                        var key = headers[j];

//                                        var value =
//                                            row.Count > j
//                                                ? row[j]?.ToString()
//                                                : "";

//                                        payload[key] = value;
//                                    }

//                                    await workflowExecutionService
//                                        .ExecuteRuleAsync(
//                                            watch.RuleId,
//                                            payload);
//                                }
//                                watch.LastProcessedRow = totalRows;

//                                context.FormWatches.Update(watch);

//                                await context.SaveChangesAsync(stoppingToken);
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogError($"[ERROR]: {ex.Message}");
//                        }
//                    }

//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "FormWatcherService outer loop error");
//                }

//                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
//            }
//        }
//    }
//}
