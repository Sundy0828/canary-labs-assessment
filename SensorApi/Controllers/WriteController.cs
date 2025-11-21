using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SensorApi.Attributes;
using SensorApi.Domains.Interfaces;
using SensorApi.Models;
using SensorApi.Parameters;
using SensorApi.Stores.Interfaces;
using SensorApi.Utility;

namespace SensorApi.Controllers;

[ApiController]
[Route("[controller]")]
[ApiKeyAuthorization]
public class WriteController(IWriteDomain writeDomain, IInMemorySensorStore store, ILogger<WriteController> logger) : ControllerBase
{
    private readonly IWriteDomain _writeDomain = writeDomain;
    private readonly IInMemorySensorStore _store = store;
    private readonly ILogger<WriteController> _logger = logger;

    [HttpPost]
    public IActionResult Post([FromBody] WriteBatchRequest request)
    {
        ClientConfig? client = HttpContext.Items["Client"] as ClientConfig;

        _logger.LogInformation("Received WriteBatchRequest for client: {ClientName}, sensors: {SensorNames}",
            client!.Name, string.Join(", ", request.Sensors.Select(s => s.SensorName)));

        Guard.AgainstNull(request, nameof(request));
        Guard.AgainstNullOrEmpty(request.Sensors, nameof(request.Sensors));

        int stored = _writeDomain.WriteBatch(request, client.Prefix);

        return Ok(new { stored, totalStored = _store.TotalPointsStored });
    }
}
