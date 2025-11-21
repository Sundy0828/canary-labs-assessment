using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SensorApi.Models;
using SensorApi.Stores.Interfaces;

namespace SensorApi.Stores;

public class InMemorySensorStore : IInMemorySensorStore
{
    private readonly ConcurrentDictionary<string, List<DataPoint>> _store = new();
    private readonly ConcurrentDictionary<string, object> _locks = new();
    private long _totalPoints = 0;

    private object GetLock(string sensor)
    {
        return _locks.GetOrAdd(sensor, _ => new object());
    }

    public long TotalPointsStored => System.Threading.Interlocked.Read(ref _totalPoints);

    public void Append(string sensorName, IEnumerable<DataPoint> points)
    {
        if (string.IsNullOrWhiteSpace(sensorName)) return;

        var list = _store.GetOrAdd(sensorName, _ => new List<DataPoint>());
        var lockObj = GetLock(sensorName);

        lock (lockObj)
        {
            foreach (var p in points)
            {
                list.Add(p);
                System.Threading.Interlocked.Increment(ref _totalPoints);
            }
        }
    }

    public IReadOnlyList<DataPoint> ReadRange(string sensorName, DateTimeOffset start, DateTimeOffset end)
    {
        if (!_store.TryGetValue(sensorName, out var list))
        {
            return Array.Empty<DataPoint>();
        }

        var lockObj = GetLock(sensorName);
        DataPoint[] snapshot;
        lock (lockObj)
        {
            snapshot = list.ToArray();
        }

        // filter on snapshot outside lock
        var result = snapshot.Where(p => p.Timestamp >= start && p.Timestamp <= end)
                                .OrderBy(p => p.Timestamp)
                                .ToArray();
        return result;
    }

    public Dictionary<string, IReadOnlyList<DataPoint>> ReadRange(IEnumerable<string> sensorNames, DateTimeOffset start, DateTimeOffset end)
    {
        var dict = new Dictionary<string, IReadOnlyList<DataPoint>>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in sensorNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            dict[s] = ReadRange(s, start, end);
        }
        return dict;
    }
}
