using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Frontend.Models.Api;

namespace Frontend.Services
{
    public class AppointmentApiService : IAppointmentApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AppointmentApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<AppointmentSearchResponseDto> SearchAsync(AppointmentSearchRequestDto request)
        {
            return await PostAsync<AppointmentSearchRequestDto, AppointmentSearchResponseDto>("gateway/appointments/search", request)
                   ?? new AppointmentSearchResponseDto();
        }

        public async Task<AppointmentResponseDto?> GetByIdAsync(int id)
        {
            return await GetAsync<AppointmentResponseDto>($"gateway/appointments/{id}", true);
        }

        public async Task<List<AppointmentResponseDto>> GetByPatientIdAsync(int patientId)
        {
            return await GetAsync<List<AppointmentResponseDto>>($"gateway/appointments/patient/{patientId}")
                   ?? new List<AppointmentResponseDto>();
        }

        public async Task<List<AppointmentResponseDto>> GetByDoctorIdAsync(int doctorId)
        {
            return await GetAsync<List<AppointmentResponseDto>>($"gateway/appointments/doctor/{doctorId}")
                   ?? new List<AppointmentResponseDto>();
        }

        public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentRequestDto request)
        {
            return await PostAsync<CreateAppointmentRequestDto, AppointmentResponseDto>("gateway/appointments", request)
                   ?? throw new ApiException("No response from Appointments API.", 500);
        }

        public async Task<AppointmentResponseDto?> RescheduleAsync(int id, RescheduleAppointmentRequestDto request)
        {
            return await PutAsync<RescheduleAppointmentRequestDto, AppointmentResponseDto>($"gateway/appointments/{id}/reschedule", request, true);
        }

        public async Task<AppointmentResponseDto?> CancelAsync(int id, CancelAppointmentRequestDto request)
        {
            return await PutAsync<CancelAppointmentRequestDto, AppointmentResponseDto>($"gateway/appointments/{id}/cancel", request, true);
        }

        public async Task<AppointmentResponseDto?> CompleteAsync(int id, CompleteAppointmentRequestDto request)
        {
            return await PutAsync<CompleteAppointmentRequestDto, AppointmentResponseDto>($"gateway/appointments/{id}/complete", request, true);
        }

        private async Task<TResponse?> GetAsync<TResponse>(string url, bool allowNotFound = false)
        {
            using var response = await _httpClient.GetAsync(url);
            return await ReadResponseAsync<TResponse>(response, allowNotFound);
        }

        private async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            using var content = CreateJsonContent(request);
            using var response = await _httpClient.PostAsync(url, content);
            return await ReadResponseAsync<TResponse>(response, false);
        }

        private async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request, bool allowNotFound)
        {
            using var content = CreateJsonContent(request);
            using var response = await _httpClient.PutAsync(url, content);
            return await ReadResponseAsync<TResponse>(response, allowNotFound);
        }

        private StringContent CreateJsonContent<TRequest>(TRequest request)
        {
            return new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json");
        }

        private async Task<TResponse?> ReadResponseAsync<TResponse>(HttpResponseMessage response, bool allowNotFound)
        {
            if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
                return default;

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApiException(ExtractMessage(body, $"API request failed with status code {(int)response.StatusCode}."), (int)response.StatusCode);

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
