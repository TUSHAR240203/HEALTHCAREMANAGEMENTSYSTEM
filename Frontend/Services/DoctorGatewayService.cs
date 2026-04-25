using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Frontend.Models.Doctors;

namespace Frontend.Services
{
    public class DoctorGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public DoctorGatewayService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<List<DoctorResponseDto>> GetAllAsync(bool? isActive = null)
        {
            var request = new { isActive };
            return await PostAsync<object, List<DoctorResponseDto>>("gateway/doctors/search", request)
                   ?? new List<DoctorResponseDto>();
        }

        public async Task<DoctorResponseDto?> GetByIdAsync(int id)
        {
            return await GetAsync<DoctorResponseDto>($"gateway/doctors/{id}", true);
        }

        public async Task<(bool Success, string Message, DoctorResponseDto? Data)> CreateAsync(CreateDoctorViewModel model)
        {
            // Send only serializable fields — PhotoFile (IFormFile) is handled by the MVC layer.
            var dto = new
            {
                fullName = model.FullName,
                email = model.Email,
                phone = model.Phone,
                gender = model.Gender,
                qualification = model.Qualification,
                specialization = model.Specialization,
                departmentId = model.DepartmentId,
                departmentName = model.DepartmentName,
                consultationFee = model.ConsultationFee,
                experienceYears = model.ExperienceYears,
                licenseNumber = model.LicenseNumber,
                roomNumber = model.RoomNumber,
                supportsTeleConsultation = model.SupportsTeleConsultation,
                photoUrl = model.PhotoUrl
            };

            try
            {
                var data = await PostAsync<object, DoctorResponseDto>("gateway/doctors", dto);
                return (true, "Doctor profile created successfully.", data);
            }
            catch (ApiException ex)
            {
                return (false, ex.Message, null);
            }
        }

        private async Task<TResponse?> GetAsync<TResponse>(string url, bool allowNotFound = false)
        {
            using var response = await _httpClient.GetAsync(url);
            return await ReadResponseAsync<TResponse>(response, allowNotFound);
        }

        private async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            using var content = new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(url, content);
            return await ReadResponseAsync<TResponse>(response, false);
        }

        private async Task<TResponse?> ReadResponseAsync<TResponse>(HttpResponseMessage response, bool allowNotFound)
        {
            if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
                return default;

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApiException(ExtractMessage(body, $"Doctors API request failed with status code {(int)response.StatusCode}."), (int)response.StatusCode);

            if (string.IsNullOrWhiteSpace(body))
                return default;

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data))
                    return data.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ? default : data.Deserialize<TResponse>(_jsonOptions);
            }
            catch (JsonException)
            {
            }

            return JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
        }

        private static string ExtractMessage(string body, string fallback)
        {
            if (string.IsNullOrWhiteSpace(body)) return fallback;
            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                    return message.GetString() ?? fallback;
                if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    return title.GetString() ?? fallback;
                if (root.TryGetProperty("errors", out var errors))
                    return errors.ToString();
            }
            catch { }
            return body;
        }
    }
}
