using Hms.ReceptionApi.DTOs.Location;

namespace Hms.ReceptionApi.Interfaces.Clients;

public interface ILocationApiClient
{
    Task<List<StateDto>> GetStatesAsync(string country);
    Task<List<CityDto>> GetCitiesAsync(string country, string state);
}