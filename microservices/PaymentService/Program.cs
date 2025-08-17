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
    // Obtenemos las cadenas de conexión de la configuración para cada cuenta
    var reviewsDbConnectionString = context.Configuration["ReviewsDbConnectionString"];
    var cartDbConnectionString = context.Configuration["CartDbConnectionString"];

    if (string.IsNullOrEmpty(reviewsDbConnectionString))
    {
        throw new InvalidOperationException("ReviewsDbConnectionString variable not set.");
    }

    if (string.IsNullOrEmpty(cartDbConnectionString))
    {
        throw new InvalidOperationException("CartDbConnectionString variable not set.");
    }
    
    // Registramos dos instancias de CosmosClient como singletons
    // Se registran como servicios con clave para poder inyectarlos por separado
    services.AddSingleton("ReviewsDbClient", new CosmosClient(reviewsDbConnectionString));
    services.AddSingleton("CartDbClient", new CosmosClient(cartDbConnectionString));

    services.AddApplicationInsightsTelemetryWorkerService();
    services.ConfigureFunctionsApplicationInsights();
});

var app = builder.Build();
app.Run();