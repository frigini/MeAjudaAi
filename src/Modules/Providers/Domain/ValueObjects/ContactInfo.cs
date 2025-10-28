using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Providers.Domain.ValueObjects;

/// <summary>
/// Informações de contato do prestador de serviços.
/// </summary>
public class ContactInfo : ValueObject
{
    public string Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Website { get; private set; }

    public ContactInfo(string email, string? phoneNumber = null, string? website = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        Email = email.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Website = website?.Trim();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Email;
        yield return PhoneNumber ?? string.Empty;
        yield return Website ?? string.Empty;
    }

    public override string ToString() => $"Email: {Email}, Phone: {PhoneNumber}, Website: {Website}";
}
