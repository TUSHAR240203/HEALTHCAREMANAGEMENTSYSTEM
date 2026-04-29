using System.Text.Json.Serialization;

namespace Frontend.Models.Auth
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }

        public int PatientId { get; set; }

        public string UHID { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public string[] Roles { get; set; } = [];

        public string? Role { get; set; }

        [JsonIgnore]
        public string EffectiveRole =>
            !string.IsNullOrWhiteSpace(Role)
                ? Role
                : Roles.FirstOrDefault() ?? string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        public bool IsPasswordLoginEnabled { get; set; }

        public bool IsOtpLoginEnabled { get; set; }

        public bool IsFirstLoginCompleted { get; set; }

        public bool IsProfileCompleted { get; set; }

        public string? PhotoUrl { get; set; }
    }
}
