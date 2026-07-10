using MeAjudaAi.Shared.Tests.TestInfrastructure.Helpers;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

[Trait("Category", "Unit")]
[Trait("Layer", "Shared")]
public class DbContextSchemaHelperTests
{
    [Theory]
    [InlineData("UsersDbContext", Schemas.Users)]
    [InlineData("ProvidersDbContext", Schemas.Providers)]
    [InlineData("DocumentsDbContext", Schemas.Documents)]
    [InlineData("ServiceCatalogsDbContext", Schemas.ServiceCatalogs)]
    [InlineData("LocationsDbContext", Schemas.Locations)]
    [InlineData("CommunicationsDbContext", Schemas.Communications)]
    [InlineData("SearchProvidersDbContext", Schemas.SearchProviders)]
    [InlineData("RatingsDbContext", Schemas.Ratings)]
    [InlineData("PaymentsDbContext", Schemas.Payments)]
    [InlineData("BookingsDbContext", Schemas.Bookings)]
    public void GetSchemaName_WithKnownContext_ShouldReturnCorrectSchema(string contextName, string expectedSchema)
    {
        // Arrange & Act
        var schema = DbContextSchemaHelper.GetSchemaName(contextName);

        // Assert
        schema.Should().Be(expectedSchema);
    }

    [Fact]
    public void GetSchemaName_WithUnknownContext_ShouldReturnPublic()
    {
        // Arrange & Act
        var schema = DbContextSchemaHelper.GetSchemaName("UnknownDbContext");

        // Assert
        schema.Should().Be("public");
    }

    [Fact]
    public void GetSchemaName_WithContextType_ShouldReturnCorrectSchema()
    {
        // Arrange
        var contextType = typeof(MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext);

        // Act
        var schema = DbContextSchemaHelper.GetSchemaName(contextType);

        // Assert
        schema.Should().Be(Schemas.Users);
    }

    [Fact]
    public void GetAllModuleSchemas_ShouldNotContainPublic()
    {
        // Arrange & Act
        var schemas = DbContextSchemaHelper.GetAllModuleSchemas();

        // Assert
        schemas.Should().NotContain("public");
    }

    [Fact]
    public void GetAllModuleSchemas_ShouldContainAllKnownSchemas()
    {
        // Arrange & Act
        var schemas = DbContextSchemaHelper.GetAllModuleSchemas();

        // Assert
        schemas.Should().Contain(Schemas.Users);
        schemas.Should().Contain(Schemas.Providers);
        schemas.Should().Contain(Schemas.Documents);
        schemas.Should().Contain(Schemas.ServiceCatalogs);
        schemas.Should().Contain(Schemas.Locations);
        schemas.Should().Contain(Schemas.Communications);
        schemas.Should().Contain(Schemas.SearchProviders);
        schemas.Should().Contain(Schemas.Ratings);
        schemas.Should().Contain(Schemas.Payments);
        schemas.Should().Contain(Schemas.Bookings);
    }

    [Fact]
    public void GetAllModuleSchemas_ShouldHaveNoDuplicates()
    {
        // Arrange & Act
        var schemas = DbContextSchemaHelper.GetAllModuleSchemas();

        // Assert
        schemas.Should().OnlyHaveUniqueItems();
    }
}
