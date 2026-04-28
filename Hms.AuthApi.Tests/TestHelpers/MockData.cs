namespace Hms.AuthApi.Tests.TestHelpers;

public static class MockData
{
    public static object User => new
    {
        Id = 1,
        FullName = "Tushar Sharma",
        Email = "tushar@gmail.com"
    };
}