using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AutomationAPI.SERVICES.Providers
{
    public class GitHubProvider : IGitHubProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConnectAppsService _connectAppsService;
        //private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly AppDbContext _dbContext;
        public GitHubProvider(IConnectAppsService connectAppsService, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _connectAppsService = connectAppsService;
        }

        public async Task<List<GitHubBranchDto>> GetBranchesAsync(string owner, string repository)
        {
            var connection = await _connectAppsService.GetConnectionAsync("github");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await _connectAppsService.GetAccessTokenAsync("github"));
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "AutomationAPI");

            var response = await _httpClient.GetAsync($"https://api.github.com/repos/{owner}/{repository}/branches");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<GitHubBranchDto>>
                (
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                ) ?? new();
        }

        public async Task CreateIssueAsync(string repoFullName, string title, string description)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await _connectAppsService.GetAccessTokenAsync("github"));
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AutomationAPI");

            var response = await _httpClient.PostAsJsonAsync(
                $"https://api.github.com/repos/{repoFullName}/issues",
                new { title, body = description });

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GitHub create-issue failed ({response.StatusCode}): {body}");
            }
        }

        public async Task CreateBranchAsync(string repoFullName, string branchName)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await _connectAppsService.GetAccessTokenAsync("github"));
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AutomationAPI");

            // Look up the repo's default branch so we know where to branch from.
            var repoResponse = await _httpClient.GetAsync($"https://api.github.com/repos/{repoFullName}");
            if (!repoResponse.IsSuccessStatusCode)
            {
                var repoErrorBody = await repoResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GitHub repo lookup failed ({repoResponse.StatusCode}): {repoErrorBody}");
            }
            var repoJson = await repoResponse.Content.ReadAsStringAsync();
            var repoInfo = JsonSerializer.Deserialize<GitHubRepositoryDto>(
                repoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var baseBranch = repoInfo?.Default_Branch ?? "main";

            // Get the SHA that the default branch currently points to.
            var refResponse = await _httpClient.GetAsync(
                $"https://api.github.com/repos/{repoFullName}/git/ref/heads/{baseBranch}");
            if (!refResponse.IsSuccessStatusCode)
            {
                var refErrorBody = await refResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GitHub base-branch lookup failed ({refResponse.StatusCode}): {refErrorBody}");
            }
            var refJson = await refResponse.Content.ReadAsStringAsync();
            using var refDoc = JsonDocument.Parse(refJson);
            var sha = refDoc.RootElement.GetProperty("object").GetProperty("sha").GetString();

            var createResponse = await _httpClient.PostAsJsonAsync(
                $"https://api.github.com/repos/{repoFullName}/git/refs",
                new { @ref = $"refs/heads/{branchName}", sha });

            if (!createResponse.IsSuccessStatusCode)
            {
                var body = await createResponse.Content.ReadAsStringAsync();

                // GitHub returns this exact message when the branch already exists — that's the
                // desired end state already being true, not a real failure. Treat it as a no-op
                // success instead of burning retries and dead-lettering on something retrying
                // can never fix.
                if (createResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity &&
                    body.Contains("Reference already exists"))
                {
                    return;
                }

                throw new HttpRequestException($"GitHub create-branch failed ({createResponse.StatusCode}): {body}");
            }
        }

        public async Task<List<GitHubRepositoryDto>> GetRepositoriesAsync()
        {
            var connection = await _connectAppsService.GetConnectionAsync("github");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await _connectAppsService.GetAccessTokenAsync("github"));
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "AutomationAPI");

            var response = await _httpClient.GetAsync("https://api.github.com/user/repos");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<GitHubRepositoryDto>>
                (
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                ) ?? new();
        }
    }
}
