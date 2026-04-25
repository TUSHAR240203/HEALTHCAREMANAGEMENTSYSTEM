namespace Frontend.Models.Auth
{
    public class CurrentUserResponseDto
    {
        public int UserId { get; set; }
        public int? PatientId { get; set; }
        public string? UHID { get; set; }
        public string? FullName { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
        public string Role => Roles.FirstOrDefault() ?? string.Empty;
        public bool IsProfileCompleted { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
