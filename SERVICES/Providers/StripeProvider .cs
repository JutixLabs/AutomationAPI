using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AutomationAPI.SERVICES.Providers
{
    public class StripeProvider : IStripeProvider
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly ISecretProtector _secretProtector;
        public StripeProvider(HttpClient httpClient, AppDbContext dbContext, ISecretProtector secretProtector)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _secretProtector = secretProtector;
        }

        // Standard Connect accounts hand back an access_token that acts as that
        // connected account's own secret key — no separate Stripe-Account header needed.
        private async Task<string> GetAccessTokenAsync(string userId)
        {
            var stripe = await _dbContext.ConnectedApps
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == "stripe" && x.IsActive);

            if (stripe == null)
                throw new InvalidOperationException("Stripe not connected.");

            return _secretProtector.Unprotect(stripe.AccessToken);
        }

        public async Task<List<StripeCustomerDto>> GetCustomersAsync(string userId)
        {
            var token = await GetAccessTokenAsync(userId);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("https://api.stripe.com/v1/customers?limit=25");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var results = new List<StripeCustomerDto>();
            foreach (var item in doc.RootElement.GetProperty("data").EnumerateArray())
            {
                results.Add(new StripeCustomerDto
                {
                    Id = item.GetProperty("id").GetString(),
                    Email = item.TryGetProperty("email", out var e) ? e.GetString() : null,
                    Name = item.TryGetProperty("name", out var n) ? n.GetString() : null
                });
            }

            return results;
        }

        public async Task CreateCustomerAsync(string userId, string email, string name)
        {
            var token = await GetAccessTokenAsync(userId);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var form = new Dictionary<string, string> { { "email", email }, { "name", name } };

            var response = await _httpClient.PostAsync(
                "https://api.stripe.com/v1/customers",
                new FormUrlEncodedContent(form));

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Stripe create-customer failed ({response.StatusCode}): {body}");
            }
        }

        public async Task CreateRefundAsync(string userId, string chargeId)
        {
            var token = await GetAccessTokenAsync(userId);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var form = new Dictionary<string, string> { { "charge", chargeId } };

            var response = await _httpClient.PostAsync(
                "https://api.stripe.com/v1/refunds",
                new FormUrlEncodedContent(form));

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Stripe create-refund failed ({response.StatusCode}): {body}");
            }
        }
    }
}