namespace MeAjudaAi.Tests.Shared.Constants;

/// <summary>
/// Constantes para dados de teste comuns em vários módulos
/// </summary>
public static class TestData
{
    // Usuários padrão para testes
    public static class Users
    {
        public const string AdminUserId = "admin-test-id";
        public const string AdminUsername = "admin";
        public const string AdminEmail = "admin@test.com";

        public const string RegularUserId = "user-test-id";
        public const string RegularUsername = "testuser";
        public const string RegularEmail = "user@test.com";

        public const string TestPassword = "TestPassword123!";
    }

    // Tokens e autenticação
    public static class Auth
    {
        public const string ValidTestToken = "Bearer test-token-valid";
        public const string InvalidTestToken = "Bearer test-token-invalid";
        public const string ExpiredTestToken = "Bearer test-token-expired";
    }

    // Configurações de paginação comuns
    public static class Pagination
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        public const int FirstPage = 1;
    }

    // Timeouts e configurações de performance
    public static class Performance
    {
        public static readonly TimeSpan ShortTimeout = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan MediumTimeout = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan LongTimeout = TimeSpan.FromMinutes(2);
    }
}