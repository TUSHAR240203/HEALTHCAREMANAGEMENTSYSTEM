using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Models.Admin;

namespace Frontend.Services
{
    public class StaffUserGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public StaffUserGatewayService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<StaffUserResponseDto>> GetUsersAsync(string token)
        {
            using var request = Authorized(HttpMethod.Get, "gateway/auth/users", token);
            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new List<StaffUserResponseDto>();
            return await response.Content.ReadFromJsonAsync<List<StaffUserResponseDto>>(_jsonOptions) ?? new List<StaffUserResponseDto>();
        }

        public async Task<(bool Success, string Message, StaffUserResponseDto? Data)> CreateAsync(string token, CreateStaffUserViewModel model)
        {
            using var request = Authorized(HttpMethod.Post, "gateway/auth/users", token);
            request.Content = JsonContent.Create(model);
            using var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<StaffUserResponseDto>(_jsonOptions);
                return (true, "User created successfully.", data);
            }
            return (false, await ReadErrorAsync(response), null);
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(string token, int id, bool isActive)
        {
            using var request = Authorized(HttpMethod.Put, $"gateway/auth/users/{id}/status", token);
            request.Content = JsonContent.Create(new UpdateUserStatusRequestDto { IsActive = isActive });
            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode ? (true, "User status updated.") : (false, await ReadErrorAsync(response));
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string token, int id)
        {
            using var request = Authorized(HttpMethod.Delete, $"gateway/auth/users/{id}", token);
            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode ? (true, "User deleted.") : (false, await ReadErrorAsync(response));
        }

        private static HttpRequestMessage Authorized(HttpMethod method, string url, string token)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content)) return $"Request failed with status code {(int)response.StatusCode}.";
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("message", out var msg)) return msg.GetString() ?? "Request failed.";
                if (doc.RootElement.TryGetProperty("title", out var title)) return title.GetString() ?? "Request failed.";
                if (doc.RootElement.TryGetProperty("errors", out var errors)) return errors.ToString();
            }
            catch { }
            return content;
        }
    }
}
