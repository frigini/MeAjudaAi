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
    private readonly Mock<ILogger<ServiceCatalogsModuleApi>> _loggerMock;
    private readonly ServiceCatalogsModuleApi _sut;

    public ServiceCatalogsModuleApiTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ServiceCatalogsModuleApi>>();

        _sut = new ServiceCatalogsModuleApi(_uowMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void ModuleName_ShouldReturn_ServiceCatalogs()
    {
        var result = _sut.ModuleName;
        result.Should().Be("ServiceCatalogs");
    }

    [Fact]
    public void ApiVersion_ShouldReturn_1_0()
    {
        var result = _sut.ApiVersion;
        result.Should().Be("1.0");
    }
}