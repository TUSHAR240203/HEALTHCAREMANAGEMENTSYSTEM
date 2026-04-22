namespace Hms.ReceptionApi.Interfaces.Clients;

public interface IAuthApiClient
{
    Task SendPortalActivationAsync(int patientId);
}