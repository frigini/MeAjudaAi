using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands.Service;

public class GetAllServicesQueryHandlerTests
{
    private readonly Mock<IServiceQueries> _queriesMock = new();
    private readonly GetAllServicesQueryHandler _handler;

    public GetAllServicesQueryHandlerTests()
    {
        _handler = new GetAllServicesQueryHandler(_queriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenException_ShouldReturnFailure()
    {
        _queriesMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await _handler.HandleAsync(new GetAllServicesQuery(false, null), default);

        result.IsFailure.Should().BeTrue();
    }
}
