using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Domain.Entities;

[Trait("Category", "Unit")]
public class ServiceTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowCatalogDomainException(string? invalidName)
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());

        // Act & Assert
        var act = () => Service.Create(categoryId, invalidName!, null, 0);
        act.Should().Throw<CatalogDomainException>()
            .WithMessage("O nome do serviço é obrigatório.");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var longName = new string('a', 151);

        // Act & Assert
        var act = () => Service.Create(categoryId, longName, null, 0);
        act.Should().Throw<CatalogDomainException>();
    }

    [Fact]
    public void Create_WithNegativeDisplayOrder_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());

        // Act & Assert
        var act = () => Service.Create(categoryId, "Valid Name", null, -1);
        act.Should().Throw<CatalogDomainException>()
            .WithMessage("A ordem de exibição não pode ser negativa.");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateService()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var service = Service.Create(categoryId, "Original Name", "Original Description", 1);

        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newDisplayOrder = 2;

        // Act
        service.Update(newName, newDescription, newDisplayOrder);

        // Assert
        service.Name.Should().Be(newName);
        service.Description.Should().Be(newDescription);
        service.DisplayOrder.Should().Be(newDisplayOrder);
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithNegativeDisplayOrder_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var service = Service.Create(categoryId, "Test Service", null, 0);

        // Act & Assert
        var act = () => service.Update("Valid Name", null, -5);
        act.Should().Throw<CatalogDomainException>()
            .WithMessage("A ordem de exibição não pode ser negativa.");
    }
}
