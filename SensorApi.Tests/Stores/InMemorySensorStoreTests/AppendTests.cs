namespace SensorApi.Tests.Stores;

public class AppendTests : InMemorySensorStoreTestBase
{
    [Fact]
    public void Append_WithValidData_IncrementsTotalPointsStored()
    {
        var store = CreateStore();
        var points = CreateDataPoints(5);

        store.Append("sensor1", points);

        Assert.Equal(5, store.TotalPointsStored);
    }

    [Fact]
    public void Append_WithMultipleSensors_TracksAllPoints()
    {
        var store = CreateStore();

        store.Append("sensor1", CreateDataPoints(3));
        store.Append("sensor2", CreateDataPoints(4));
        store.Append("sensor3", CreateDataPoints(2));

        Assert.Equal(9, store.TotalPointsStored);
    }

    [Fact]
    public void Append_WithNullOrWhitespaceSensorName_DoesNotAppend()
    {
        var store = CreateStore();
        var points = CreateDataPoints(5);

        store.Append("", points);
        store.Append("   ", points);
        store.Append(null!, points);

        Assert.Equal(0, store.TotalPointsStored);
    }

    [Fact]
    public void Append_WithSameSensorMultipleTimes_AppendsAllPoints()
    {
        var store = CreateStore();

        store.Append("sensor1", CreateDataPoints(2));
        store.Append("sensor1", CreateDataPoints(3));
        store.Append("sensor1", CreateDataPoints(4));

        Assert.Equal(9, store.TotalPointsStored);
    }
}
