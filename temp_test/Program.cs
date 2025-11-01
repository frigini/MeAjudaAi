using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;

Console.WriteLine("Testing BusinessProfile nullability changes...");

// Test 1: Valid request with BusinessProfile
var validRequest = new CreateProviderRequest
{
    UserId = Guid.NewGuid().ToString(),
    Name = "Test Provider",
    Type = EProviderType.Individual,
    BusinessProfile = new BusinessProfileDto(
        LegalName: "Test Legal Name",
        FantasyName: "Test Fantasy",
        Description: "Test Description",
        ContactInfo: new ContactInfoDto(
            Email: "test@example.com",
            PhoneNumber: "+55 11 99999-9999",
            Website: "https://www.test.com"
        ),
        PrimaryAddress: new AddressDto(
            Street: "Test Street",
            Number: "123",
            Complement: null,
            Neighborhood: "Centro",
            City: "Test City",
            State: "TS",
            ZipCode: "12345-678",
            Country: "Brasil"
        )
    )
};

try
{
    var validCommand = validRequest.ToCommand();
    Console.WriteLine("✅ Valid request with BusinessProfile: SUCCESS");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Valid request failed: {ex.Message}");
}

// Test 2: Invalid request with null BusinessProfile
var invalidRequest = new CreateProviderRequest
{
    UserId = Guid.NewGuid().ToString(),
    Name = "Test Provider", 
    Type = EProviderType.Individual,
    BusinessProfile = null // This should throw ArgumentNullException
};

try
{
    var invalidCommand = invalidRequest.ToCommand();
    Console.WriteLine("❌ Invalid request with null BusinessProfile: SHOULD HAVE FAILED");
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"✅ Invalid request correctly caught: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"❓ Unexpected exception: {ex.Message}");
}

Console.WriteLine("Test completed!");
