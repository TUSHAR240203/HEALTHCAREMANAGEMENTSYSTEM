namespace Hms.ReceptionApi.Helpers;

public static class ApiRoutes
{
    public static class Patients
    {
        public const string Search = "/api/patients/search";
        public const string Create = "/api/patients";
        public static string GetById(int id) => $"/api/patients/{id}";
    }

    public static class Appointments
    {
        public const string Create = "/api/appointments";
        public static string Reschedule(int id) => $"/api/appointments/{id}/reschedule";
        public static string Cancel(int id) => $"/api/appointments/{id}/cancel";
    }

}