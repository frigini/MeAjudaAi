namespace MeAjudaAi.Shared.Constants;

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