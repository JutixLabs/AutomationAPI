using AutomationAPI.DATA;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.SERVICES
{
    public class ExecutionLogService : IExecutionLogService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ExecutionLogService> _logger;
        public ExecutionLogService(AppDbContext dbContext, ILogger<ExecutionLogService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<List<ExecutionLog>> GetExecutionLogAsync()
        {
            try
            {
                var executionLog = await _dbContext.ExecutionLogs
                        .OrderByDescending(l => l.ExecutedAt)
                        .ToListAsync();
                
                if (executionLog == null)
                    throw new Exception("Execuion Logs Not Found");

                var list = new List<ExecutionLog>();
                foreach (var log in executionLog)
                {
                    list.Add(log);
                }

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }

        public async Task<List<ExecutionLog>> DeleteAllLogsAsync()
        {
            try
            {
                var logs = await _dbContext.ExecutionLogs.ToListAsync();
                if (logs == null || logs.Count == 0)
                    throw new Exception("No logs to delete.");

                _dbContext.ExecutionLogs.RemoveRange(logs);
                await _dbContext.SaveChangesAsync();

                return logs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }
    }
}
