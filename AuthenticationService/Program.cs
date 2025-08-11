using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AuthenticationService
{
    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureAppConfiguration(appConfig =>
                {
                    appConfig.AddEnvironmentVariables();
                })
                .ConfigureServices(services =>
                {
                    // Configurar logging
                    services.AddLogging();
                    
                    // Configurar CosmosClient
                    services.AddSingleton(sp =>
                    {
                        var configuration = sp.GetService<IConfiguration>();
                        var logger = sp.GetService<ILogger<Program>>();
                        
                        var connectionString = configuration.GetValue<string>("CosmosDbConnectionString");
                        
                        if (logger != null)
                        {
                            logger.LogInformation($"CosmosDbConnectionString configured: {!string.IsNullOrEmpty(connectionString)}");
                        }
                        
                        if (string.IsNullOrEmpty(connectionString))
                        {
                            // Para desarrollo local con Azurite
                            connectionString = "AccountEndpoint=http://azurite:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;";
                            if (logger != null)
                            {
                                logger.LogWarning("Using default Azurite connection string");
                            }
                        }
                        
                        try
                        {
                            var cosmosClientOptions = new CosmosClientOptions()
                            {
                                RequestTimeout = TimeSpan.FromSeconds(60),
                                ConnectionMode = ConnectionMode.Gateway
                            };
                            
                            return new CosmosClient(connectionString, cosmosClientOptions);
                        }
                        catch (Exception ex)
                        {
                            if (logger != null)
                            {
                                logger.LogError(ex, "Failed to create CosmosClient");
                            }
                            throw;
                        }
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .Build();

            await host.RunAsync();
        }
    }
}