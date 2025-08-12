using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CourseCatalogService.Services;
using Microsoft.Extensions.Configuration;
using System;
using MongoDB.Driver; // Añadido para el driver de MongoDB

var builder = new HostBuilder();

builder.ConfigureFunctionsWebApplication();

builder.ConfigureAppConfiguration(config =>
{
    config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
          .AddEnvironmentVariables();
});

builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<CourseService>();
    services.AddApplicationInsightsTelemetryWorkerService();
    services.ConfigureFunctionsApplicationInsights();

    // -- INICIO DEL CAMBIO --
    // Obtenemos la configuración
    var configuration = context.Configuration;
    var cosmosDbConnectionString = configuration["CosmosDb:ConnectionString"];
    var cosmosDbName = configuration["CosmosDb:DatabaseName"];

    if (string.IsNullOrEmpty(cosmosDbConnectionString))
    {
        throw new InvalidOperationException("CosmosDb:ConnectionString variable not set.");
    }
    if (string.IsNullOrEmpty(cosmosDbName))
    {
        throw new InvalidOperationException("CosmosDb:DatabaseName variable not set.");
    }

    // Registramos MongoClient como un singleton
    services.AddSingleton<IMongoClient>(new MongoClient(cosmosDbConnectionString));

    // Registramos la base de datos como un singleton
    services.AddSingleton<IMongoDatabase>(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        return client.GetDatabase(cosmosDbName);
    });
    // -- FIN DEL CAMBIO --
});

var app = builder.Build();
app.Run();