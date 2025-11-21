using Moq;
using SensorApi.Domains;
using SensorApi.DTO;
using SensorApi.Parameters;
using SensorApi.Stores.Interfaces;

namespace SensorApi.Tests.Domains;

public abstract class WriteDomainTestBase
{
    protected readonly Mock<IInMemorySensorStore> MockStore;
    protected readonly WriteDomain Domain;
    protected const string TestPrefix = "TEST";

    protected WriteDomainTestBase()
    {
        MockStore = new Mock<IInMemorySensorStore>();
        Domain = new WriteDomain(MockStore.Object);
    }

    protected WriteBatchRequest CreateWriteBatchRequest(int sensorCount = 2, int pointsPerSensor = 3)
    {
        var request = new WriteBatchRequest { Sensors = new List<SensorWriteDto>() };
        var baseTime = DateTimeOffset.UtcNow;

        for (int i = 0; i < sensorCount; i++)
        {
            var sensor = new SensorWriteDto
            {
                SensorName = $"sensor{i}",
                DataPoints = new List<DataPointDto>()
            };

            for (int j = 0; j < pointsPerSensor; j++)
            {
                sensor.DataPoints.Add(new DataPointDto
                {
                    Timestamp = baseTime.AddMinutes(-j),
                    Value = 20.0 + j
                });
            }

            request.Sensors.Add(sensor);
        }

        return request;
    }
}
