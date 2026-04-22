namespace Hms.ReceptionApi.Helpers;

public static class TokenNumberGenerator
{
    public static int GenerateNext(int lastToken)
    {
        return lastToken + 1;
    }
}