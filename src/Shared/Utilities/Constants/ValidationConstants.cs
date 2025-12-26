namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes de validação baseadas nas constraints reais do banco de dados
/// </summary>
/// <remarks>
/// Valores extraídos das migrations existentes para garantir consistência.
/// </remarks>
public static class ValidationConstants
{
    /// <summary>
    /// Limites para campos do usuário (baseados nas migrations)
    /// </summary>
    public static class UserLimits
    {
        // Baseado em: .HasMaxLength(30) nas migrations
        public const int UsernameMaxLength = 30;

        // Baseado em: .HasMaxLength(254) nas migrations
        public const int EmailMaxLength = 254;

        // Baseado em: .HasMaxLength(100) nas migrations
        public const int FirstNameMaxLength = 100;

        // Baseado em: .HasMaxLength(100) nas migrations
        public const int LastNameMaxLength = 100;

        // Baseado em: .HasMaxLength(50) nas migrations
        public const int KeycloakIdMaxLength = 50;

        // Limites mínimos práticos
        public const int UsernameMinLength = 3;
        public const int FirstNameMinLength = 2;
        public const int LastNameMinLength = 2;
    }

    /// <summary>
    /// Padrões regex utilizados no sistema
    /// </summary>
    public static class Patterns
    {
        // Padrão básico para email (compatível com HTML5)
        public const string Email = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

        // Username: letras, números, underscore, hífen e pontos
        public const string Username = @"^[a-zA-Z0-9_.-]+$";

        // Names: apenas letras e espaços (para FirstName e LastName)
        public const string Name = @"^[a-zA-ZÀ-ÿ\s]+$";

        // GUID/UUID padrão
        public const string Guid = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";

        // Password: pelo menos uma minúscula, uma maiúscula e um número
        public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)";
    }

    /// <summary>
    /// Configurações de paginação (baseadas no uso atual)
    /// </summary>
    public static class Pagination
    {
        public const int DefaultPageNumber = 1;
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
        public const int MinPageSize = 1;
    }

    /// <summary>
    /// Limites para entidades do módulo ServiceCatalogs
    /// </summary>
    public static class CatalogLimits
    {
        // Service Category
        public const int ServiceCategoryNameMaxLength = 100;
        public const int ServiceCategoryDescriptionMaxLength = 500;

        // Service
        public const int ServiceNameMaxLength = 150;
        public const int ServiceDescriptionMaxLength = 1000;
    }
}
