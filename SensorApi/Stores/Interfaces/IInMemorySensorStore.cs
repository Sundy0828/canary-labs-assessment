using SensorApi.Models;

namespace SensorApi.Stores.Interfaces;

public interface IInMemorySensorStore
{
    void Append(string sensorName, IEnumerable<DataPoint> points);
    IReadOnlyList<DataPoint> ReadRange(string sensorName, DateTimeOffset start, DateTimeOffset end);
    Dictionary<string, IReadOnlyList<DataPoint>> ReadRange(IEnumerable<string> sensorNames, DateTimeOffset start, DateTimeOffset end);
    long TotalPointsStored { get; }
}
