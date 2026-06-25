using DTOs = MeAjudaAi.Modules.Providers.Application.DTOs;
using ContactInfoDto = MeAjudaAi.Modules.Providers.Application.DTOs.ContactInfoDto;
using AddressDto = MeAjudaAi.Modules.Providers.Application.DTOs.AddressDto;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;

[ExcludeFromCodeCoverage]
public class BusinessProfileDtoBuilder
{
    private string _legalName = "Test Legal Name";
    private string? _fantasyName = "Test Fantasy Name";
    private string? _description = "Test Description";
    private ContactInfoDto? _contactInfo;
    private AddressDto? _primaryAddress;

    public static BusinessProfileDtoBuilder Create() => new();

    public static BusinessProfileDtoBuilder CreateValid() => new();

    public BusinessProfileDtoBuilder WithLegalName(string legalName)
    {
        _legalName = legalName;
        return this;
    }

    public BusinessProfileDtoBuilder WithFantasyName(string? fantasyName)
    {
        _fantasyName = fantasyName;
        return this;
    }

    public BusinessProfileDtoBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public BusinessProfileDtoBuilder WithContactInfo(ContactInfoDto contactInfo)
    {
        _contactInfo = contactInfo;
        return this;
    }

    public BusinessProfileDtoBuilder WithAddress(AddressDto address)
    {
        _primaryAddress = address;
        return this;
    }

    public BusinessProfileDtoBuilder WithEmail(string email)
    {
        _contactInfo = new ContactInfoDto(email, "11999999999", null);
        return this;
    }

    public DTOs.BusinessProfileDto Build()
    {
        return new DTOs.BusinessProfileDto(
            LegalName: _legalName,
            FantasyName: _fantasyName,
            Description: _description,
            ContactInfo: _contactInfo ?? new ContactInfoDto("test@provider.com", "11999999999", null),
            PrimaryAddress: _primaryAddress ?? CreateDefaultAddress());
    }

    private static AddressDto CreateDefaultAddress()
    {
        return new AddressDto(
            Street: "Test Street",
            Number: "123",
            Complement: null,
            Neighborhood: "Test Neighborhood",
            City: "Test City",
            State: "TS",
            ZipCode: "12345678",
            Country: "Brasil");
    }
}
