namespace Hms.AuthApi.Common;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Receptionist = "Receptionist";
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    public const string Nurse = "Nurse";

    public static readonly string[] All =
    {
        Admin,
        Receptionist,
        Patient,
        Doctor,
        Nurse
    };
}