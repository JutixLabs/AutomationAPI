using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;

namespace AutomationAPI.SERVICES.Resources
{
    public class GitHubResourceProvider : IResourceProvider
    {
        private readonly IGitHubProvider _gitHubProvider;
        public GitHubResourceProvider(IGitHubProvider gitHubProvider)
        {
            _gitHubProvider = gitHubProvider;
        }

        public async Task<List<ResourceOptionDto>> GetResourcesAsync(string resourceType, Dictionary<string, string> filters = null)
        {
            if (resourceType != "repository")
                return new();

            var repos = await _gitHubProvider.GetRepositoriesAsync();

            return repos.Select(x => new ResourceOptionDto
            {
                Id = x.Full_Name,

                Name = x.Full_Name
            }).ToList();
        }

        public string ProviderName()
            => "github";

        //public async Task<List<ResourceDTO>> GetResourcesAsync()
        //{
        //    var repositories = await _gitHubProvider.GetRepositoriesAsync();

        //    return repositories.Select(r => new ResourceDTO
        //    {
        //        Id = r.Id.ToString(),
        //        Name = r.Name,
        //        Type = "repository"
        //    })
        //    .ToList();
        //}
    }
}
