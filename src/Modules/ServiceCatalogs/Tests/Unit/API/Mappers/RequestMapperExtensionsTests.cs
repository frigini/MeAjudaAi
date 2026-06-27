using MeAjudaAi.Modules.ServiceCatalogs.API.Mappers;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.ServiceCategory;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.API.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "API")]
public class RequestMapperExtensionsTests
{
    // ========== Service Tests ==========

    [Fact]
    public void ToCommand_CreateServiceRequest_ShouldMapAllProperties()
    {
        // Arrange
        var request = new CreateServiceRequest
        {
            CategoryId = Guid.NewGuid(),
            Name = "Haircut",
            Description = "Professional haircut service",
            DisplayOrder = 1
        };

        // Act
        var command = request.ToCommand();

        // Assert
        command.Should().NotBeNull();
        command.CategoryId.Should().Be(request.CategoryId);
        command.Name.Should().Be("Haircut");
        command.Description.Should().Be("Professional haircut service");
        command.DisplayOrder.Should().Be(1);
    }

    [Fact]
    public void ToCommand_CreateServiceRequest_WithNullDescription_ShouldMapNull()
    {
        // Arrange
        var request = new CreateServiceRequest
        {
            CategoryId = Guid.NewGuid(),
            Name = "Simple Service",
            Description = null
        };

        // Act
        var command = request.ToCommand();

        // Assert
        command.Description.Should().BeNull();
    }

    [Fact]
    public void ToCommand_UpdateServiceRequest_ShouldMapAllPropertiesIncludingId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateServiceRequest
        {
            Name = "Updated Service",
            Description = "Updated description",
            DisplayOrder = 5
        };

        // Act
        var command = request.ToCommand(id);

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
        command.Name.Should().Be("Updated Service");
        command.Description.Should().Be("Updated description");
        command.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void ToDeleteCommand_Service_ShouldMapId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = id.ToDeleteCommand();

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
    }

    [Fact]
    public void ToActivateCommand_Service_ShouldMapId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = id.ToActivateCommand();

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
    }

    [Fact]
    public void ToDeactivateCommand_Service_ShouldMapId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = id.ToDeactivateCommand();

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
    }

    [Fact]
    public void ToCommand_ChangeServiceCategoryRequest_ShouldMapAllProperties()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var request = new ChangeServiceCategoryRequest
        {
            NewCategoryId = newCategoryId
        };

        // Act
        var command = request.ToCommand(serviceId);

        // Assert
        command.Should().NotBeNull();
        command.ServiceId.Should().Be(serviceId);
        command.NewCategoryId.Should().Be(newCategoryId);
    }

    // ========== ServiceCategory Tests ==========

    [Fact]
    public void ToCommand_CreateServiceCategoryRequest_ShouldMapAllProperties()
    {
        // Arrange
        var request = new CreateServiceCategoryRequest(
            Name: "Beauty",
            Description: "Beauty services",
            DisplayOrder: 2);

        // Act
        var command = request.ToCommand();

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("Beauty");
        command.Description.Should().Be("Beauty services");
        command.DisplayOrder.Should().Be(2);
    }

    [Fact]
    public void ToCommand_UpdateServiceCategoryRequest_ShouldMapAllPropertiesIncludingId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateServiceCategoryRequest(
            Name: "Updated Category",
            Description: "Updated description",
            DisplayOrder: 10);

        // Act
        var command = request.ToCommand(id);

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
        command.Name.Should().Be("Updated Category");
        command.Description.Should().Be("Updated description");
        command.DisplayOrder.Should().Be(10);
    }

    [Fact]
    public void ToDeleteCategoryCommand_ShouldMapId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = id.ToDeleteCategoryCommand();

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
    }

    [Fact]
    public void ToActivateCategoryCommand_ShouldMapId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = id.ToActivateCategoryCommand();

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
    }

    [Fact]
    public void ToDeactivateCategoryCommand_ShouldMapId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = id.ToDeactivateCategoryCommand();

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
    }
}
