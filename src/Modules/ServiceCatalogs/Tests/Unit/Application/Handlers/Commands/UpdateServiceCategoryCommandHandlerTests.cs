using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class UpdateServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<ServiceCategory, ServiceCategoryId>> _repositoryMock;
    private readonly Mock<IServiceCategoryQueries> _queriesMock;
    private readonly UpdateServiceCategoryCommandHandler _handler;

    public UpdateServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        _queriesMock = new Mock<IServiceCategoryQueries>();
        
        _uowMock.Setup(x => x.GetRepository<ServiceCategory, ServiceCategoryId>()).Returns(_repositoryMock.Object);
        _handler = new UpdateServiceCategoryCommandHandler(_uowMock.Object, _queriesMock.Object, NullLogger<UpdateServiceCategoryCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().WithName("Original Name").Build();
        var command = new UpdateServiceCategoryCommand(category.Id.Value, "Updated Name", "Updated Description", 2);

        _repositoryMock
            .Setup(x => x.TryFindAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _queriesMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be("Updated Name");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

// Minimal Builder for test
internal class ServiceCategoryBuilder
{
    private ServiceCategory _category = ServiceCategory.Create("Default", "Desc", 1);
    public ServiceCategoryBuilder WithName(string name) { _category.Update(name, _category.Description, _category.DisplayOrder); return this; }
    public ServiceCategoryBuilder AsActive() { _category.Activate(); return this; }
    public ServiceCategoryBuilder AsInactive() { _category.Deactivate(); return this; }
    public ServiceCategory Build() => _category;
}
