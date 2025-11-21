using SensorApi.Models;

namespace SensorApi.Services.Interfaces;

public interface IClientService
{
    ClientConfig? GetClientByApiKey(string apiKey);
}
