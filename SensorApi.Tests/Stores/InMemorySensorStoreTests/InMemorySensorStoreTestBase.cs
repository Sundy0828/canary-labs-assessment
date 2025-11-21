using SensorApi.Models;
using SensorApi.Stores;

namespace SensorApi.Tests.Stores;

public abstract class InMemorySensorStoreTestBase
{
    protected InMemorySensorStore CreateStore()
    {
        return new InMemorySensorStore();
    }

    protected List<DataPoint> CreateDataPoints(int count, DateTimeOffset? baseTime = null)
    {
        var points = new List<DataPoint>();
        var startTime = baseTime ?? DateTimeOffset.UtcNow.AddHours(-1);

        for (int i = 0; i < count; i++)
        {
            points.Add(new DataPoint(startTime.AddMinutes(i * 5), 20.0 + i));
        }

        return points;
    }
}
