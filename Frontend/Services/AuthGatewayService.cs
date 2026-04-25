using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Models.Auth;

namespace Frontend.Services
{
    public class AuthGatewayService
    {
        private readonly HttpClient _httpClient;

        public AuthGatewayService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string Message)> SendLoginOtpAsync(int patientId, string mobileNumber)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "gateway/auth/patient/send-login-otp",
                new SendPatientPortalActivationRequestDto
                {
                    PatientId = patientId,
                    MobileNumber = mobileNumber
                });

            if (response.IsSuccessStatusCode)
                return (true, "Login OTP sent successfully.");

            var error = await ReadErrorAsync(response);
            return (false, error);
        }

        public async Task<(bool Success, AuthResponseDto? Data, string Message)> StaffLoginAsync(StaffLoginRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("gateway/auth/staff/login", request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                return (true, data, "Login successful.");
            }
            var error = await ReadErrorAsync(response);
            return (false, null, error);
        }

        public async Task<(bool Success, string Message)> SendStaffLoginOtpAsync(string loginIdOrMobile)
        {
            var response = await _httpClient.PostAsJsonAsync("gateway/auth/staff/send-login-otp", new StaffOtpRequestDto { LoginId = loginIdOrMobile, MobileNumber = loginIdOrMobile });
            if (response.IsSuccessStatusCode) return (true, "Staff login OTP sent successfully.");
            var error = await ReadErrorAsync(response);
            return (false, error);
        }

        public async Task<(bool Success, AuthResponseDto? Data, string Message)> StaffOtpLoginAsync(StaffOtpLoginRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("gateway/auth/staff/otp-login", request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                return (true, data, "Login successful.");
            }
            var error = await ReadErrorAsync(response);
            return (false, null, error);
        }

        public async Task<(bool Success, AuthResponseDto? Data, string Message)> UpdateAuthPreferenceAsync(string token, AuthPreferenceViewModel model)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, "gateway/auth/auth-preference");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(model);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                return (true, data, "Authentication preference saved.");
            }
            var error = await ReadErrorAsync(response);
            return (false, null, error);
        }

        public async Task<(bool Success, AuthResponseDto? Data, string Message)> LoginAsync(PatientLoginRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("gateway/auth/patient/login", request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                return (true, data, "Login successful.");
            }

            var error = await ReadErrorAsync(response);
            return (false, null, error);
        }

        public async Task<CurrentUserResponseDto?> GetCurrentUserAsync(string token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "gateway/auth/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CurrentUserResponseDto>();
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return $"Request failed with status code {(int)response.StatusCode}.";

            try
            {
                using var doc = JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? "Request failed.";

                if (doc.RootElement.TryGetProperty("title", out var title))
                    return title.GetString() ?? "Request failed.";

                if (doc.RootElement.TryGetProperty("errors", out var errors))
                    return errors.ToString();

                return content;
            }
            catch
            {
                return content;
            }
        }
    }
}