using SensorApi.Domains.Interfaces;
using SensorApi.DTO;
using SensorApi.Parameters;
using SensorApi.Stores.Interfaces;

namespace SensorApi.Domains;

public class ReadDomain(IInMemorySensorStore store) : IReadDomain
{
    private readonly IInMemorySensorStore _store = store;

    public ReadResponse ReadRange(ReadRequest request, string clientPrefix)
    {
        // Prepend client prefix to all requested sensor names
        List<string> prefixedSensorNames = request.SensorNames.Select(name => $"{clientPrefix}_{name}").ToList();

        Dictionary<string, IReadOnlyList<Models.DataPoint>> results = _store.ReadRange(prefixedSensorNames, request.Start, request.End);

        ReadResponse dto = new();
        foreach (KeyValuePair<string, IReadOnlyList<Models.DataPoint>> kv in results)
        {
            // Remove the client prefix from the key in the response
            string unprefixedKey = kv.Key.StartsWith($"{clientPrefix}_")
                ? kv.Key[(clientPrefix.Length + 1)..]
                : kv.Key;

            dto.Results[unprefixedKey] = kv.Value.Select(dp => new DataPointDto { Timestamp = dp.Timestamp, Value = dp.Value }).ToList();
        }

        return dto;
    }
}
