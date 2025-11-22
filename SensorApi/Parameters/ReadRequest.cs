namespace SensorApi.Parameters;

public class ReadRequest
{
    public List<string> SensorNames { get; set; } = new();
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
}
