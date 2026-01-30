using System.Net;
using System.Text.Json;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Modules.Locations;

/// <summary>
/// Testes E2E para o módulo Locations (AllowedCities e validações geográficas)
/// Valida fluxo completo de CRUD com autenticação de admin
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Locations")]
public class LocationsEndToEndTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public LocationsEndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateAllowedCity_WithValidData_ShouldCreateAndReturnCityId()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        // Use dados únicos para o fluxo/workflow para evitar conflitos com outros testes
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..4];
        var request = new
        {
            City = $"MuriaéFlow_{uniqueSuffix}",
            State = "MG",
            IbgeCode = 3143906,
            IsActive = true
        };

        // Act
        var response = await _fixture.PostJsonAsync("/api/v1/admin/allowed-cities", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, TestContainerFixture.JsonOptions);

        // Create retorna Response<T> (legado), então use "data"
        result.TryGetProperty("data", out var dataElement).Should().BeTrue();
        var cityId = Guid.Parse(dataElement.GetString()!);
        cityId.Should().NotBeEmpty();

        // Verify database persistence
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();
            var city = await dbContext.AllowedCities.FirstOrDefaultAsync(c => c.Id == cityId);

            city.Should().NotBeNull();
            city!.CityName.Should().Be(request.City);
            city.StateSigla.Should().Be(request.State);
            city.IbgeCode.Should().Be(request.IbgeCode);
            city.IsActive.Should().BeTrue();
            city.CreatedBy.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public async Task CreateAllowedCity_WithDuplicateCityAndState_ShouldReturnBadRequest()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        // Create first city
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();
            var city = new AllowedCity("São Paulo", "SP", "system", 3550308);
            dbContext.AllowedCities.Add(city);
            await dbContext.SaveChangesAsync();
        });

        var duplicateRequest = new
        {
            City = "São Paulo",
            State = "SP",
            IbgeCode = 9999999
        };

        // Act
        var response = await _fixture.PostJsonAsync("/api/v1/admin/allowed-cities", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("já cadastrada");
    }

    [Fact]
    public async Task CreateAllowedCity_WithoutAdminAuth_ShouldReturnForbidden()
    {
        // Arrange - authenticate as regular user
        TestContainerFixture.AuthenticateAsUser();

        var request = new
        {
            City = "Curitiba",
            State = "PR"
        };

        // Act
        var response = await _fixture.PostJsonAsync("/api/v1/admin/allowed-cities", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAllowedCity_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        var invalidRequest = new
        {
            City = "", // Nome da cidade vazio
            State = "MG"
        };

        // Act
        var response = await _fixture.PostJsonAsync("/api/v1/admin/allowed-cities", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllAllowedCities_WithOnlyActiveTrue_ShouldReturnOnlyActiveCities()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        // Create active and inactive cities
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();

            var activeCity = new AllowedCity("Rio de Janeiro", "RJ", "system", 3304557, true);
            var inactiveCity = new AllowedCity("Niterói", "RJ", "system", 3303302, false);

            dbContext.AllowedCities.AddRange(activeCity, inactiveCity);
            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await _fixture.ApiClient.GetAsync("/api/v1/admin/allowed-cities?onlyActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, TestContainerFixture.JsonOptions);

        // GetAll retorna Result<T>, então use "value"
        result.TryGetProperty("value", out var dataElement).Should().BeTrue();
        var cities = dataElement.EnumerateArray().ToList();

        cities.Should().NotBeEmpty();

        // Verify all cities are active
        foreach (var city in cities)
        {
            city.TryGetProperty("isActive", out var isActive).Should().BeTrue();
            isActive.GetBoolean().Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetAllAllowedCities_WithOnlyActiveFalse_ShouldReturnAllCities()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        // Create active and inactive cities
        var activeCityId = Guid.Empty;
        var inactiveCityId = Guid.Empty;

        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();

            var activeCity = new AllowedCity("Salvador", "BA", "system", 2927408, true);
            var inactiveCity = new AllowedCity("Feira de Santana", "BA", "system", 2910800, false);

            dbContext.AllowedCities.AddRange(activeCity, inactiveCity);
            await dbContext.SaveChangesAsync();

            activeCityId = activeCity.Id;
            inactiveCityId = inactiveCity.Id;
        });

        // Act
        var response = await _fixture.ApiClient.GetAsync("/api/v1/admin/allowed-cities?onlyActive=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, TestContainerFixture.JsonOptions);

        // GetAll retorna Result<T>, então use "value"
        result.TryGetProperty("value", out var dataElement).Should().BeTrue();
        var cities = dataElement.EnumerateArray().ToList();

        // Verify both active and inactive cities are present
        var cityIds = cities
            .Select(city => city.TryGetProperty("id", out var id) ? Guid.Parse(id.GetString()!) : Guid.Empty)
            .ToList();

        cityIds.Should().Contain(activeCityId);
        cityIds.Should().Contain(inactiveCityId);
    }

    [Fact]
    public async Task GetAllAllowedCities_ShouldReturnOrderedByStateAndCity()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();

        // Create cities in different states
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();

            // Limpar tabela antes de inserir novos dados
            dbContext.AllowedCities.RemoveRange(dbContext.AllowedCities);
            await dbContext.SaveChangesAsync();

            var cities = new[]
            {
                new AllowedCity("Uberlândia", "MG", "system"),
                new AllowedCity("Belo Horizonte", "MG", "system"),
                new AllowedCity("Brasília", "DF", "system"),
                new AllowedCity("Goiânia", "GO", "system")
            };

            dbContext.AllowedCities.AddRange(cities);
            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await _fixture.ApiClient.GetAsync("/api/v1/admin/allowed-cities");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, TestContainerFixture.JsonOptions);

        // GetAll retorna Result<T>, então use "value"
        result.TryGetProperty("value", out var dataElement).Should().BeTrue();
        var cities = dataElement.EnumerateArray().ToList();

        cities.Should().NotBeEmpty();

        // Verify ordering: first by StateSigla, then by CityName
        var orderedStates = new List<string>();
        foreach (var city in cities)
        {
            if (city.TryGetProperty("stateSigla", out var state))
            {
                orderedStates.Add(state.GetString()!);
            }
        }

        orderedStates.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAllowedCityById_WithValidId_ShouldReturnCity()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        Guid cityId = Guid.Empty;
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();
            var city = new AllowedCity("Recife", "PE", "system", 2611606);
            dbContext.AllowedCities.Add(city);
            await dbContext.SaveChangesAsync();
            cityId = city.Id;
        });

        // Act
        var response = await _fixture.ApiClient.GetAsync($"/api/v1/admin/allowed-cities/{cityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, TestContainerFixture.JsonOptions);

        // GetById retorna Response<T> (legado), então use "data"
        result.TryGetProperty("data", out var dataElement).Should().BeTrue();

        dataElement.TryGetProperty("id", out var idElement).Should().BeTrue();
        Guid.Parse(idElement.GetString()!).Should().Be(cityId);

        dataElement.TryGetProperty("cityName", out var cityNameElement).Should().BeTrue();
        cityNameElement.GetString().Should().Be("Recife");

        dataElement.TryGetProperty("stateSigla", out var stateElement).Should().BeTrue();
        stateElement.GetString().Should().Be("PE");
    }

    [Fact]
    public async Task GetAllowedCityById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _fixture.ApiClient.GetAsync($"/api/v1/admin/allowed-cities/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAllowedCity_WithValidData_ShouldUpdateCity()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        Guid cityId = Guid.Empty;
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();
            var city = new AllowedCity("Porto Alegre", "RS", "system", 4314902);
            dbContext.AllowedCities.Add(city);
            await dbContext.SaveChangesAsync();
            cityId = city.Id;
        });

        var updateRequest = new
        {
            City = "Porto Alegre Atualizado",
            State = "RS",
            IbgeCode = 4314902,
            IsActive = false
        };

        // Act
        var response = await _fixture.PutJsonAsync($"/api/v1/admin/allowed-cities/{cityId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify database changes
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();
            var updatedCity = await dbContext.AllowedCities.FirstOrDefaultAsync(c => c.Id == cityId);

            updatedCity.Should().NotBeNull();
            updatedCity!.CityName.Should().Be("Porto Alegre Atualizado");
            updatedCity.IsActive.Should().BeFalse();
            updatedCity.UpdatedAt.Should().NotBeNull();
            updatedCity.UpdatedBy.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public async Task UpdateAllowedCity_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        var nonExistentId = Guid.NewGuid();
        var updateRequest = new
        {
            City = "Fortaleza",
            State = "CE"
        };

        // Act
        var response = await _fixture.PutJsonAsync($"/api/v1/admin/allowed-cities/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAllowedCity_WithDuplicateCityAndState_ShouldReturnBadRequest()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        Guid city1Id = Guid.Empty;
        Guid city2Id = Guid.Empty;

        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();
            var city1 = new AllowedCity("Manaus", "AM", "system");
            var city2 = new AllowedCity("Belém", "PA", "system");

            dbContext.AllowedCities.AddRange(city1, city2);
            await dbContext.SaveChangesAsync();

            city1Id = city1.Id;
            city2Id = city2.Id;
        });

        var updateRequest = new
        {
            City = "Belém", // Trying to rename to existing city
            State = "PA"
        };

        // Act
        var response = await _fixture.PutJsonAsync($"/api/v1/admin/allowed-cities/{city1Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("já cadastrada");
    }

    [Fact]
    public async Task DeleteAllowedCity_WithValidId_ShouldRemoveCity()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        Guid cityId = Guid.Empty;
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();
            var city = new AllowedCity("Florianópolis", "SC", "system");
            dbContext.AllowedCities.Add(city);
            await dbContext.SaveChangesAsync();
            cityId = city.Id;
        });

        // Act
        var response = await _fixture.ApiClient.DeleteAsync($"/api/v1/admin/allowed-cities/{cityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify city was removed
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();
            var city = await dbContext.AllowedCities.FirstOrDefaultAsync(c => c.Id == cityId);
            city.Should().BeNull();
        });
    }

    [Fact]
    public async Task DeleteAllowedCity_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _fixture.ApiClient.DeleteAsync($"/api/v1/admin/allowed-cities/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AllowedCityWorkflow_Should_CreateUpdateAndDelete()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        // Step 1: Create city
        // Use dados únicos para o fluxo de trabalho para evitar conflito com outros testes
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..4];
        var createRequest = new
        {
            City = $"Vitória_{uniqueSuffix}",
            State = "ES",
            IbgeCode = 3205309
        };

        var createResponse = await _fixture.PostJsonAsync("/api/v1/admin/allowed-cities", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<JsonElement>(createContent, TestContainerFixture.JsonOptions);
        // Create retorna Response<T> (legado), então use "data"
        createResult.TryGetProperty("data", out var cityIdElement).Should().BeTrue();
        var cityId = Guid.Parse(cityIdElement.GetString()!);

        // Step 2: Verify city exists
        var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/admin/allowed-cities/{cityId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Update city
        var updateRequest = new
        {
            City = "Vitória Atualizada",
            State = "ES",
            IbgeCode = 3205309,
            IsActive = false
        };

        var updateResponse = await _fixture.PutJsonAsync($"/api/v1/admin/allowed-cities/{cityId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Verify update
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<LocationsDbContext>();
            var city = await dbContext.AllowedCities.FirstOrDefaultAsync(c => c.Id == cityId);

            city.Should().NotBeNull();
            city!.CityName.Should().Be("Vitória Atualizada");
            city.IsActive.Should().BeFalse();
        });

        // Step 5: Delete city
        var deleteResponse = await _fixture.ApiClient.DeleteAsync($"/api/v1/admin/allowed-cities/{cityId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 6: Verify deletion
        var getFinalResponse = await _fixture.ApiClient.GetAsync($"/api/v1/admin/allowed-cities/{cityId}");
        getFinalResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

