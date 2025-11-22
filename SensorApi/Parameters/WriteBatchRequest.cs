using SensorApi.DTO;

namespace SensorApi.Parameters;

public class WriteBatchRequest
{
    public List<SensorWriteDto> Sensors { get; set; } = new();
}
