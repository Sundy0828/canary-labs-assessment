using Moq;
using SensorApi.DTO;
using SensorApi.Models;
using SensorApi.Parameters;

namespace SensorApi.Tests.Domains;

public class WriteDomainTests : WriteDomainTestBase
{
    [Fact]
    public void WriteBatch_WithValidRequest_ReturnsCorrectCount()
    {
        var request = CreateWriteBatchRequest(2, 3);

        var result = Domain.WriteBatch(request, TestPrefix);

        Assert.Equal(6, result); // 2 sensors * 3 points
    }

    [Fact]
    public void WriteBatch_CallsStoreAppendForEachSensor()
    {
        var request = CreateWriteBatchRequest(3, 2);

        var result = Domain.WriteBatch(request, TestPrefix);

        MockStore.Verify(
            x => x.Append(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<DataPoint>>()),
            Times.Exactly(3));
    }

    [Fact]
    public void WriteBatch_PrependsClientPrefixToSensorNames()
    {
        var request = new WriteBatchRequest
        {
            Sensors = new List<SensorWriteDto>
            {
                new SensorWriteDto
                {
                    SensorName = "temperature",
                    DataPoints = new List<DataPointDto>
                    {
                        new DataPointDto { Timestamp = DateTimeOffset.UtcNow, Value = 25.0 }
                    }
                }
            }
        };

        var result = Domain.WriteBatch(request, TestPrefix);

        MockStore.Verify(
            x => x.Append(
                $"{TestPrefix}_temperature",
                It.IsAny<IEnumerable<DataPoint>>()),
            Times.Once);
    }

    [Fact]
    public void WriteBatch_ConvertsDataPointDtoToDataPoint()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var value = 42.5;
        var request = new WriteBatchRequest
        {
            Sensors = new List<SensorWriteDto>
            {
                new SensorWriteDto
                {
                    SensorName = "sensor1",
                    DataPoints = new List<DataPointDto>
                    {
                        new DataPointDto { Timestamp = timestamp, Value = value }
                    }
                }
            }
        };

        DataPoint? capturedPoint = null;
        MockStore
            .Setup(x => x.Append(It.IsAny<string>(), It.IsAny<IEnumerable<DataPoint>>()))
            .Callback<string, IEnumerable<DataPoint>>((_, points) =>
            {
                capturedPoint = points.FirstOrDefault();
            });

        var result = Domain.WriteBatch(request, TestPrefix);

        Assert.NotNull(capturedPoint);
        Assert.Equal(timestamp, capturedPoint.Timestamp);
        Assert.Equal(value, capturedPoint.Value);
    }

    [Fact]
    public void WriteBatch_WithEmptySensorList_ReturnsZero()
    {
        var request = new WriteBatchRequest { Sensors = new List<SensorWriteDto>() };

        var result = Domain.WriteBatch(request, TestPrefix);

        Assert.Equal(0, result);
        MockStore.Verify(
            x => x.Append(It.IsAny<string>(), It.IsAny<IEnumerable<DataPoint>>()),
            Times.Never);
    }

    [Fact]
    public void WriteBatch_WithEmptyDataPoints_ReturnsZero()
    {
        var request = new WriteBatchRequest
        {
            Sensors = new List<SensorWriteDto>
            {
                new SensorWriteDto
                {
                    SensorName = "sensor1",
                    DataPoints = new List<DataPointDto>()
                }
            }
        };

        var result = Domain.WriteBatch(request, TestPrefix);

        Assert.Equal(0, result);
    }

    [Fact]
    public void WriteBatch_WithMultipleSensors_AppendsAllCorrectly()
    {
        var request = CreateWriteBatchRequest(5, 10);

        var result = Domain.WriteBatch(request, TestPrefix);

        Assert.Equal(50, result); // 5 sensors * 10 points
        MockStore.Verify(
            x => x.Append(
                It.Is<string>(s => s.StartsWith($"{TestPrefix}_")),
                It.Is<IEnumerable<DataPoint>>(p => p.Count() == 10)),
            Times.Exactly(5));
    }

    [Fact]
    public void WriteBatch_WithDifferentPrefix_UsesThatPrefix()
    {
        var request = new WriteBatchRequest
        {
            Sensors = new List<SensorWriteDto>
            {
                new SensorWriteDto
                {
                    SensorName = "sensor",
                    DataPoints = new List<DataPointDto>
                    {
                        new DataPointDto { Timestamp = DateTimeOffset.UtcNow, Value = 1.0 }
                    }
                }
            }
        };

        var result = Domain.WriteBatch(request, "CUSTOM");

        MockStore.Verify(
            x => x.Append("CUSTOM_sensor", It.IsAny<IEnumerable<DataPoint>>()),
            Times.Once);
    }
}
