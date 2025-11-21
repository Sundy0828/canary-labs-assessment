using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SensorApi.Services.Interfaces;

namespace SensorApi.Attributes;

public class ApiKeyAuthorizationAttribute : Attribute, IAuthorizationFilter
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        IClientService? clientService = context.HttpContext.RequestServices.GetService<IClientService>();
        if (clientService == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        // Check for API key in header
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out Microsoft.Extensions.Primitives.StringValues apiKeyValues))
        {
            context.Result = new UnauthorizedObjectResult("API Key is missing");
            return;
        }

        string? apiKey = apiKeyValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Result = new UnauthorizedObjectResult("API Key is invalid");
            return;
        }

        // Validate API key
        Models.ClientConfig? client = clientService.GetClientByApiKey(apiKey);
        if (client == null)
        {
            context.Result = new UnauthorizedObjectResult("Invalid API Key");
            return;
        }

        // Store client in HttpContext for use by controller
        context.HttpContext.Items["Client"] = client;
    }
}
