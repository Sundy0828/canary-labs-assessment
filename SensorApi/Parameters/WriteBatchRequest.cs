using SensorApi.DTO;
using System.Collections.Generic;

namespace SensorApi.Parameters;

public class WriteBatchRequest
{
    public List<SensorWriteDto> Sensors { get; set; } = new();
}
