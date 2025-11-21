namespace SensorApi.DTO;

public class ReadResponse
{
    public Dictionary<string, List<DataPointDto>> Results { get; set; } = [];
}
