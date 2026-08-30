using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IWorkSpaceService
    {
        Task<List<WorkSpace>> GetWorkSpaceAsync();
        Task<WorkSpace> CreatetWorkSpaceAsync(CreateWorkSpace model);
        Task<Folder> CreateFolder(int workSpaceId, CreateFolder model);
        Task<List<Folder>> GetAllFoldersAsync(int workSpaceId);
    }
}
