using System.Net.Http.Json;
using System.Text.Json;
using Hms.Web.Models.Patients;

namespace Hms.Web.Services
{
    public class PatientGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PatientGatewayService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string Message, PatientResponseDto? Data)> CreateAsync(CreatePatientRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("gateway/patients", request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<PatientResponseDto>(_jsonOptions);
                return (true, "Patient created successfully.", data);
            }

            return (false, await ReadErrorAsync(response), null);
        }
        public async Task<PatientResponseDto?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"gateway/patients/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<PatientResponseDto>(_jsonOptions);
        }

        public async Task<PatientResponseDto?> GetByUhidAsync(string uhid)
        {
            var response = await _httpClient.GetAsync($"gateway/patients/by-uhid/{uhid}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<PatientResponseDto>(_jsonOptions);
        }

        public async Task<List<PatientResponseDto>> SearchAsync(PatientSearchRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("gateway/patients/search", request);

            if (!response.IsSuccessStatusCode)
                return new List<PatientResponseDto>();

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var wrapped = JsonSerializer.Deserialize<PatientSearchResponseDto>(content, _jsonOptions);
                if (wrapped?.Patients != null && wrapped.Patients.Count > 0)
                    return wrapped.Patients;
            }
            catch
            {
            }

            try
            {
                var list = JsonSerializer.Deserialize<List<PatientResponseDto>>(content, _jsonOptions);
                return list ?? new List<PatientResponseDto>();
            }
            catch
            {
                return new List<PatientResponseDto>();
            }
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, UpdatePatientRequestDto request)
        {
            var response = await _httpClient.PutAsJsonAsync($"gateway/patients/{id}", request);

            if (response.IsSuccessStatusCode)
                return (true, "Patient updated successfully.");

            return (false, await ReadErrorAsync(response));
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"gateway/patients/{id}");

            if (response.IsSuccessStatusCode)
                return (true, "Patient deleted successfully.");

            return (false, await ReadErrorAsync(response));
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return $"Request failed with status code {(int)response.StatusCode}.";

            try
            {
                using var doc = JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("errors", out var errors))
                {
                    var messages = new List<string>();

                    foreach (var error in errors.EnumerateObject())
                    {
                        foreach (var item in error.Value.EnumerateArray())
                        {
                            messages.Add($"{error.Name}: {item.GetString()}");
                        }
                    }

                    return string.Join(" | ", messages);
                }

                if (doc.RootElement.TryGetProperty("message", out var message))
                    return message.GetString() ?? "Request failed.";

                if (doc.RootElement.TryGetProperty("title", out var title))
                    return title.GetString() ?? "Request failed.";

                return content;
            }
            catch
            {
                return content;
            }
        }
    }
}