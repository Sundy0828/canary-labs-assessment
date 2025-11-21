using Moq;
using SensorApi.Models;
using SensorApi.Parameters;

namespace SensorApi.Tests.Domains;

public class ReadDomainTests : ReadDomainTestBase
{
    [Fact]
    public void ReadRange_WithValidRequest_ReturnsDataWithoutPrefix()
    {
        var sensorNames = new List<string> { "temperature", "humidity" };
        var request = new ReadRequest
        {
            SensorNames = sensorNames,
            Start = DateTimeOffset.UtcNow.AddHours(-2),
            End = DateTimeOffset.UtcNow
        };

        var mockData = CreateMockStoreData(TestPrefix, sensorNames);
        MockStore
            .Setup(x => x.ReadRange(
                It.IsAny<IEnumerable<string>>(),
                request.Start,
                request.End))
            .Returns(mockData);

        var result = Domain.ReadRange(request, TestPrefix);

        Assert.NotNull(result);
        Assert.Equal(2, result.Results.Count);
        Assert.True(result.Results.ContainsKey("temperature"));
        Assert.True(result.Results.ContainsKey("humidity"));
        Assert.False(result.Results.ContainsKey($"{TestPrefix}_temperature"));
    }

    [Fact]
    public void ReadRange_PrependsClientPrefixToSensorNames()
    {
        var sensorNames = new List<string> { "sensor1", "sensor2" };
        var request = new ReadRequest
        {
            SensorNames = sensorNames,
            Start = DateTimeOffset.UtcNow.AddHours(-1),
            End = DateTimeOffset.UtcNow
        };

        var mockData = CreateMockStoreData(TestPrefix, sensorNames);
        MockStore
            .Setup(x => x.ReadRange(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()))
            .Returns(mockData);

        var result = Domain.ReadRange(request, TestPrefix);

        MockStore.Verify(
            x => x.ReadRange(
                It.Is<IEnumerable<string>>(names =>
                    names.Contains($"{TestPrefix}_sensor1") &&
                    names.Contains($"{TestPrefix}_sensor2")),
                request.Start,
                request.End),
            Times.Once);
    }

    [Fact]
    public void ReadRange_ConvertsDataPointsToDto()
    {
        var sensorNames = new List<string> { "pressure" };
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-30);
        var value = 101.3;

        var request = new ReadRequest
        {
            SensorNames = sensorNames,
            Start = DateTimeOffset.UtcNow.AddHours(-1),
            End = DateTimeOffset.UtcNow
        };

        var mockData = new Dictionary<string, IReadOnlyList<DataPoint>>
        {
            [$"{TestPrefix}_pressure"] = new List<DataPoint>
            {
                new DataPoint(timestamp, value)
            }
        };

        MockStore
            .Setup(x => x.ReadRange(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()))
            .Returns(mockData);

        var result = Domain.ReadRange(request, TestPrefix);

        Assert.Single(result.Results["pressure"]);
        Assert.Equal(timestamp, result.Results["pressure"][0].Timestamp);
        Assert.Equal(value, result.Results["pressure"][0].Value);
    }

    [Fact]
    public void ReadRange_WithEmptyResults_ReturnsEmptyDictionary()
    {
        var request = new ReadRequest
        {
            SensorNames = new List<string> { "nonexistent" },
            Start = DateTimeOffset.UtcNow.AddHours(-1),
            End = DateTimeOffset.UtcNow
        };

        MockStore
            .Setup(x => x.ReadRange(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()))
            .Returns(new Dictionary<string, IReadOnlyList<DataPoint>>());

        var result = Domain.ReadRange(request, TestPrefix);

        Assert.NotNull(result);
        Assert.Empty(result.Results);
    }

    [Fact]
    public void ReadRange_WithMultipleDataPoints_MaintainsAllPoints()
    {
        var sensorNames = new List<string> { "sensor1" };
        var request = new ReadRequest
        {
            SensorNames = sensorNames,
            Start = DateTimeOffset.UtcNow.AddHours(-1),
            End = DateTimeOffset.UtcNow
        };

        var mockData = CreateMockStoreData(TestPrefix, sensorNames, 10);
        MockStore
            .Setup(x => x.ReadRange(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()))
            .Returns(mockData);

        var result = Domain.ReadRange(request, TestPrefix);

        Assert.Equal(10, result.Results["sensor1"].Count);
    }

    [Fact]
    public void ReadRange_RemovesPrefixFromResponseKeys()
    {
        var request = new ReadRequest
        {
            SensorNames = new List<string> { "temp" },
            Start = DateTimeOffset.UtcNow.AddHours(-1),
            End = DateTimeOffset.UtcNow
        };

        var mockData = new Dictionary<string, IReadOnlyList<DataPoint>>
        {
            ["CLIENT_temp"] = new List<DataPoint> { new DataPoint(DateTimeOffset.UtcNow, 25.0) }
        };

        MockStore
            .Setup(x => x.ReadRange(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()))
            .Returns(mockData);

        var result = Domain.ReadRange(request, "CLIENT");

        Assert.True(result.Results.ContainsKey("temp"));
        Assert.False(result.Results.ContainsKey("CLIENT_temp"));
    }
}
