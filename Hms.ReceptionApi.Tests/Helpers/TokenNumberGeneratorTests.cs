using Hms.ReceptionApi.Helpers;
using Xunit;

namespace Hms.ReceptionApi.Tests.Helpers;

public class TokenNumberGeneratorTests
{
    [Fact]
    public void GenerateNext_ShouldReturnNextNumber_WhenLastTokenIsZero()
    {
        var result = TokenNumberGenerator.GenerateNext(0);

        Assert.Equal(1, result);
    }

    [Fact]
    public void GenerateNext_ShouldReturnNextNumber_WhenLastTokenIsPositive()
    {
        var result = TokenNumberGenerator.GenerateNext(100);

        Assert.Equal(101, result);
    }

    [Fact]
    public void GenerateNext_ShouldReturnZero_WhenLastTokenIsNegativeOne()
    {
        var result = TokenNumberGenerator.GenerateNext(-1);

        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(5, 6)]
    [InlineData(99, 100)]
    [InlineData(999, 1000)]
    public void GenerateNext_ShouldReturnExpectedValues(
        int input,
        int expected)
    {
        var result = TokenNumberGenerator.GenerateNext(input);

        Assert.Equal(expected, result);
    }
}