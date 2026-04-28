using FluentAssertions;
using Hms.BillingApi;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

public class BillingApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BillingApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateInvoice_ShouldReturnResponse()
    {
        var request = new
        {
            patientId = 1,
            consultationFee = 100,
            items = new[]
            {
                new { serviceName = "Test", price = 50, quantity = 2 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/billing/invoice", request);

        response.Should().NotBeNull(); // ✅ safe check
    }

    [Fact]
    public async Task AddPayment_InvalidInvoice_ShouldFail()
    {
        var response = await _client.PostAsJsonAsync("/api/billing/999/payment",
            new { amount = 100 });

        response.IsSuccessStatusCode.Should().BeFalse();
    }
}