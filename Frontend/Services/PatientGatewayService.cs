using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Models.Patients;

namespace Frontend.Services
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

        public async Task<(bool Success, string Message, PatientResponseDto? Data)> RegisterForPortalAsync(CreatePatientRequestDto request)
        {
            var apiRequest = new
            {
                firstName = request.FirstName?.Trim(),
                middleName = (string?)null,
                lastName = request.LastName?.Trim(),
                dateOfBirth = request.DateOfBirth.ToString("yyyy-MM-dd"),
                gender = request.Gender,
                mobileNumber = request.MobileNumber?.Trim(),
                email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                portalAccessEnabled = true
            };

            var response = await _httpClient.PostAsJsonAsync("gateway/patients", apiRequest);

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
            request ??= new PatientSearchRequestDto();

            var response = await _httpClient.PostAsJsonAsync(
                "gateway/patients/search",
                request);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    string.IsNullOrWhiteSpace(body)
                        ? $"Patient search failed with status code {(int)response.StatusCode}."
                        : body,
                    (int)response.StatusCode);
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return new List<PatientResponseDto>();
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            using var document = System.Text.Json.JsonDocument.Parse(body);
            var root = document.RootElement;

            // Case 1:
            // { success: true, message: "...", data: { patients: [...] } }
            if (root.TryGetProperty("data", out var data))
            {
                if (data.ValueKind == System.Text.Json.JsonValueKind.Null)
                    return new List<PatientResponseDto>();

                if (data.TryGetProperty("patients", out var patientsElement))
                {
                    return patientsElement.Deserialize<List<PatientResponseDto>>(options)
                           ?? new List<PatientResponseDto>();
                }

                // Case 2:
                // { success: true, data: [...] }
                if (data.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    return data.Deserialize<List<PatientResponseDto>>(options)
                           ?? new List<PatientResponseDto>();
                }
            }

            // Case 3:
            // { patients: [...] }
            if (root.TryGetProperty("patients", out var directPatients))
            {
                return directPatients.Deserialize<List<PatientResponseDto>>(options)
                       ?? new List<PatientResponseDto>();
            }

            // Case 4:
            // [...]
            if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                return root.Deserialize<List<PatientResponseDto>>(options)
                       ?? new List<PatientResponseDto>();
            }

            return new List<PatientResponseDto>();
        }

        public async Task<(bool Success, string Message, PatientResponseDto? Data)> CompleteProfileAsync(int id, CompletePatientProfileRequestDto request)
        {
            var response = await _httpClient.PutAsJsonAsync($"gateway/patients/{id}/complete-profile", request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<PatientResponseDto>(_jsonOptions);
                return (true, "Profile completed successfully.", data);
            }

            return (false, await ReadErrorAsync(response), null);
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
