using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.SERVICES
{
    public class WorkSpaceService : IWorkSpaceService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<WorkSpaceService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public WorkSpaceService(AppDbContext dbContext, ILogger<WorkSpaceService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<WorkSpace>> GetWorkSpaceAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            try
            {
                var workSpace = await _dbContext.WorkSpaces
                    .Include(w => w.Folders)
                        .Where(w => w.UserId == userId)
                        .Select(w => new
                        {
                            ID = w.ID,
                            Name = w.Name,
                            Color = w.Color,
                            Icon = w.Icon,
                            Folders = w.Folders.Count
                        })
                        .ToListAsync();
                if (workSpace == null || workSpace.Count == 0)
                    throw new Exception("No workspace found for the user.");
                var list = new List<WorkSpace>();
                foreach (var w in workSpace)
                {
                    list.Add(new WorkSpace
                    {
                        ID = w.ID,
                        Name = w.Name,
                        Color = w.Color,
                        Icon = w.Icon,
                        Folders = new List<Folder>() // You can populate this if needed
                    });
                }
                _logger.LogInformation($"[INFO] Retrieved {workSpace.Count} workspaces for user {userId}.");

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }

        public async Task<WorkSpace> CreatetWorkSpaceAsync(CreateWorkSpace model)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            try
            {
                var name = model.Name.ToLower().Trim();
                var validateWorkSpace = await _dbContext.WorkSpaces.FirstOrDefaultAsync(w => w.UserId == userId && w.Name.ToLower().Trim() == name);
                if (validateWorkSpace != null)
                    throw new Exception("Workspace with the same name already exists.");

                var newWorkSpace = new WorkSpace
                {
                    Name = model.Name,
                    Color = model.Color,
                    Icon = model.Icon,
                    UserId = userId
                };

                await _dbContext.WorkSpaces.AddAsync(newWorkSpace);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"[INFO] Workspace '{model.Name}' created successfully for user {userId}.");

                return newWorkSpace;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }

        public async Task<Folder> CreateFolder(int workSpaceId, CreateFolder model)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            try
            {
                var validateWorkSpace = await _dbContext.WorkSpaces.FirstOrDefaultAsync(w => w.ID == workSpaceId && w.UserId == userId);
                if (validateWorkSpace == null)
                    throw new Exception("Workspace not found.");

                var name = model.Name.ToLower().Trim();
                var validateFolder = await _dbContext.Folders.FirstOrDefaultAsync(f => f.WorkSpaceId == workSpaceId && f.Name.ToLower().Trim() == name);
                if (validateFolder != null)
                    throw new Exception("Folder with the same name already exists in this workspace.");

                var folder = new Folder
                {
                    Name = model.Name,
                    WorkSpaceId = workSpaceId
                };
                await _dbContext.Folders.AddAsync(folder);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"[INFO] Folder '{model.Name}' created successfully in workspace ID {workSpaceId}.");

                return folder;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }

        public async Task<List<Folder>> GetAllFoldersAsync(int workSpaceId)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            var folders = await _dbContext.Folders
                .Where(f => f.WorkSpaceId == workSpaceId && f.WorkSpace.UserId == userId)
                .Select(f => new Folder
                {
                    ID = f.ID,
                    Name = f.Name,
                    WorkSpaceId = f.WorkSpaceId
                })
                .ToListAsync();

            _logger.LogInformation($"[INFO] Retrieved {folders.Count} folders for workspace ID {workSpaceId} and user {userId}.");

            return folders;
        }
    }
}
