using SensorApi.Models;

namespace SensorApi.Tests.Stores;

public class ReadRangeTests : InMemorySensorStoreTestBase
{

    [Fact]
    public void ReadRange_WithNonExistentSensor_ReturnsEmptyList()
    {
        var store = CreateStore();
        var start = DateTimeOffset.UtcNow.AddHours(-1);
        var end = DateTimeOffset.UtcNow;

        var result = store.ReadRange("nonexistent", start, end);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ReadRange_WithValidSensor_ReturnsFilteredData()
    {
        var store = CreateStore();
        var baseTime = DateTimeOffset.UtcNow.AddHours(-2);
        var points = new List<DataPoint>
        {
            new DataPoint(baseTime.AddMinutes(10), 10.0),
            new DataPoint(baseTime.AddMinutes(20), 20.0),
            new DataPoint(baseTime.AddMinutes(30), 30.0),
            new DataPoint(baseTime.AddMinutes(40), 40.0),
            new DataPoint(baseTime.AddMinutes(50), 50.0)
        };
        store.Append("sensor1", points);

        var start = baseTime.AddMinutes(15);
        var end = baseTime.AddMinutes(45);

        var result = store.ReadRange("sensor1", start, end);

        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.True(p.Timestamp >= start && p.Timestamp <= end));
    }

    [Fact]
    public void ReadRange_ReturnsDataInChronologicalOrder()
    {
        var store = CreateStore();
        var baseTime = DateTimeOffset.UtcNow.AddHours(-1);
        var points = new List<DataPoint>
        {
            new DataPoint(baseTime.AddMinutes(30), 30.0),
            new DataPoint(baseTime.AddMinutes(10), 10.0),
            new DataPoint(baseTime.AddMinutes(20), 20.0)
        };
        store.Append("sensor1", points);

        var result = store.ReadRange("sensor1", baseTime, baseTime.AddHours(1));

        Assert.Equal(3, result.Count);
        Assert.True(result[0].Timestamp < result[1].Timestamp);
        Assert.True(result[1].Timestamp < result[2].Timestamp);
    }

    [Fact]
    public void ReadRange_WithMultipleSensors_ReturnsCorrectDataPerSensor()
    {
        var store = CreateStore();
        var start = DateTimeOffset.UtcNow.AddHours(-1);
        var end = DateTimeOffset.UtcNow;

        store.Append("sensor1", CreateDataPoints(3, start));
        store.Append("sensor2", CreateDataPoints(5, start));
        store.Append("sensor3", CreateDataPoints(2, start));

        var sensorNames = new[] { "sensor1", "sensor2", "sensor3" };

        var result = store.ReadRange(sensorNames, start, end);

        Assert.Equal(3, result.Count);
        Assert.Equal(3, result["sensor1"].Count);
        Assert.Equal(5, result["sensor2"].Count);
        Assert.Equal(2, result["sensor3"].Count);
    }

    [Fact]
    public void ReadRange_WithDuplicateSensorNames_ReturnsUniqueResults()
    {
        var store = CreateStore();
        var start = DateTimeOffset.UtcNow.AddHours(-1);
        var end = DateTimeOffset.UtcNow;

        store.Append("sensor1", CreateDataPoints(3, start));

        var sensorNames = new[] { "sensor1", "sensor1", "SENSOR1" }; // Case insensitive duplicates

        var result = store.ReadRange(sensorNames, start, end);

        Assert.Single(result);
    }

    [Fact]
    public void ReadRange_WithMultipleSensors_IsCaseInsensitiveForLookup()
    {
        var store = CreateStore();
        var start = DateTimeOffset.UtcNow.AddHours(-1);
        var end = DateTimeOffset.UtcNow;

        // Store with exact case
        store.Append("sensor1", CreateDataPoints(3, start));
        // ReadRange with multiple sensors uses case-insensitive comparison for deduplication
        var result = store.ReadRange(new[] { "sensor1", "SENSOR1", "Sensor1" }, start, end);
        // Should deduplicate to one entry because of case-insensitive StringComparer
        Assert.Single(result);
        var entry = result.Values.First();
        Assert.Equal(3, entry.Count);
    }

    [Fact]
    public void ReadRange_WithExactBoundaries_IncludesEndpoints()
    {
        var store = CreateStore();
        var start = DateTimeOffset.UtcNow.AddHours(-1);
        var middle = start.AddMinutes(30);
        var end = start.AddHours(1);

        var points = new List<DataPoint>
        {
            new DataPoint(start, 1.0),
            new DataPoint(middle, 2.0),
            new DataPoint(end, 3.0)
        };
        store.Append("sensor1", points);

        var result = store.ReadRange("sensor1", start, end);

        Assert.Equal(3, result.Count);
    }
}
