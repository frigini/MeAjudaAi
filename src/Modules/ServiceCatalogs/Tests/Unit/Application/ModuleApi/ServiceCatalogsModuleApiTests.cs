using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.ModuleApi;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class ServiceCatalogsModuleApiTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _serviceRepoMock;
    private readonly Mock<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.IServiceCategoryQueries> _categoryQueriesMock;
    private readonly Mock<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.IServiceQueries> _serviceQueriesMock;
    private readonly Mock<ILogger<ServiceCatalogsModuleApi>> _loggerMock;
    private readonly ServiceCatalogsModuleApi _sut;

    public ServiceCatalogsModuleApiTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _serviceRepoMock = new Mock<IRepository<Service, ServiceId>>();
        _categoryQueriesMock = new Mock<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.IServiceCategoryQueries>();
        _serviceQueriesMock = new Mock<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.IServiceQueries>();
        _loggerMock = new Mock<ILogger<ServiceCatalogsModuleApi>>();

        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>()).Returns(_serviceRepoMock.Object);

        _sut = new ServiceCatalogsModuleApi(
            _uowMock.Object, 
            _categoryQueriesMock.Object, 
            _serviceQueriesMock.Object, 
            _loggerMock.Object);
    }

    [Fact]
    public void ModuleName_ShouldReturn_ServiceCatalogs()
    {
        var result = _sut.ModuleName;
        result.Should().Be("ServiceCatalogs");
    }

    [Fact]
    public async Task GetServiceById_ShouldReturnService()
    {
        var category = new ServiceCategoryBuilder().Build();
        var service = new ServiceBuilder().WithCategoryId(category.Id).Build();
        
        var categoryRepoMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        
        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>()).Returns(_serviceRepoMock.Object);
        _uowMock.Setup(x => x.GetRepository<ServiceCategory, ServiceCategoryId>()).Returns(categoryRepoMock.Object);
        
        _serviceRepoMock.Setup(x => x.TryFindAsync(service.Id, default))
            .ReturnsAsync(service);
        categoryRepoMock.Setup(x => x.TryFindAsync(service.CategoryId, default))
            .ReturnsAsync(category);

        var result = await _sut.GetServiceByIdAsync(service.Id.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(service.Id.Value);
    }

    [Fact]
    public async Task IsServiceActive_ShouldReturnStatus()
    {
        var serviceId = Guid.NewGuid();
        var service = new ServiceBuilder().AsActive().Build();
        _serviceRepoMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), default))
            .ReturnsAsync(service);

        var result = await _sut.IsServiceActiveAsync(serviceId);

        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateServices_ShouldReturnResults()
    {
        var ids = new List<Guid> { Guid.NewGuid() };
        _serviceRepoMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), default))
            .ReturnsAsync(new ServiceBuilder().Build());

        var result = await _sut.ValidateServicesAsync(ids);

        result.Value.AllValid.Should().BeTrue();
    }

    [Fact]
    public async Task CheckHealth_WhenQueryFails_ShouldReturnUnhealthy()
    {
        _categoryQueriesMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _sut.CheckHealthAsync();

        result.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);
    }
}
