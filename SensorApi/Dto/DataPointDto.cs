using System.Text.Json.Serialization;
using SensorApi.Models;

namespace SensorApi.DTO;

public class DataPointDto
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }

    public DataPoint ToDataPoint()
    {
        return new(Timestamp, Value);
    }
}

