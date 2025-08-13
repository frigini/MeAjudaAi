using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Domain.ValuleObjects;

public record UserProfile : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }
    public string FullName => $"{FirstName} {LastName}";

    public UserProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }
}