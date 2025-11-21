using SensorApi.Domains.Interfaces;
using SensorApi.Parameters;
using SensorApi.Stores.Interfaces;

namespace SensorApi.Domains;

public class WriteDomain(IInMemorySensorStore store) : IWriteDomain
{
    private readonly IInMemorySensorStore _store = store;

    public int WriteBatch(WriteBatchRequest request, string clientPrefix)
    {
        foreach (DTO.SensorWriteDto sensor in request.Sensors)
        {
            IEnumerable<Models.DataPoint> points = sensor.DataPoints.Select(d => d.ToDataPoint());
            string prefixedSensorName = $"{clientPrefix}_{sensor.SensorName}";
            _store.Append(prefixedSensorName, points);
        }

        return request.Sensors.Sum(s => s.DataPoints.Count);
    }
}
