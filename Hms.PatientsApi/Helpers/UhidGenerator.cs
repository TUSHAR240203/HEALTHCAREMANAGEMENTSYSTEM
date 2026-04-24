namespace Hms.PatientsApi.Helpers;

public static class UhidGenerator
{
    public static string Generate()
    {
        var year = DateTime.UtcNow.Year;
        var uniquePart = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return $"UHID-{year}-{uniquePart}";
    }
}