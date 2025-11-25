using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients.Interfaces;
using MeAjudaAi.Modules.Locations.Infrastructure.Services;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Unit tests para IbgeService com mock de IIbgeClient e ICacheService.
/// Testa validação de cidades, cache behavior e error handling.
/// </summary>
public sealed class IbgeServiceTests
{
    private readonly Mock<IIbgeClient> _ibgeClientMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<IbgeService>> _loggerMock;
    private readonly IbgeService _sut;

    public IbgeServiceTests()
    {
        _ibgeClientMock = new Mock<IIbgeClient>(MockBehavior.Strict);
        _cacheServiceMock = new Mock<ICacheService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<IbgeService>>();
        _sut = new IbgeService(_ibgeClientMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
    }

    #region ValidateCityInAllowedRegionsAsync Tests

    [Fact]
    public async Task ValidateCityInAllowedRegionsAsync_CityInAllowedList_ReturnsTrue()
    {
        // Arrange
        const string cityName = "Muriaé";
        const string stateSigla = "MG";
        var allowedCities = new[] { "Muriaé", "Itaperuna", "Linhares" };

        var municipio = CreateMunicipio(3129707, "Muriaé", "MG");

        SetupCacheGetOrCreate(cityName, municipio);

        // Act
        var result = await _sut.ValidateCityInAllowedRegionsAsync(cityName, stateSigla, allowedCities);

        // Assert
        result.Should().BeTrue();
        _cacheServiceMock.Verify();
    }

    [Fact]
    public async Task ValidateCityInAllowedRegionsAsync_CityNotInAllowedList_ReturnsFalse()
    {
        // Arrange
        const string cityName = "São Paulo";
        const string stateSigla = "SP";
        var allowedCities = new[] { "Muriaé", "Itaperuna", "Linhares" };

        var municipio = CreateMunicipio(3550308, "São Paulo", "SP");

        SetupCacheGetOrCreate(cityName, municipio);

        // Act
        var result = await _sut.ValidateCityInAllowedRegionsAsync(cityName, stateSigla, allowedCities);

        // Assert
        result.Should().BeFalse();
        _cacheServiceMock.Verify();
    }

    [Fact]
    public async Task ValidateCityInAllowedRegionsAsync_CaseInsensitiveMatching_ReturnsTrue()
    {
        // Arrange
        const string cityName = "muriaé"; // lowercase
        const string stateSigla = "mg"; // lowercase
        var allowedCities = new[] { "MURIAÉ", "ITAPERUNA" }; // uppercase

        var municipio = CreateMunicipio(3129707, "Muriaé", "MG"); // title case

        SetupCacheGetOrCreate(cityName, municipio);

        // Act
        var result = await _sut.ValidateCityInAllowedRegionsAsync(cityName, stateSigla, allowedCities);

        // Assert
        result.Should().BeTrue();
        _cacheServiceMock.Verify();
    }

    [Fact]
    public async Task ValidateCityInAllowedRegionsAsync_MunicipioNotFound_ReturnsFalse()
    {
        // Arrange
        const string cityName = "CidadeInexistente";
        const string stateSigla = "XX";
        var allowedCities = new[] { "Muriaé" };

        SetupCacheGetOrCreate(cityName, null); // Município não existe

        // Act
        var result = await _sut.ValidateCityInAllowedRegionsAsync(cityName, stateSigla, allowedCities);

        // Assert
        result.Should().BeFalse();
        _cacheServiceMock.Verify();
    }

    [Fact]
    public async Task ValidateCityInAllowedRegionsAsync_StateDoesNotMatch_ReturnsFalse()
    {
        // Arrange
        const string cityName = "Muriaé";
        const string stateSigla = "RJ"; // Errado: Muriaé é MG
        var allowedCities = new[] { "Muriaé" };

        var municipio = CreateMunicipio(3129707, "Muriaé", "MG");

        SetupCacheGetOrCreate(cityName, municipio);

        // Act
        var result = await _sut.ValidateCityInAllowedRegionsAsync(cityName, stateSigla, allowedCities);

        // Assert
        result.Should().BeFalse();
        _cacheServiceMock.Verify();
    }

    [Fact]
    public async Task ValidateCityInAllowedRegionsAsync_NoStateProvided_ValidatesOnlyCity()
    {
        // Arrange
        const string cityName = "Muriaé";
        string? stateSigla = null; // Sem validação de estado
        var allowedCities = new[] { "Muriaé" };

        var municipio = CreateMunicipio(3129707, "Muriaé", "MG");

        SetupCacheGetOrCreate(cityName, municipio);

        // Act
        var result = await _sut.ValidateCityInAllowedRegionsAsync(cityName, stateSigla, allowedCities);

        // Assert
        result.Should().BeTrue();
        _cacheServiceMock.Verify();
    }

    [Fact]
    public async Task ValidateCityInAllowedRegionsAsync_IbgeClientThrowsException_ReturnsFalse()
    {
        // Arrange
        const string cityName = "Muriaé";
        const string stateSigla = "MG";
        var allowedCities = new[] { "Muriaé" };

        SetupCacheGetOrCreateWithException(cityName, new HttpRequestException("IBGE API unreachable"));

        // Act
        var result = await _sut.ValidateCityInAllowedRegionsAsync(cityName, stateSigla, allowedCities);

        // Assert
        result.Should().BeFalse(); // Fail-closed
        _cacheServiceMock.Verify();
    }

    #endregion

    #region GetCityDetailsAsync Tests

    [Fact]
    public async Task GetCityDetailsAsync_CacheHit_ReturnsFromCache()
    {
        // Arrange
        const string cityName = "Muriaé";
        var expectedMunicipio = CreateMunicipio(3129707, "Muriaé", "MG");

        SetupCacheHit(cityName, expectedMunicipio);

        // Act
        var result = await _sut.GetCityDetailsAsync(cityName);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(3129707);
        result.Nome.Should().Be("Muriaé");
        result.GetEstadoSigla().Should().Be("MG");
        _cacheServiceMock.Verify();

        // Verify that the IBGE client was NOT called (cache hit)
        _ibgeClientMock.Verify(
            x => x.GetMunicipioByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCityDetailsAsync_CacheMiss_CallsIbgeClient()
    {
        // Arrange
        const string cityName = "Itaperuna";
        var expectedMunicipio = CreateMunicipio(3302270, "Itaperuna", "RJ");

        SetupCacheGetOrCreate(cityName, expectedMunicipio);

        // Act
        var result = await _sut.GetCityDetailsAsync(cityName);

        // Assert
        result.Should().NotBeNull();
        result!.Nome.Should().Be("Itaperuna");
        result.GetEstadoSigla().Should().Be("RJ");
        _cacheServiceMock.Verify();
    }

    [Fact]
    public async Task GetCityDetailsAsync_MunicipioNotFound_ReturnsNull()
    {
        // Arrange
        const string cityName = "CidadeInexistente";

        SetupCacheGetOrCreate(cityName, null);

        // Act
        var result = await _sut.GetCityDetailsAsync(cityName);

        // Assert
        result.Should().BeNull();
        _cacheServiceMock.Verify();
    }

    #endregion

    #region GetMunicipiosByUFAsync Tests

    [Fact]
    public async Task GetMunicipiosByUFAsync_ValidUF_ReturnsMunicipios()
    {
        // Arrange
        const string ufSigla = "MG";
        var expectedMunicipios = new List<Municipio>
        {
            CreateMunicipio(3129707, "Muriaé", "MG"),
            CreateMunicipio(3106200, "Belo Horizonte", "MG")
        };

        SetupCacheGetOrCreateForUF(ufSigla, expectedMunicipios);

        // Act
        var result = await _sut.GetMunicipiosByUFAsync(ufSigla);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Nome == "Muriaé");
        result.Should().Contain(m => m.Nome == "Belo Horizonte");
        _cacheServiceMock.Verify();
    }

    [Fact]
    public async Task GetMunicipiosByUFAsync_InvalidUF_ReturnsEmptyList()
    {
        // Arrange
        const string ufSigla = "XX";

        SetupCacheGetOrCreateForUF(ufSigla, null);

        // Act
        var result = await _sut.GetMunicipiosByUFAsync(ufSigla);

        // Assert
        result.Should().BeEmpty();
        _cacheServiceMock.Verify();
    }

    [Fact]
    public async Task GetMunicipiosByUFAsync_IbgeClientThrowsException_ThrowsException()
    {
        // Arrange
        const string ufSigla = "MG";

        SetupCacheGetOrCreateForUFWithException(ufSigla, new HttpRequestException("IBGE API down"));

        // Act
        var act = async () => await _sut.GetMunicipiosByUFAsync(ufSigla);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("IBGE API down");
        _cacheServiceMock.Verify();
    }

    #endregion

    #region Helper Methods

    private static Municipio CreateMunicipio(int id, string nome, string ufSigla)
    {
        return new Municipio
        {
            Id = id,
            Nome = nome,
            Microrregiao = new Microrregiao
            {
                Id = 31056,
                Nome = $"Microrregiao de {nome}",
                Mesorregiao = new Mesorregiao
                {
                    Id = 3112,
                    Nome = $"Mesorregiao de {nome}",
                    UF = new UF
                    {
                        Id = ufSigla switch
                        {
                            "MG" => 31,
                            "RJ" => 33,
                            "ES" => 32,
                            "SP" => 35,
                            _ => throw new ArgumentException($"UF não suportada: {ufSigla}")
                        },
                        Sigla = ufSigla,
                        Nome = ufSigla switch
                        {
                            "MG" => "Minas Gerais",
                            "RJ" => "Rio de Janeiro",
                            "ES" => "Espírito Santo",
                            "SP" => "São Paulo",
                            _ => ufSigla
                        },
                        Regiao = new Regiao
                        {
                            Id = 3,
                            Sigla = "SE",
                            Nome = "Sudeste"
                        }
                    }
                }
            }
        };
    }

    private void SetupCacheHit(string cityName, Municipio? municipio)
    {
        _cacheServiceMock
            .Setup(x => x.GetOrCreateAsync<Municipio?>(
                $"ibge:municipio:{cityName.ToLowerInvariant()}",
                It.IsAny<Func<CancellationToken, ValueTask<Municipio?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(municipio)
            .Verifiable();
    }

    private void SetupCacheGetOrCreate(string cityName, Municipio? municipio)
    {
        _cacheServiceMock
            .Setup(x => x.GetOrCreateAsync<Municipio?>(
                $"ibge:municipio:{cityName.ToLowerInvariant()}",
                It.IsAny<Func<CancellationToken, ValueTask<Municipio?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (string key, Func<CancellationToken, ValueTask<Municipio?>> factory, TimeSpan? expiration, HybridCacheEntryOptions? options, IReadOnlyCollection<string>? tags, CancellationToken ct) =>
            {
                // Simular cache miss: chamar factory
                return await factory(ct);
            })
            .Verifiable();

        if (municipio is not null)
        {
            _ibgeClientMock
                .Setup(x => x.GetMunicipioByNameAsync(cityName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(municipio)
                .Verifiable();
        }
        else
        {
            _ibgeClientMock
                .Setup(x => x.GetMunicipioByNameAsync(cityName, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Municipio?)null)
                .Verifiable();
        }
    }

    private void SetupCacheGetOrCreateWithException(string cityName, Exception exception)
    {
        _cacheServiceMock
            .Setup(x => x.GetOrCreateAsync<Municipio?>(
                $"ibge:municipio:{cityName.ToLowerInvariant()}",
                It.IsAny<Func<CancellationToken, ValueTask<Municipio?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception)
            .Verifiable();
    }

    private void SetupCacheGetOrCreateForUF(string ufSigla, List<Municipio>? municipios)
    {
        _cacheServiceMock
            .Setup(x => x.GetOrCreateAsync<List<Municipio>?>(
                $"ibge:uf:{ufSigla.ToUpperInvariant()}",
                It.IsAny<Func<CancellationToken, ValueTask<List<Municipio>?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (string key, Func<CancellationToken, ValueTask<List<Municipio>?>> factory, TimeSpan? expiration, HybridCacheEntryOptions? options, IReadOnlyCollection<string>? tags, CancellationToken ct) =>
            {
                // Simular cache miss: chamar factory
                return await factory(ct);
            })
            .Verifiable();

        if (municipios is not null)
        {
            _ibgeClientMock
                .Setup(x => x.GetMunicipiosByUFAsync(ufSigla, It.IsAny<CancellationToken>()))
                .ReturnsAsync(municipios)
                .Verifiable();
        }
        else
        {
            _ibgeClientMock
                .Setup(x => x.GetMunicipiosByUFAsync(ufSigla, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Municipio>())
                .Verifiable();
        }
    }

    private void SetupCacheGetOrCreateForUFWithException(string ufSigla, Exception exception)
    {
        _cacheServiceMock
            .Setup(x => x.GetOrCreateAsync<List<Municipio>?>(
                $"ibge:uf:{ufSigla.ToUpperInvariant()}",
                It.IsAny<Func<CancellationToken, ValueTask<List<Municipio>?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception)
            .Verifiable();
    }

    #endregion
}
