using Microsoft.OpenApi.Models;
using SensorApi.Domains;
using SensorApi.Domains.Interfaces;
using SensorApi.Services;
using SensorApi.Services.Interfaces;
using SensorApi.Stores;
using SensorApi.Stores.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load clients.json configuration
builder.Configuration.AddJsonFile("clients.json", optional: false, reloadOnChange: true);

// add controllers
builder.Services.AddSingleton<IInMemorySensorStore, InMemorySensorStore>();

// add services
builder.Services.AddSingleton<IClientService, ClientService>();

// add domains
builder.Services.AddScoped<IWriteDomain, WriteDomain>();
builder.Services.AddScoped<IReadDomain, ReadDomain>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Api-Key",
        Description = "API Key Authentication"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
