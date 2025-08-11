using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // Registro del CosmosClient para las reseñas
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config["CosmosDb:ConnectionString"]; // ← Corregido: agregado ":"
            return new CosmosClient(connectionString);
        });

        // Registro del BlobServiceClient
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config["AzureBlobStorageConnectionString"];
            return new BlobServiceClient(connectionString);
        });

        // Registro del MongoClient para los cursos
        services.AddSingleton<IMongoClient>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config["CoursesDbConnectionString"];
            return new MongoClient(connectionString);
        });

        // Registro de las funciones como servicios
        services.AddSingleton<ReviewService.Functions.SubmitReview>();
        services.AddSingleton<ReviewService.Functions.GetReviews>();
        services.AddSingleton<ReviewService.Functions.SearchCourses>(); // ← Agregado
    })
    .Build();

host.Run();