using SensorApi.DTO;
using SensorApi.Parameters;

namespace SensorApi.Domains.Interfaces;

public interface IReadDomain
{
    ReadResponse ReadRange(ReadRequest request, string clientPrefix);
}
