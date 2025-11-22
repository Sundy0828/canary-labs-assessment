using Microsoft.AspNetCore.Mvc;
using SensorApi.Attributes;
using SensorApi.Domains.Interfaces;
using SensorApi.Models;
using SensorApi.Parameters;
using SensorApi.Utility;

namespace SensorApi.Controllers;

[ApiController]
[Route("[controller]")]
[ApiKeyAuthorization]
public class ReadController(IReadDomain readDomain, ILogger<ReadController> logger) : ControllerBase
{
    private readonly IReadDomain _readDomain = readDomain;
    private readonly ILogger<ReadController> _logger = logger;

    [HttpPost]
    public IActionResult Post([FromBody] ReadRequest request)
    {
        ClientConfig? client = HttpContext.Items["Client"] as ClientConfig;

        _logger.LogInformation("Received ReadRequest for client: {ClientName}, sensors: {SensorNames}",
            client!.Name, string.Join(", ", request.SensorNames));

        Guard.AgainstNull(request, nameof(request));
        Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(request.SensorNames, nameof(request.SensorNames));

        DTO.ReadResponse response = _readDomain.ReadRange(request, client.Prefix);

        return Ok(response);
    }
}
