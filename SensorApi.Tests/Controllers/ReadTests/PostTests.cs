using Microsoft.AspNetCore.Mvc;
using Moq;
using SensorApi.DTO;
using SensorApi.Parameters;

namespace SensorApi.Tests.Controllers.Read;

public class PostTests : ReadControllerTestBase
{
    [Fact]
    public void Post_WithValidRequest_ReturnsOkResultWithData()
    {
        ReadRequest request = CreateValidReadRequest();
        ReadResponse expectedResponse = CreateMockReadResponse();

        MockReadDomain
            .Setup(x => x.ReadRange(It.IsAny<ReadRequest>(), TestClient.Prefix))
            .Returns(expectedResponse);

        IActionResult result = Controller.Post(request);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        ReadResponse response = Assert.IsType<ReadResponse>(okResult.Value);
        Assert.Equal(expectedResponse.Results.Count, response.Results.Count);

        MockReadDomain.Verify(
            x => x.ReadRange(request, TestClient.Prefix),
            Times.Once);
    }

    [Fact]
    public void Post_WithMultipleSensors_CallsReadDomainWithCorrectPrefix()
    {
        List<string> sensorNames = ["temp", "humidity", "pressure"];
        ReadRequest request = CreateValidReadRequest(sensorNames);
        ReadResponse expectedResponse = CreateMockReadResponse();

        MockReadDomain
            .Setup(x => x.ReadRange(It.IsAny<ReadRequest>(), TestClient.Prefix))
            .Returns(expectedResponse);

        IActionResult result = Controller.Post(request);

        Assert.IsType<OkObjectResult>(result);
        MockReadDomain.Verify(
            x => x.ReadRange(
                It.Is<ReadRequest>(r => r.SensorNames.Count == 3),
                TestClient.Prefix),
            Times.Once);
    }

    [Fact]
    public void Post_WithDateRange_PassesCorrectDatesToReadDomain()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddHours(-2);
        DateTimeOffset end = DateTimeOffset.UtcNow;
        ReadRequest request = new()
        {
            SensorNames = ["sensor1"],
            Start = start,
            End = end
        };
        ReadResponse expectedResponse = CreateMockReadResponse();

        MockReadDomain
            .Setup(x => x.ReadRange(It.IsAny<ReadRequest>(), TestClient.Prefix))
            .Returns(expectedResponse);

        IActionResult result = Controller.Post(request);

        Assert.IsType<OkObjectResult>(result);
        MockReadDomain.Verify(
            x => x.ReadRange(
                It.Is<ReadRequest>(r => r.Start == start && r.End == end),
                TestClient.Prefix),
            Times.Once);
    }

    [Fact]
    public void Post_WhenCalled_LogsInformation()
    {
        ReadRequest request = CreateValidReadRequest();
        ReadResponse expectedResponse = CreateMockReadResponse();

        MockReadDomain
            .Setup(x => x.ReadRange(It.IsAny<ReadRequest>(), TestClient.Prefix))
            .Returns(expectedResponse);

        IActionResult result = Controller.Post(request);

        Assert.IsType<OkObjectResult>(result);
        MockLogger.Verify(
            x => x.Log(
                Microsoft.Extensions.Logging.LogLevel.Information,
                It.IsAny<Microsoft.Extensions.Logging.EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Received ReadRequest")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Post_WithEmptyResults_ReturnsOkWithEmptyDictionary()
    {
        ReadRequest request = CreateValidReadRequest();
        ReadResponse emptyResponse = new()
        {
            Results = []
        };

        MockReadDomain
            .Setup(x => x.ReadRange(It.IsAny<ReadRequest>(), TestClient.Prefix))
            .Returns(emptyResponse);

        IActionResult result = Controller.Post(request);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        ReadResponse response = Assert.IsType<ReadResponse>(okResult.Value);
        Assert.Empty(response.Results);
    }
}
