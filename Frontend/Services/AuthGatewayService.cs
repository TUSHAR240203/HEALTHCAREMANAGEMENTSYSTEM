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
                NormalizePhotoUrl(data);
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
                NormalizePhotoUrl(data);
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
                NormalizePhotoUrl(data);
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
                NormalizePhotoUrl(data);
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

            var user = await response.Content.ReadFromJsonAsync<CurrentUserResponseDto>();
            NormalizePhotoUrl(user);
            return user;
        }

        public async Task<(bool Success, CurrentUserResponseDto? Data, string Message)> UploadMyPhotoAsync(string token, IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return (false, null, "Please select a photo to upload.");

            using var request = new HttpRequestMessage(HttpMethod.Post, "gateway/auth/me/photo");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var form = new MultipartFormDataContent();
            using var stream = photo.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(photo.ContentType ?? "application/octet-stream");
            form.Add(fileContent, "photo", photo.FileName);
            request.Content = form;

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response);
                return (false, null, error);
            }

            var data = await response.Content.ReadFromJsonAsync<CurrentUserResponseDto>();
            NormalizePhotoUrl(data);
            return (true, data, "Profile photo uploaded successfully.");
        }

        public async Task<(bool Success, CurrentUserResponseDto? Data, string Message)> UpdateMyPhotoUrlAsync(string token, string photoUrl)
        {
            if (string.IsNullOrWhiteSpace(photoUrl))
                return (false, null, "Photo URL is required.");

            using var request = new HttpRequestMessage(HttpMethod.Put, "gateway/auth/me/photo-url");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(new { photoUrl });

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response);
                return (false, null, error);
            }

            var data = await response.Content.ReadFromJsonAsync<CurrentUserResponseDto>();
            NormalizePhotoUrl(data);
            return (true, data, "Profile photo uploaded successfully.");
        }

        private void NormalizePhotoUrl(AuthResponseDto? user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.PhotoUrl)) return;
            user.PhotoUrl = NormalizePhotoUrl(user.PhotoUrl);
        }

        private void NormalizePhotoUrl(CurrentUserResponseDto? user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.PhotoUrl)) return;
            user.PhotoUrl = NormalizePhotoUrl(user.PhotoUrl);
        }

        private string NormalizePhotoUrl(string photoUrl)
        {
            if (string.IsNullOrWhiteSpace(photoUrl)) return photoUrl;
            if (photoUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                photoUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                photoUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return photoUrl;

            if (photoUrl.StartsWith("/gateway/", StringComparison.OrdinalIgnoreCase) ||
                photoUrl.StartsWith("gateway/", StringComparison.OrdinalIgnoreCase))
            {
                var baseUri = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
                return $"{baseUri}/{photoUrl.TrimStart('/')}";
            }

            return photoUrl;
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
