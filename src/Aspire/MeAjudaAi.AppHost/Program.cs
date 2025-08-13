var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .WithEnvironment("POSTGRES_DB", "MeAjudaAi")
    .WithEnvironment("POSTGRES_USER", "postgres")
    .WithEnvironment("POSTGRES_PASSWORD", "dev123");

var redis = builder.AddRedis("redis")
    .WithDataVolume()
    .WithRedisCommander();

var serviceBus = builder.AddAzureServiceBus("messaging");

var keycloak = builder.AddKeycloak("keycloak", port: 8080)
    .WithDataVolume()
    .WithRealmImport("");

var MeAjudaAiDb = postgres.AddDatabase("MeAjudaAi-db", "MeAjudaAi");

var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
    .WithReference(MeAjudaAiDb)
    .WithReference(redis)
    .WithReference(serviceBus)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName);

// Module APIs (podem rodar como serviços separados ou integrados)
//var userApi = builder.AddProject<Projects>("user-api")
//    .WithReference(userDb)
//    .WithReference(redis)
//    .WithReference(serviceBus)
//    .WithReference(keycloak);

//var providerApi = builder.AddProject<Projects.ServiceMarketplace_ProviderManagement_Api>("provider-api")
//    .WithReference(providerDb)
//    .WithReference(redis)
//    .WithReference(serviceBus);

//var searchApi = builder.AddProject<Projects.ServiceMarketplace_SearchDiscovery_Api>("search-api")
//    .WithReference(searchDb)
//    .WithReference(redis)
//    .WithReference(serviceBus);

//// Notification Service (background service)
//builder.AddProject<Projects.ServiceMarketplace_Notifications_Api>("notification-service")
//    .WithReference(redis)
//    .WithReference(serviceBus);

builder.Build().Run();