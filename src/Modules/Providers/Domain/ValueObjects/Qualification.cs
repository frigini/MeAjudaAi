using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Providers.Domain.ValueObjects;

/// <summary>
/// Qualificação ou habilitação do prestador de serviços.
/// NOTA: IsExpired usa DateTime.UtcNow diretamente pois:
/// 1. É um Value Object - não deve ter dependências injetadas
/// 2. É uma propriedade calculada de conveniência, não parte do modelo de domínio
/// 3. Para lógica de negócio crítica, use métodos de domínio que recebam a data como parâmetro
/// </summary>
public class Qualification : ValueObject
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? IssuingOrganization { get; private set; }
    public DateTime? IssueDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public string? DocumentNumber { get; private set; }

    /// <summary>
    /// Construtor privado para Entity Framework
    /// </summary>
    private Qualification()
    {
        Name = string.Empty;
    }

    public Qualification(
        string name,
        string? description = null,
        string? issuingOrganization = null,
        DateTime? issueDate = null,
        DateTime? expirationDate = null,
        string? documentNumber = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Qualification name cannot be empty", nameof(name));

        // Valida se a data de expiração não é anterior à data de emissão
        if (issueDate.HasValue && expirationDate.HasValue && expirationDate.Value < issueDate.Value)
            throw new ArgumentException("Expiration date cannot be before issue date", nameof(expirationDate));

        Name = name.Trim();
        Description = description?.Trim();
        IssuingOrganization = issuingOrganization?.Trim();
        IssueDate = issueDate;
        ExpirationDate = expirationDate;
        DocumentNumber = documentNumber?.Trim();
    }

    /// <summary>
    /// Checks if the qualification is expired at a specific reference date.
    /// This method enables deterministic testing of expiration logic.
    /// </summary>
    public bool IsExpiredAt(DateTime referenceDate) =>
        ExpirationDate.HasValue && ExpirationDate.Value < referenceDate;

    /// <summary>
    /// Checks if the qualification is currently expired.
    /// Uses DateTime.UtcNow for convenience in production code.
    /// </summary>
    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Description ?? string.Empty;
        yield return IssuingOrganization ?? string.Empty;
        yield return (object?)IssueDate ?? typeof(DateTime?);
        yield return (object?)ExpirationDate ?? typeof(DateTime?);
        yield return DocumentNumber ?? string.Empty;
    }

    public override string ToString() =>
        $"Name: {Name}, Organization: {IssuingOrganization}, Issue Date: {IssueDate}, Expiration: {ExpirationDate}";
}
