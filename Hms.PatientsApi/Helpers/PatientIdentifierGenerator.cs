namespace Hms.PatientsApi.Helpers;

public static class PatientIdentifierGenerator
{
    public static string Generate()
    {
        var year = DateTime.UtcNow.Year;
        var uniquePart = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"PAT-{year}-{uniquePart}";
    }
}
