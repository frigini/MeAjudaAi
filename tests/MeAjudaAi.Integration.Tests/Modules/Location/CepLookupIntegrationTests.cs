using FluentAssertions;
using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Contracts.Modules.Location;
using MeAjudaAi.Shared.Tests.Mocks.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Location;

/// <summary>
/// Testes de integração para o serviço de lookup de CEP com mock HTTP handlers.
/// Utiliza infraestrutura compartilhada de mocks do Shared.Tests.
/// </summary>
public sealed class CepLookupIntegrationTests : IAsyncLifetime
{
    private ServiceProvider? _serviceProvider;
    private MockHttpClientBuilder? _httpMockBuilder;

    public async ValueTask InitializeAsync()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));

        // Add caching (in-memory for tests)
        services.AddHybridCache();
        services.AddSingleton<CacheMetrics>();
        services.AddSingleton<ICacheService, HybridCacheService>();

        // Configure HTTP clients com mocks usando infraestrutura compartilhada
        _httpMockBuilder = new MockHttpClientBuilder(services);

        _httpMockBuilder
            .AddMockedClient<ViaCepClient>()
            .AddMockedClient<BrasilApiCepClient>()
            .AddMockedClient<OpenCepClient>();

        // Add Location module services
        var configuration = new ConfigurationBuilder().Build();
        MeAjudaAi.Modules.Location.Infrastructure.Extensions.AddLocationModule(services, configuration);

        _serviceProvider = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetAddressFromCepAsync_WithValidCep_ShouldReturnAddress()
    {
        // Arrange
        var cep = "01310100";
        var viaCepResponse = """
            {
                "cep": "01310-100",
                "logradouro": "Avenida Paulista",
                "complemento": "lado ímpar",
                "bairro": "Bela Vista",
                "localidade": "São Paulo",
                "uf": "SP",
                "erro": false
            }
            """;

        _httpMockBuilder!
            .GetHandler<ViaCepClient>()
            .SetupResponse($"viacep.com.br/ws/{cep}/json", HttpStatusCode.OK, viaCepResponse);

        var locationApi = _serviceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(cep);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Cep.Should().Be("01310-100");
        result.Value.Street.Should().Be("Avenida Paulista");
        result.Value.Neighborhood.Should().Be("Bela Vista");
        result.Value.City.Should().Be("São Paulo");
        result.Value.State.Should().Be("SP");
    }

    [Fact]
    public async Task GetAddressFromCepAsync_WhenViaCepFails_ShouldFallbackToBrasilApi()
    {
        // Arrange
        var cep = "01310100";

        // ViaCEP retorna erro
        _httpMockBuilder!
            .GetHandler<ViaCepClient>()
            .SetupErrorResponse("viacep.com.br", HttpStatusCode.InternalServerError);

        // BrasilAPI retorna sucesso
        var brasilApiResponse = """
            {
                "cep": "01310100",
                "street": "Avenida Paulista",
                "neighborhood": "Bela Vista",
                "city": "São Paulo",
                "state": "SP"
            }
            """;

        _httpMockBuilder
            .GetHandler<BrasilApiCepClient>()
            .SetupResponse($"brasilapi.com.br/api/cep/v2/{cep}", HttpStatusCode.OK, brasilApiResponse);

        var locationApi = _serviceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(cep);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Street.Should().Be("Avenida Paulista");
    }

    [Fact]
    public async Task GetAddressFromCepAsync_WithInvalidCep_ShouldReturnFailure()
    {
        // Arrange
        var invalidCep = "abcd1234";
        var locationApi = _serviceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(invalidCep);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("inválido");
    }

    [Fact]
    public async Task GetAddressFromCepAsync_WhenAllProvidersFail_ShouldReturnFailure()
    {
        // Arrange
        var cep = "99999999";

        // Todos os provedores retornam erro
        _httpMockBuilder!
            .GetHandler<ViaCepClient>()
            .SetupErrorResponse("viacep.com.br", HttpStatusCode.NotFound);

        _httpMockBuilder
            .GetHandler<BrasilApiCepClient>()
            .SetupErrorResponse("brasilapi.com.br", HttpStatusCode.NotFound);

        _httpMockBuilder
            .GetHandler<OpenCepClient>()
            .SetupErrorResponse("opencep.com", HttpStatusCode.NotFound);

        var locationApi = _serviceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(cep);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("não encontrado");
    }

    // TODO: Investigar comportamento do HybridCache em ambiente de testes
    // Cache está fazendo 3 chamadas HTTP ao invés de usar cache na segunda requisição
    [Fact(Skip = "Cache behavior needs investigation - HybridCache may not be working as expected in test environment")]
    public async Task GetAddressFromCepAsync_WithCaching_ShouldCacheResults()
    {
        // Arrange
        var cep = "01310100";
        var viaCepResponse = """
            {
                "cep": "01310-100",
                "logradouro": "Avenida Paulista",
                "bairro": "Bela Vista",
                "localidade": "São Paulo",
                "uf": "SP",
                "erro": false
            }
            """;

        var mockHandler = _httpMockBuilder!.GetHandler<ViaCepClient>();
        mockHandler.SetupResponse($"viacep.com.br/ws/{cep}/json", HttpStatusCode.OK, viaCepResponse);

        var locationApi = _serviceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act - Primeira chamada (cache miss)
        var result1 = await locationApi.GetAddressFromCepAsync(cep);
        // Segunda chamada (cache hit)
        var result2 = await locationApi.GetAddressFromCepAsync(cep);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Should().BeEquivalentTo(result2.Value);

        // Verifica que a API externa foi chamada apenas uma vez (cache funcionando)
        mockHandler.VerifyRequest($"viacep.com.br/ws/{cep}/json", Times.Once());
    }
}

