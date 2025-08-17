// src/PaymentService/Program.cs

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication() // ← CAMBIO: Usar ConfigureFunctionsWebApplication en lugar de ConfigureFunctionsWorkerDefaults
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Leer las dos cadenas de conexión
        var reviewsDbConnectionString = Environment.GetEnvironmentVariable("ReviewsDbConnectionString");
        var cartDbConnectionString = Environment.GetEnvironmentVariable("CartDbConnectionString");

        if (string.IsNullOrEmpty(reviewsDbConnectionString))
        {
            throw new ArgumentNullException("ReviewsDbConnectionString", "The ReviewsDbConnectionString environment variable is not set.");
        }

        if (string.IsNullOrEmpty(cartDbConnectionString))
        {
            throw new ArgumentNullException("CartDbConnectionString", "The CartDbConnectionString environment variable is not set.");
        }

        // Registrar un Factory para crear los clientes de Cosmos DB
        services.AddSingleton<Func<string, CosmosClient>>(provider => dbName =>
        {
            if (dbName.Equals("ReviewsDbClient", StringComparison.OrdinalIgnoreCase))
            {
                return new CosmosClient(reviewsDbConnectionString);
            }
            else if (dbName.Equals("CartDbClient", StringComparison.OrdinalIgnoreCase))
            {
                return new CosmosClient(cartDbConnectionString);
            }
            else
            {
                throw new ArgumentException($"Unknown Cosmos DB client key: {dbName}");
            }
        });
    })
    .Build();

host.Run();