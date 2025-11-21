namespace SensorApi.DTO;

public class SensorWriteDto
{
    public string SensorName { get; set; } = string.Empty;
    public List<DataPointDto> DataPoints { get; set; } = [];
}
