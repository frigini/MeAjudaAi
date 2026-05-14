using FluentAssertions;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Extensions;

public class SharedExtensionsTests
{
    [Fact]
    public void AddSharedServices_ShouldRegisterRoutingUnitOfWork()
    {
        var sc = new ServiceCollection();
        sc.AddLogging();
        // Mock configuração mínima necessária
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        
        // Chamando o método de extensão do projeto
        sc.AddSharedServices(config); 
        
        using var sp = sc.BuildServiceProvider();

        var uow = sp.GetRequiredService<IUnitOfWork>();
        uow.Should().BeOfType<RoutingUnitOfWork>();
    }
}
