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
        // Registro del CosmosClient para las reseñas (SIN CAMBIOS)
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config["CosmosDb:ConnectionString"];
            return new CosmosClient(connectionString);
        });

        // Registro del BlobServiceClient (SIN CAMBIOS)
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config["AzureBlobStorageConnectionString"];
            return new BlobServiceClient(connectionString);
        });

        // Registro del MongoClient para los cursos (SIN CAMBIOS)
        services.AddSingleton<IMongoClient>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config["CoursesDbConnectionString"];
            return new MongoClient(connectionString);
        });
        
        // [NUEVO] Registro del IMongoDatabase para la base de datos de cursos.
        // Esto permite inyectar IMongoDatabase directamente en las funciones.
        services.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            // [IMPORTANTE] Reemplaza "NombreDeTuBaseDeDatosDeCursos" con el nombre real de tu DB
            var dbName = "NombreDeTuBaseDeDatosDeCursos"; 
            return client.GetDatabase(dbName);
        });

        // Registro de las funciones como servicios
        services.AddSingleton<ReviewService.Functions.SubmitReview>();
        services.AddSingleton<ReviewService.Functions.GetReviews>();
        services.AddSingleton<ReviewService.Functions.SearchCourses>();
        // [NUEVO] Se registra tu nueva función GetPurchasedCourses.
        services.AddSingleton<ReviewService.Functions.GetPurchasedCourses>();
    })
    .Build();

host.Run();