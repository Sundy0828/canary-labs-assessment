using Microsoft.AspNetCore.Mvc;
using Moq;
using SensorApi.DTO;
using SensorApi.Parameters;

namespace SensorApi.Tests.Controllers.Write;

public class PostTests : WriteControllerTestBase
{
    [Fact]
    public void Post_WithValidRequest_ReturnsOkResultWithStoredCount()
    {
        WriteBatchRequest request = CreateValidWriteBatchRequest();
        int expectedStored = 4; // 2 sensors x 2 data points each

        MockWriteDomain
            .Setup(x => x.WriteBatch(It.IsAny<WriteBatchRequest>(), TestClient.Prefix))
            .Returns(expectedStored);

        MockStore.Setup(x => x.TotalPointsStored).Returns(104);

        IActionResult result = Controller.Post(request);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Type valueType = okResult.Value!.GetType();
        System.Reflection.PropertyInfo? storedProp = valueType.GetProperty("stored");
        System.Reflection.PropertyInfo? totalStoredProp = valueType.GetProperty("totalStored");
        Assert.NotNull(storedProp);
        Assert.NotNull(totalStoredProp);
        Assert.Equal(expectedStored, (int)storedProp.GetValue(okResult.Value)!);
        Assert.Equal(104L, (long)totalStoredProp.GetValue(okResult.Value)!);

        MockWriteDomain.Verify(
            x => x.WriteBatch(request, TestClient.Prefix),
            Times.Once);
    }

    [Fact]
    public void Post_WithMultipleSensors_CallsWriteDomainWithCorrectPrefix()
    {
        WriteBatchRequest request = CreateValidWriteBatchRequest();
        int expectedStored = 4;

        MockWriteDomain
            .Setup(x => x.WriteBatch(It.IsAny<WriteBatchRequest>(), TestClient.Prefix))
            .Returns(expectedStored);

        IActionResult result = Controller.Post(request);

        Assert.IsType<OkObjectResult>(result);
        MockWriteDomain.Verify(
            x => x.WriteBatch(
                It.Is<WriteBatchRequest>(r => r.Sensors.Count == 2),
                TestClient.Prefix),
            Times.Once);
    }

    [Fact]
    public void Post_WithSingleSensor_ReturnsCorrectStoredCount()
    {
        WriteBatchRequest request = CreateSingleSensorRequest("pressure", 10);
        int expectedStored = 10;

        MockWriteDomain
            .Setup(x => x.WriteBatch(It.IsAny<WriteBatchRequest>(), TestClient.Prefix))
            .Returns(expectedStored);

        MockStore.Setup(x => x.TotalPointsStored).Returns(110);

        IActionResult result = Controller.Post(request);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Type valueType = okResult.Value!.GetType();
        System.Reflection.PropertyInfo? storedProp = valueType.GetProperty("stored");
        Assert.NotNull(storedProp);
        Assert.Equal(expectedStored, (int)storedProp.GetValue(okResult.Value)!);

        MockWriteDomain.Verify(
            x => x.WriteBatch(
                It.Is<WriteBatchRequest>(r =>
                    r.Sensors.Count == 1 &&
                    r.Sensors[0].SensorName == "pressure" &&
                    r.Sensors[0].DataPoints.Count == 10),
                TestClient.Prefix),
            Times.Once);
    }

    [Fact]
    public void Post_WhenCalled_LogsInformation()
    {
        WriteBatchRequest request = CreateValidWriteBatchRequest();

        MockWriteDomain
            .Setup(x => x.WriteBatch(It.IsAny<WriteBatchRequest>(), TestClient.Prefix))
            .Returns(4);

        IActionResult result = Controller.Post(request);

        Assert.IsType<OkObjectResult>(result);
        MockLogger.Verify(
            x => x.Log(
                Microsoft.Extensions.Logging.LogLevel.Information,
                It.IsAny<Microsoft.Extensions.Logging.EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Received WriteBatchRequest")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Post_WithNoDataPoints_StoresZeroPoints()
    {
        WriteBatchRequest request = new()
        {
            Sensors =
            [
                new()
                {
                    SensorName = "empty_sensor",
                    DataPoints = []
                }
            ]
        };

        MockWriteDomain
            .Setup(x => x.WriteBatch(It.IsAny<WriteBatchRequest>(), TestClient.Prefix))
            .Returns(0);

        MockStore.Setup(x => x.TotalPointsStored).Returns(100);

        IActionResult result = Controller.Post(request);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Type valueType = okResult.Value!.GetType();
        System.Reflection.PropertyInfo? storedProp = valueType.GetProperty("stored");
        System.Reflection.PropertyInfo? totalStoredProp = valueType.GetProperty("totalStored");
        Assert.NotNull(storedProp);
        Assert.NotNull(totalStoredProp);
        Assert.Equal(0, (int)storedProp.GetValue(okResult.Value)!);
        Assert.Equal(100L, (long)totalStoredProp.GetValue(okResult.Value)!);
    }

    [Fact]
    public void Post_VerifyTotalPointsStoredIsRetrieved()
    {
        WriteBatchRequest request = CreateValidWriteBatchRequest();
        int totalPointsInStore = 250;

        MockWriteDomain
            .Setup(x => x.WriteBatch(It.IsAny<WriteBatchRequest>(), TestClient.Prefix))
            .Returns(4);

        MockStore.Setup(x => x.TotalPointsStored).Returns(totalPointsInStore);

        IActionResult result = Controller.Post(request);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Type valueType = okResult.Value!.GetType();
        System.Reflection.PropertyInfo? totalStoredProp = valueType.GetProperty("totalStored");
        Assert.NotNull(totalStoredProp);
        Assert.Equal(totalPointsInStore, (long)totalStoredProp.GetValue(okResult.Value)!);

        MockStore.Verify(x => x.TotalPointsStored, Times.Once);
    }

    [Fact]
    public void Post_WithLargeBatch_HandlesCorrectly()
    {
        WriteBatchRequest request = new()
        {
            Sensors = []
        };

        // Add 10 sensors with 100 data points each
        for (int i = 0; i < 10; i++)
        {
            List<DataPointDto> dataPoints = [];
            for (int j = 0; j < 100; j++)
            {
                dataPoints.Add(new DataPointDto
                {
                    Timestamp = DateTimeOffset.UtcNow.AddMinutes(-j),
                    Value = (i * 100.0) + j
                });
            }

            request.Sensors.Add(new SensorWriteDto
            {
                SensorName = $"sensor{i}",
                DataPoints = dataPoints
            });
        }

        int expectedStored = 1000;

        MockWriteDomain
            .Setup(x => x.WriteBatch(It.IsAny<WriteBatchRequest>(), TestClient.Prefix))
            .Returns(expectedStored);

        MockStore.Setup(x => x.TotalPointsStored).Returns(1100);

        IActionResult result = Controller.Post(request);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Type valueType = okResult.Value!.GetType();
        System.Reflection.PropertyInfo? storedProp = valueType.GetProperty("stored");
        Assert.NotNull(storedProp);
        Assert.Equal(expectedStored, (int)storedProp.GetValue(okResult.Value)!);

        MockWriteDomain.Verify(
            x => x.WriteBatch(
                It.Is<WriteBatchRequest>(r => r.Sensors.Count == 10),
                TestClient.Prefix),
            Times.Once);
    }
}
