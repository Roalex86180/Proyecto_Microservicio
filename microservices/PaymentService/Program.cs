using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration; // Añadido
using Microsoft.Azure.Cosmos; // Añadido
using System;

var builder = new HostBuilder();

builder.ConfigureFunctionsWebApplication();

// -- INICIO DEL CAMBIO --
// Añadimos la configuración
builder.ConfigureAppConfiguration(config =>
{
    config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
          .AddEnvironmentVariables();
});

// Añadimos el registro de servicios
builder.ConfigureServices((context, services) =>
{
    // Obtenemos la cadena de conexión de la configuración
    var cosmosDbConnectionString = context.Configuration["CosmosDb:ConnectionString"];
    
    if (string.IsNullOrEmpty(cosmosDbConnectionString))
    {
        throw new InvalidOperationException("CosmosDb:ConnectionString variable not set.");
    }
    
    // Registramos CosmosClient como un singleton
    services.AddSingleton(new CosmosClient(cosmosDbConnectionString));

    services.AddApplicationInsightsTelemetryWorkerService();
    services.ConfigureFunctionsApplicationInsights();
});

var app = builder.Build();
app.Run();