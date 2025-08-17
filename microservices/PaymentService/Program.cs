using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System;

var builder = new HostBuilder();

builder.ConfigureFunctionsWebApplication();

// -- INICIO DEL CAMBIO CORREGIDO --
// Añadimos la configuración
builder.ConfigureAppConfiguration(config =>
{
    config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
          .AddEnvironmentVariables();
});

// Añadimos el registro de servicios
builder.ConfigureServices((context, services) =>
{
    // Obtenemos las cadenas de conexión
    var reviewsDbConnectionString = context.Configuration["ReviewsDbConnectionString"];
    var cartDbConnectionString = context.Configuration["CartDbConnectionString"];

    if (string.IsNullOrEmpty(reviewsDbConnectionString) || string.IsNullOrEmpty(cartDbConnectionString))
    {
        throw new InvalidOperationException("Cosmos DB connection strings not set correctly.");
    }
    
    // Registramos una fábrica para los CosmosClients
    services.AddSingleton(provider => new Func<string, CosmosClient>(dbName =>
    {
        if (dbName.Equals("ReviewsDbClient", StringComparison.OrdinalIgnoreCase))
        {
            return new CosmosClient(reviewsDbConnectionString);
        }
        else if (dbName.Equals("CartDbClient", StringComparison.OrdinalIgnoreCase))
        {
            return new CosmosClient(cartDbConnectionString);
        }
        throw new ArgumentException("Invalid client name requested.");
    }));

    services.AddApplicationInsightsTelemetryWorkerService();
    services.ConfigureFunctionsApplicationInsights();
});

var app = builder.Build();
app.Run();