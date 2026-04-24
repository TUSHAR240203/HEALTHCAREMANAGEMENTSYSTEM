using System.Net.Http.Json;
using Hms.ReceptionApi.DTOs.Location;
using Hms.ReceptionApi.Interfaces.Clients;

namespace Hms.ReceptionApi.Clients;

public class LocationApiClient : ILocationApiClient
{
    private readonly HttpClient _httpClient;

    public LocationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<StateDto>> GetStatesAsync(string country)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v0.1/countries/states", new
        {
            country
        });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to fetch states. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<StatesApiResponse>();

        if (result?.Data?.States == null)
            return new List<StateDto>();

        return result.Data.States
            .Select(x => new StateDto
            {
                Name = x.Name
            })
            .ToList();
    }

    public async Task<List<CityDto>> GetCitiesAsync(string country, string state)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v0.1/countries/state/cities", new
        {
            country,
            state
        });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to fetch cities. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<CitiesApiResponse>();

        if (result?.Data == null)
            return new List<CityDto>();

        return result.Data
            .Select(x => new CityDto
            {
                Name = x
            })
            .ToList();
    }

    private class StatesApiResponse
    {
        public StatesData? Data { get; set; }
    }

    private class StatesData
    {
        public List<StateItem> States { get; set; } = new();
    }

    private class StateItem
    {
        public string Name { get; set; } = default!;
    }

    private class CitiesApiResponse
    {
        public List<string> Data { get; set; } = new();
    }
}