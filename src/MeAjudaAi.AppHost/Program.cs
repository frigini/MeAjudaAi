var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

//var redis = builder.AddRedis("redis")
//    .WithDataVolume();

var serviceBus = builder.AddAzureServiceBus("messaging");

// Databases por módulo
var userDb = postgres.AddDatabase("userdb", "users");
//var providerDb = postgres.AddDatabase("providerdb", "providers");
//var catalogDb = postgres.AddDatabase("catalogdb", "catalog");
//var searchDb = postgres.AddDatabase("searchdb", "search");
//var bookingDb = postgres.AddDatabase("bookingdb", "bookings");
//var reviewDb = postgres.AddDatabase("reviewdb", "reviews");
//var paymentDb = postgres.AddDatabase("paymentdb", "payments");

// Main API Gateway
var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
    //.WithReference(redis)
    .WithReference(serviceBus)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

// Module APIs (podem rodar como serviços separados ou integrados)
//var userApi = builder.AddProject<Projects.ServiceMarketplace_UserManagement_Api>("user-api")
//    .WithReference(userDb)
//    .WithReference(redis)
//    .WithReference(serviceBus);

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
