using System.Net;
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

        public async Task<DoctorResponseDto?> GetByAuthUserIdAsync(int authUserId)
        {
            return await GetAsync<DoctorResponseDto>(
                $"gateway/doctors/by-auth-user/{authUserId}",
                true);
        }

        public async Task<List<DoctorResponseDto>> GetAllAsync(bool? isActive = null)
        {
            var url = "gateway/doctors";

            if (isActive.HasValue)
            {
                url += $"?isActive={isActive.Value.ToString().ToLowerInvariant()}";
            }

            return await GetAsync<List<DoctorResponseDto>>(url) ?? new List<DoctorResponseDto>();
        }

        public async Task<DoctorResponseDto?> GetByIdAsync(int id)
        {
            return await GetAsync<DoctorResponseDto>(
                $"gateway/doctors/{id}",
                true);
        }

        public async Task<(bool Success, string Message, DoctorResponseDto? Data)> CreateAsync(CreateDoctorViewModel model)
        {
            var dto = new
            {
                authUserId = model.AuthUserId,
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
                var data = await PostAsync<object, DoctorResponseDto>(
                    "gateway/doctors",
                    dto);

                return (true, "Doctor profile created successfully.", data);
            }
            catch (ApiException ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<DoctorAvailabilityResponseDto?> GetAvailabilityAsync(
            int doctorId,
            DateOnly date,
            bool? isTeleConsultation = null)
        {
            var url = $"gateway/doctors/{doctorId}/available-slots?date={date:yyyy-MM-dd}";

            if (isTeleConsultation.HasValue)
            {
                url += $"&isTeleConsultation={isTeleConsultation.Value.ToString().ToLowerInvariant()}";
            }

            return await GetAsync<DoctorAvailabilityResponseDto>(url, true);
        }

        public async Task<List<DoctorScheduleResponseDto>> GetSchedulesAsync(int doctorId)
        {
            return await GetAsync<List<DoctorScheduleResponseDto>>(
                $"gateway/doctors/{doctorId}/schedules") ?? new List<DoctorScheduleResponseDto>();
        }

        public async Task<DoctorScheduleResponseDto?> AddScheduleAsync(
            int doctorId,
            CreateDoctorScheduleViewModel model)
        {
            var dto = new
            {
                dayOfWeek = model.DayOfWeek,
                startTime = model.StartTime,
                endTime = model.EndTime,
                breakStartTime = model.BreakStartTime,
                breakEndTime = model.BreakEndTime,
                slotDurationMinutes = model.SlotDurationMinutes,
                maxPatientsPerDay = model.MaxPatientsPerDay,
                isActive = model.IsActive
            };

            return await PostAsync<object, DoctorScheduleResponseDto>(
                $"gateway/doctors/{doctorId}/schedules",
                dto);
        }

        public async Task DeleteScheduleAsync(int doctorId, int scheduleId)
        {
            await DeleteAsync($"gateway/doctors/{doctorId}/schedules/{scheduleId}");
        }

        public async Task<List<DoctorLeaveResponseDto>> GetLeavesAsync(string? status = null)
        {
            var url = string.IsNullOrWhiteSpace(status)
                ? "gateway/doctors/leaves"
                : $"gateway/doctors/leaves?status={Uri.EscapeDataString(status)}";

            return await GetAsync<List<DoctorLeaveResponseDto>>(url)
                   ?? new List<DoctorLeaveResponseDto>();
        }

        public async Task<List<DoctorLeaveResponseDto>> GetLeavesByDoctorAsync(int doctorId)
        {
            return await GetAsync<List<DoctorLeaveResponseDto>>(
                $"gateway/doctors/{doctorId}/leaves") ?? new List<DoctorLeaveResponseDto>();
        }

        public async Task<DoctorLeaveResponseDto?> RequestLeaveAsync(
            int doctorId,
            CreateDoctorLeaveViewModel model)
        {
            var dto = new
            {
                startDate = model.StartDate,
                endDate = model.EndDate,
                reason = model.Reason
            };

            return await PostAsync<object, DoctorLeaveResponseDto>(
                $"gateway/doctors/{doctorId}/leaves",
                dto);
        }

        public async Task<DoctorLeaveResponseDto?> ApproveLeaveAsync(int leaveId, string? reviewedBy)
        {
            return await PutAsync<object, DoctorLeaveResponseDto>(
                $"gateway/doctors/leaves/{leaveId}/approve?reviewedBy={Uri.EscapeDataString(reviewedBy ?? "Admin")}",
                new { });
        }

        public async Task<DoctorLeaveResponseDto?> RejectLeaveAsync(int leaveId, string? reviewedBy)
        {
            return await PutAsync<object, DoctorLeaveResponseDto>(
                $"gateway/doctors/leaves/{leaveId}/reject?reviewedBy={Uri.EscapeDataString(reviewedBy ?? "Admin")}",
                new { });
        }

        private async Task<TResponse?> GetAsync<TResponse>(
            string url,
            bool allowNotFound = false)
        {
            using var response = await _httpClient.GetAsync(url);
            return await ReadResponseAsync<TResponse>(response, allowNotFound);
        }

        private async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string url,
            TRequest request)
        {
            using var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.PostAsync(url, content);

            return await ReadResponseAsync<TResponse>(response, false);
        }

        private async Task<TResponse?> PutAsync<TRequest, TResponse>(
            string url,
            TRequest request)
        {
            using var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.PutAsync(url, content);

            return await ReadResponseAsync<TResponse>(response, false);
        }

        private async Task DeleteAsync(string url)
        {
            using var response = await _httpClient.DeleteAsync(url);
            await ReadResponseAsync<object>(response, false);
        }

        private async Task<TResponse?> ReadResponseAsync<TResponse>(
            HttpResponseMessage response,
            bool allowNotFound)
        {
            if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
                return default;

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    ExtractMessage(body, $"Doctors API request failed with status code {(int)response.StatusCode}."),
                    (int)response.StatusCode);
            }

            if (string.IsNullOrWhiteSpace(body))
                return default;

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("data", out var data))
                {
                    return data.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                        ? default
                        : data.Deserialize<TResponse>(_jsonOptions);
                }
            }
            catch (JsonException)
            {
            }

            return JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
        }

        private static string ExtractMessage(string body, string fallback)
        {
            if (string.IsNullOrWhiteSpace(body))
                return fallback;

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                if (root.TryGetProperty("message", out var message) &&
                    message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString() ?? fallback;
                }

                if (root.TryGetProperty("title", out var title) &&
                    title.ValueKind == JsonValueKind.String)
                {
                    return title.GetString() ?? fallback;
                }

                if (root.TryGetProperty("errors", out var errors))
                {
                    return errors.ToString();
                }
            }
            catch
            {
            }

            return body;
        }
    }
}