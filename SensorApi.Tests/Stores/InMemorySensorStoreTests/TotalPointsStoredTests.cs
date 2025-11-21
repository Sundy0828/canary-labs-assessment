using SensorApi.Models;

namespace SensorApi.Tests.Stores;

public class TotalPointsStoredTests : InMemorySensorStoreTestBase
{
    [Fact]
    public void TotalPointsStored_IsThreadSafe()
    {
        var store = CreateStore();
        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    store.Append($"sensor_{i}", CreateDataPoints(1));
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        Assert.Equal(1000, store.TotalPointsStored);
    }
}
