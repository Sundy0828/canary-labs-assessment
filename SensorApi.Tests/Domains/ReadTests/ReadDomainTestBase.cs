using Moq;
using SensorApi.Domains;
using SensorApi.Models;
using SensorApi.Stores.Interfaces;

namespace SensorApi.Tests.Domains;

public abstract class ReadDomainTestBase
{
    protected readonly Mock<IInMemorySensorStore> MockStore;
    protected readonly ReadDomain Domain;
    protected const string TestPrefix = "TEST";

    protected ReadDomainTestBase()
    {
        MockStore = new Mock<IInMemorySensorStore>();
        Domain = new ReadDomain(MockStore.Object);
    }

    protected Dictionary<string, IReadOnlyList<DataPoint>> CreateMockStoreData(string prefix, List<string> sensorNames, int pointsPerSensor = 5)
    {
        var result = new Dictionary<string, IReadOnlyList<DataPoint>>();
        var baseTime = DateTimeOffset.UtcNow.AddHours(-1);

        foreach (var sensor in sensorNames)
        {
            var points = new List<DataPoint>();
            for (int i = 0; i < pointsPerSensor; i++)
            {
                points.Add(new DataPoint(baseTime.AddMinutes(i * 10), 20.0 + i));
            }
            result[$"{prefix}_{sensor}"] = points;
        }

        return result;
    }
}
