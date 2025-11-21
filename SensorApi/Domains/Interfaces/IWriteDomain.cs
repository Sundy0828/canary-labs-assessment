using SensorApi.Parameters;

namespace SensorApi.Domains.Interfaces;

public interface IWriteDomain
{
    int WriteBatch(WriteBatchRequest request, string clientPrefix);
}
