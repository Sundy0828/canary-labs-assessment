using SensorApi.Models;
using SensorApi.Services.Interfaces;

namespace SensorApi.Services;

public class ClientService : IClientService
{
    private readonly Dictionary<string, ClientConfig> _clientsByApiKey;

    public ClientService(IConfiguration configuration)
    {
        _clientsByApiKey = [];

        IConfigurationSection clientsSection = configuration.GetSection("clients");
        List<ClientConfig>? clients = clientsSection.Get<List<ClientConfig>>();

        if (clients != null)
        {
            foreach (ClientConfig client in clients)
            {
                _clientsByApiKey[client.ApiKey] = client;
            }
        }
    }

    public ClientConfig? GetClientByApiKey(string apiKey)
    {
        return _clientsByApiKey.TryGetValue(apiKey, out ClientConfig? client) ? client : null;
    }
}
