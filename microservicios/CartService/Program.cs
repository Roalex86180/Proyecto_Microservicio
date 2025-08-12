using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration; 
using Microsoft.Azure.Cosmos;
using System;
using System.Net.Http;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    
    // -- INICIO DEL CAMBIO --
    // He eliminado la línea "builder.ConfigureFunctionsWebApplication();"
    // He cambiado el builder a host para que sea consistente
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    // -- FIN DEL CAMBIO --

    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration["CosmosDb:ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("CosmosDb:ConnectionString variable is not set.");
        }
        
        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            return new CosmosClient(connectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        });
        
        services.AddHttpClient();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();