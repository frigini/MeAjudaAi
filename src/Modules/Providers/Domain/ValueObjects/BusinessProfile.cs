using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Providers.Domain.ValueObjects;

/// <summary>
/// Perfil empresarial do prestador de serviços.
/// Encapsula informações de identidade empresarial e contato primário.
/// </summary>
public class BusinessProfile : ValueObject
{
    public string LegalName { get; private set; }
    public string? FantasyName { get; private set; }
    public string? Description { get; private set; }
    public ContactInfo ContactInfo { get; private set; }
    public Address PrimaryAddress { get; private set; }

    /// <summary>
    /// Construtor privado para Entity Framework
    /// </summary>
    private BusinessProfile()
    {
        LegalName = string.Empty;
        ContactInfo = null!;
        PrimaryAddress = null!;
    }

    public BusinessProfile(
        string legalName,
        ContactInfo contactInfo,
        Address primaryAddress,
        string? fantasyName = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(legalName))
            throw new ArgumentException("Legal name cannot be empty", nameof(legalName));

        LegalName = legalName.Trim();
        FantasyName = fantasyName?.Trim();
        Description = description?.Trim();
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        PrimaryAddress = primaryAddress ?? throw new ArgumentNullException(nameof(primaryAddress));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return LegalName;
        yield return FantasyName ?? string.Empty;
        yield return Description ?? string.Empty;
        yield return ContactInfo;
        yield return PrimaryAddress;
    }

    public override string ToString() =>
        $"Legal Name: {LegalName}, Fantasy Name: {FantasyName}, Description: {Description}";
}
