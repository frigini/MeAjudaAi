using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class ChangeServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _repositoryMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly Mock<IServiceCategoryQueries> _categoryQueriesMock;
    private readonly ChangeServiceCategoryCommandHandler _handler;

    public ChangeServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Service, ServiceId>>();
        _serviceQueriesMock = new Mock<IServiceQueries>();
        _categoryQueriesMock = new Mock<IServiceCategoryQueries>();

        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>()).Returns(_repositoryMock.Object);

        _handler = new ChangeServiceCategoryCommandHandler(
            _uowMock.Object,
            _serviceQueriesMock.Object,
            _categoryQueriesMock.Object,
            NullLogger<ChangeServiceCategoryCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var oldCategory = new ServiceCategoryBuilder().AsActive().WithName("Original").Build();
        var newCategory = new ServiceCategoryBuilder().AsActive().WithName("New").Build();
        var service = Service.Create(oldCategory.Id, "Service", "Desc", 1);
        
        var command = new ChangeServiceCategoryCommand(service.Id.Value, newCategory.Id.Value);

        _repositoryMock
            .Setup(x => x.TryFindAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(newCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(service.Name, service.Id, newCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        service.CategoryId.Should().Be(newCategory.Id);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
