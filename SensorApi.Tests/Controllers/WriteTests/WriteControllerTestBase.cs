using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SensorApi.Controllers;
using SensorApi.Domains.Interfaces;
using SensorApi.DTO;
using SensorApi.Models;
using SensorApi.Parameters;
using SensorApi.Stores.Interfaces;

namespace SensorApi.Tests.Controllers.Write;

public abstract class WriteControllerTestBase
{
    protected readonly Mock<IWriteDomain> MockWriteDomain = new();
    protected readonly Mock<IInMemorySensorStore> MockStore = new();
    protected readonly Mock<ILogger<WriteController>> MockLogger = new();
    protected readonly WriteController Controller;
    protected readonly ClientConfig TestClient;

    protected WriteControllerTestBase()
    {
        Controller = new WriteController(MockWriteDomain.Object, MockStore.Object, MockLogger.Object);

        // Setup test client
        TestClient = new ClientConfig(
            Name: "TestClient",
            Prefix: "TEST",
            ApiKey: "test-api-key-123"
        );

        // Setup HttpContext with client
        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        Controller.HttpContext.Items["Client"] = TestClient;

        // Setup default store total
        MockStore.Setup(x => x.TotalPointsStored).Returns(100);
    }

    protected WriteBatchRequest CreateValidWriteBatchRequest()
    {
        return new WriteBatchRequest
        {
            Sensors =
            [
                new()
                {
                    SensorName = "temperature",
                    DataPoints =
                    [
                        new() { Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10), Value = 22.5 },
                        new() { Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5), Value = 23.1 }
                    ]
                },
                new()
                {
                    SensorName = "humidity",
                    DataPoints =
                    [
                        new() { Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10), Value = 65.0 },
                        new() { Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5), Value = 67.5 }
                    ]
                }
            ]
        };
    }

    protected WriteBatchRequest CreateSingleSensorRequest(string sensorName = "sensor1", int dataPointCount = 5)
    {
        List<DataPointDto> dataPoints = [];
        for (int i = 0; i < dataPointCount; i++)
        {
            dataPoints.Add(new DataPointDto
            {
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-i),
                Value = 20.0 + i
            });
        }

        return new WriteBatchRequest
        {
            Sensors =
            [
                new()
                {
                    SensorName = sensorName,
                    DataPoints = dataPoints
                }
            ]
        };
    }
}
