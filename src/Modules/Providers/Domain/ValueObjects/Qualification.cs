using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Providers.Domain.ValueObjects;

/// <summary>
/// Qualificação ou habilitação do prestador de serviços.
/// </summary>
public class Qualification : ValueObject
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? IssuingOrganization { get; private set; }
    public DateTime? IssueDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public string? DocumentNumber { get; private set; }

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

        Name = name.Trim();
        Description = description?.Trim();
        IssuingOrganization = issuingOrganization?.Trim();
        IssueDate = issueDate;
        ExpirationDate = expirationDate;
        DocumentNumber = documentNumber?.Trim();
    }

    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Description ?? string.Empty;
        yield return IssuingOrganization ?? string.Empty;
        yield return IssueDate ?? DateTime.MinValue;
        yield return ExpirationDate ?? DateTime.MinValue;
        yield return DocumentNumber ?? string.Empty;
    }

    public override string ToString() => 
        $"Name: {Name}, Organization: {IssuingOrganization}, Issue Date: {IssueDate}, Expiration: {ExpirationDate}";
}
