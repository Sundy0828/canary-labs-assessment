using System.Net.Http.Json;

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run -- writer|reader [options]");
    return 1;
}

string mode = args[0].ToLowerInvariant();
if (mode == "writer")
{
    return await RunWriter(args);
}

if (mode == "reader")
{
    return await RunReader(args);
}

Console.WriteLine("Unknown mode");
return 1;

// Example: dotnet run -- writer key-client-a-12345 http://localhost:5043 100 10 1
// args: writer <apiKey> <apiBase> <numSensors> <pointsPerSecond> <durationSeconds>
async Task<int> RunWriter(string[] args)
{
    string apiKey = args.Length > 1 ? args[1] : "key-client-a-12345"; // default ClientA
    string baseUrl = args.Length > 2 ? args[2] : "http://localhost:5043";
    int numSensors = args.Length > 3 ? int.Parse(args[3]) : 100; // default 100
    int pointsPerSecond = args.Length > 4 ? int.Parse(args[4]) : 10; // default 10
    int durationSeconds = args.Length > 5 ? int.Parse(args[5]) : 10; // default 10 seconds

    using HttpClient client = new() { BaseAddress = new Uri(baseUrl) };
    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

    List<string> sensors = [];
    for (int i = 0; i < numSensors; i++)
    {
        sensors.Add($"Sensor-{i + 1:D4}");
    }

    double intervalMs = 1000.0 / pointsPerSecond;
    int totalPoints = 0;

    DateTime stopAt = DateTime.UtcNow.AddSeconds(durationSeconds);

    Console.WriteLine($"Writer: sensors={numSensors}, pps={pointsPerSecond}, intervalMs={intervalMs}, duration={durationSeconds}s");

    while (DateTime.UtcNow < stopAt)
    {
        var batch = new
        {
            Sensors = new List<object>()
        };

        foreach (string s in sensors)
        {
            var dp = new[]
            {
                new { Timestamp = DateTimeOffset.UtcNow, Value = Random.Shared.NextDouble() * 100.0 }
            };

            batch.Sensors.Add(new { SensorName = s, DataPoints = dp });
            totalPoints++;
        }

        // send
        try
        {
            HttpResponseMessage resp = await client.PostAsJsonAsync("/write", batch);
            resp.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Write error: {ex.Message}");
        }

        await Task.Delay(TimeSpan.FromMilliseconds(intervalMs));
    }

    Console.WriteLine($"Writer done, total points attempted: {totalPoints}");
    return 0;
}

// Example: dotnet run -- reader key-client-a-12345 http://localhost:5043 Sensor-0001,Sensor-0002 2025-01-01T00:00:00Z 2026-01-01T00:00:00Z
async Task<int> RunReader(string[] args)
{
    string apiKey = args.Length > 1 ? args[1] : "key-client-a-12345"; // default ClientA
    string baseUrl = args.Length > 2 ? args[2] : "http://localhost:5043";
    string sensorsArg = args.Length > 3 ? args[3] : "Sensor-0001";
    string startArg = args.Length > 4 ? args[4] : DateTimeOffset.UtcNow.AddMinutes(-10).ToString("o");
    string endArg = args.Length > 5 ? args[5] : DateTimeOffset.UtcNow.AddMinutes(10).ToString("o");

    string[] sensorNames = sensorsArg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    using HttpClient client = new() { BaseAddress = new Uri(baseUrl) };
    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

    var req = new
    {
        SensorNames = sensorNames,
        Start = DateTimeOffset.Parse(startArg),
        End = DateTimeOffset.Parse(endArg)
    };

    HttpResponseMessage resp = await client.PostAsJsonAsync("/read", req);
    if (!resp.IsSuccessStatusCode)
    {
        Console.WriteLine($"Read failed: {resp.StatusCode}");
        string t = await resp.Content.ReadAsStringAsync();
        Console.WriteLine(t);
        return 1;
    }

    string json = await resp.Content.ReadAsStringAsync();
    Console.WriteLine($"Read success, raw payload length {json.Length}");
    Console.WriteLine(json.Substring(0, Math.Min(json.Length, 2000)));
    return 0;
}
