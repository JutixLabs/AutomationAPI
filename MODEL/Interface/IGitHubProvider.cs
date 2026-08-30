using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.MODEL.Interface
{
    public interface IGitHubProvider
    {
        Task<List<GitHubRepositoryDto>> GetRepositoriesAsync();
        Task<List<GitHubBranchDto>> GetBranchesAsync(string owner, string repository);

        Task CreateIssueAsync(string repoFullName, string title, string description);
        Task CreateBranchAsync(string repoFullName, string branchName);
    }
}
