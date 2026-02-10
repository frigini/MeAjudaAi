namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes para endpoints da API organizados por módulo
/// </summary>
/// <remarks>
/// Baseado nos endpoints realmente existentes no projeto.
/// Mantém apenas o que está implementado para evitar confusão.
/// </remarks>
public static class ApiEndpoints
{
    /// <summary>
    /// Endpoints do módulo de usuários (UserAdmin)
    /// </summary>
    /// <remarks>
    /// Todos estes endpoints existem em UserAdmin/ e estão funcionais.
    /// </remarks>
    public static class Users
    {
        // Endpoints existentes e implementados
        public const string Create = "/";                    // POST   CreateUserEndpoint
        public const string GetAll = "/";                   // GET    GetUsersEndpoint  
        public const string GetById = "/{id:guid}";         // GET    GetUserByIdEndpoint
        public const string Delete = "/{id:guid}";          // DELETE DeleteUserEndpoint
        public const string GetByEmail = "/by-email/{email}"; // GET    GetUserByEmailEndpoint
        public const string UpdateProfile = "/{id:guid}/profile"; // PUT    UpdateUserProfileEndpoint
    }

    /// <summary>
    /// Endpoints do módulo de prestadores de serviços (ProviderAdmin)
    /// </summary>
    /// <remarks>
    /// Todos estes endpoints existem em ProviderAdmin/ e estão funcionais.
    /// </remarks>
    public static class Providers
    {
        // Endpoints existentes e implementados
        public const string Create = "/";                    // POST   CreateProviderEndpoint
        public const string GetAll = "/";                   // GET    GetProvidersEndpoint  
        public const string GetById = "/{id:guid}";         // GET    GetProviderByIdEndpoint
        public const string Delete = "/{id:guid}";          // DELETE DeleteProviderEndpoint
        public const string GetByUserId = "/by-user/{userId:guid}"; // GET GetProviderByUserIdEndpoint
        public const string GetByCity = "/by-city/{city}";  // GET    GetProvidersByCityEndpoint
        public const string GetByState = "/by-state/{state}"; // GET   GetProvidersByStateEndpoint
        public const string GetByType = "/by-type/{type}";  // GET    GetProvidersByTypeEndpoint
        public const string GetByVerificationStatus = "/by-verification-status/{status}"; // GET GetProvidersByVerificationStatusEndpoint
        public const string UpdateProfile = "/{id:guid}/profile"; // PUT UpdateProviderProfileEndpoint
        public const string UpdateVerificationStatus = "/{id:guid}/verification-status"; // PUT UpdateVerificationStatusEndpoint
        public const string AddDocument = "/{id:guid}/documents"; // POST AddDocumentEndpoint
        public const string RemoveDocument = "/{id:guid}/documents/{documentType}"; // DELETE RemoveDocumentEndpoint
        public const string RequireBasicInfoCorrection = "/{id:guid}/require-basic-info-correction"; // POST RequireBasicInfoCorrectionEndpoint
        public const string GetPublicById = "{id:guid}/public"; // GET GetPublicProviderByIdEndpoint
    }

    /// <summary>
    /// Endpoints de sistema (Health checks e monitoramento)
    /// </summary>
    /// <remarks>
    /// Endpoints básicos que toda aplicação ASP.NET Core possui.
    /// </remarks>
    public static class System
    {
        public const string Health = "/health";
        public const string HealthReady = "/health/ready";
        public const string HealthLive = "/health/live";
    }
}
