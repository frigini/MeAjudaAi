using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Value object representando o perfil básico de um usuário.
/// </summary>
public class UserProfile : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }
    public PhoneNumber? PhoneNumber { get; }
    public string FullName => $"{FirstName} {LastName}";

    public UserProfile(string firstName, string lastName, PhoneNumber? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        if (PhoneNumber is not null)
            yield return PhoneNumber;
    }
}