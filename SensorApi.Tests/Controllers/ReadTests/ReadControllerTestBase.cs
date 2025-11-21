using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SensorApi.Controllers;
using SensorApi.Domains.Interfaces;
using SensorApi.DTO;
using SensorApi.Models;
using SensorApi.Parameters;

namespace SensorApi.Tests.Controllers.Read;

public abstract class ReadControllerTestBase
{
    protected readonly Mock<IReadDomain> MockReadDomain = new();
    protected readonly Mock<ILogger<ReadController>> MockLogger = new();
    protected readonly ReadController Controller;
    protected readonly ClientConfig TestClient;

    protected ReadControllerTestBase()
    {
        Controller = new ReadController(MockReadDomain.Object, MockLogger.Object);

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
    }

    protected ReadRequest CreateValidReadRequest(List<string>? sensorNames = null)
    {
        return new ReadRequest
        {
            SensorNames = sensorNames ?? ["sensor1", "sensor2"],
            Start = DateTimeOffset.UtcNow.AddHours(-1),
            End = DateTimeOffset.UtcNow
        };
    }

    protected ReadResponse CreateMockReadResponse()
    {
        return new ReadResponse
        {
            Results = new Dictionary<string, List<DataPointDto>>
            {
                ["TEST_sensor1"] =
                [
                    new() { Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30), Value = 10.5 },
                    new() { Timestamp = DateTimeOffset.UtcNow.AddMinutes(-15), Value = 11.2 }
                ],
                ["TEST_sensor2"] =
                [
                    new() { Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30), Value = 20.3 },
                    new() { Timestamp = DateTimeOffset.UtcNow.AddMinutes(-15), Value = 21.7 }
                ]
            }
        };
    }
}
