using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Frontend.Models.Reception;

namespace Frontend.Services
{
    public class ReceptionApiService : IReceptionApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ReceptionApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<ReceptionPatientSearchResponseDto?> SearchPatientsAsync(ReceptionPatientSearchRequestDto request)
            => await PostAsync<ReceptionPatientSearchRequestDto, ReceptionPatientSearchResponseDto>(
                "gateway/reception/patients/search", request);

        public async Task<ReceptionPatientSummaryDto?> GetPatientSummaryAsync(int patientId)
            => await GetAsync<ReceptionPatientSummaryDto>($"gateway/reception/patients/{patientId}/summary", true);

        public async Task<T?> RegisterPatientAsync<T>(RegisterPatientByReceptionRequestDto request)
            => await PostAsync<RegisterPatientByReceptionRequestDto, T>("gateway/reception/patients/register", request);

        public async Task<T?> VerifyPatientAsync<T>(int patientId, VerifyPatientRequestDto request)
            => await PostAsync<VerifyPatientRequestDto, T>($"gateway/reception/patients/{patientId}/verify", request);

        public async Task<T?> BookAppointmentAsync<T>(BookAppointmentRequestDto request)
            => await PostAsync<BookAppointmentRequestDto, T>("gateway/reception/appointments/book", request);

        public async Task<T?> CheckInAsync<T>(CheckInRequestDto request)
            => await PostAsync<CheckInRequestDto, T>("gateway/reception/checkin", request);

        public async Task<DepartmentQueueResponseDto?> GetQueueAsync(int departmentId, DateOnly date)
            => await GetAsync<DepartmentQueueResponseDto>($"gateway/reception/queue/{departmentId}?date={date:yyyy-MM-dd}");

        public async Task<QueueCurrentResponseDto?> GetCurrentQueueAsync(int departmentId, DateOnly date)
            => await GetAsync<QueueCurrentResponseDto>($"gateway/reception/queue/{departmentId}/current?date={date:yyyy-MM-dd}", true);

        public async Task<T?> CallNextAsync<T>(int departmentId, DateOnly date)
            => await PostAsync<object, T>($"gateway/reception/queue/{departmentId}/next?date={date:yyyy-MM-dd}", new { });

        public async Task<T?> StartTokenAsync<T>(int queueTokenId)
            => await PutAsync<object, T>($"gateway/reception/queue/token/{queueTokenId}/start", new { }, true);

        public async Task<T?> CompleteTokenAsync<T>(int queueTokenId, CompleteQueueTokenRequestDto request)
            => await PutAsync<CompleteQueueTokenRequestDto, T>($"gateway/reception/queue/token/{queueTokenId}/complete", request, true);

        public async Task<T?> SkipTokenAsync<T>(int queueTokenId)
            => await PutAsync<object, T>($"gateway/reception/queue/token/{queueTokenId}/skip", new { }, true);

        public async Task<T?> RecallTokenAsync<T>(int queueTokenId)
            => await PutAsync<object, T>($"gateway/reception/queue/token/{queueTokenId}/recall", new { }, true);

        public async Task<T?> CancelTokenAsync<T>(int queueTokenId, CancelQueueTokenRequestDto request)
            => await PutAsync<CancelQueueTokenRequestDto, T>($"gateway/reception/queue/token/{queueTokenId}/cancel", request, true);

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
            return new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");
        }

        private async Task<TResponse?> ReadResponseAsync<TResponse>(HttpResponseMessage response, bool allowNotFound)
        {
            if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
                return default;

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    string.IsNullOrWhiteSpace(body)
                        ? $"API request failed with status code {(int)response.StatusCode}."
                        : body,
                    (int)response.StatusCode);
            }

            if (string.IsNullOrWhiteSpace(body))
                return default;

            return JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
        }
    }
}
