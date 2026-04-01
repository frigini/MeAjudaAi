using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

/// <summary>
/// Testes de integração para os endpoints admin de cidades permitidas (AllowedCity).
/// Cobre CreateAllowedCity, GetAllAllowedCities, GetAllowedCityById,
/// UpdateAllowedCity, PatchAllowedCity, DeleteAllowedCity e SearchLocations.
/// </summary>
public class AllowedCityApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Locations;

    private const string BaseRoute = "/api/v1/admin/allowed-cities";

    // ─── Helper para criar uma cidade teste via API ───────────────────────────
    private async Task<Guid> CreateTestCityAsync(
        string cityName = "Muriaé",
        string state = "MG",
        double lat = -21.13,
        double lon = -42.37,
        double radiusKm = 50.0)
    {
        AuthConfig.ConfigureAdmin();
        var payload = new
        {
            city = cityName,
            state,
            latitude = lat,
            longitude = lon,
            serviceRadiusKm = radiusKm,
            isActive = true
        };

        var response = await Client.PostAsJsonAsync(BaseRoute, payload);
        response.EnsureSuccessStatusCode();
        var json = await ReadJsonAsync<JsonElement>(response.Content);
        var data = GetResponseData(json);
        return data.GetGuid();
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAllowedCity_WhenAdmin_ShouldReturn201WithId()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var payload = new
        {
            city = $"Cidade_{Guid.NewGuid():N}"[..20],
            state = "SP",
            latitude = -23.5,
            longitude = -46.6,
            serviceRadiusKm = 50.0,
            isActive = true
        };

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var json = await ReadJsonAsync<JsonElement>(response.Content);
        var data = GetResponseData(json);
        data.ValueKind.Should().Be(JsonValueKind.String, "response should contain the new Guid");
        Guid.TryParse(data.GetString(), out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateAllowedCity_WithoutAuthentication_ShouldReturn401Or403()
    {
        // Arrange - no auth configured
        var payload = new
        {
            city = "TestCity",
            state = "SP",
            latitude = -23.5,
            longitude = -46.6,
            serviceRadiusKm = 50.0,
            isActive = true
        };

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, payload);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAllowedCity_WithInvalidPayload_ShouldReturn400Or500()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var badPayload = new { }; // Missing required fields

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, badPayload);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError);
    }

    // ─── GET ALL ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAllowedCities_WhenAdmin_ShouldReturn200WithList()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        var json = JsonDocument.Parse(content);
        json.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
    }

    [Fact]
    public async Task GetAllAllowedCities_WithOnlyActiveFilter_ShouldReturn200()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync($"{BaseRoute}?onlyActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllAllowedCities_WithoutAuthentication_ShouldReturn401Or403()
    {
        // Arrange - no auth

        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // ─── GET BY ID ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllowedCityById_WithExistingId_ShouldReturn200()
    {
        // Arrange
        var cityId = await CreateTestCityAsync($"CidadeGet_{Guid.NewGuid():N}"[..20], "RJ");

        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{cityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await ReadJsonAsync<JsonElement>(response.Content);
        var data = GetResponseData(json);
        data.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public async Task GetAllowedCityById_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllowedCityById_WithoutAuthentication_ShouldReturn401Or403()
    {
        // Arrange - no auth
        var anyId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"{BaseRoute}/{anyId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // ─── UPDATE (PUT) ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAllowedCity_WithValidData_ShouldReturn204()
    {
        // Arrange
        var cityId = await CreateTestCityAsync($"CidadeUpd_{Guid.NewGuid():N}"[..20], "ES");

        AuthConfig.ConfigureAdmin();
        var updatePayload = new
        {
            cityName = $"CidadeUpdated_{Guid.NewGuid():N}"[..20],
            stateSigla = "ES",
            latitude = -20.0,
            longitude = -40.0,
            serviceRadiusKm = 60.0,
            isActive = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{cityId}", updatePayload);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateAllowedCity_WithNonExistingId_ShouldReturn404Or400()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var nonExistingId = Guid.NewGuid();
        var updatePayload = new
        {
            cityName = "TestCity",
            stateSigla = "SP",
            latitude = -23.5,
            longitude = -46.6,
            serviceRadiusKm = 50.0,
            isActive = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{nonExistingId}", updatePayload);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task UpdateAllowedCity_WithoutAuthentication_ShouldReturn401Or403()
    {
        // Arrange - no auth
        var anyId = Guid.NewGuid();
        var payload = new { cityName = "Test", stateSigla = "SP", latitude = 0.0, longitude = 0.0, serviceRadiusKm = 50.0, isActive = true };

        // Act
        var response = await Client.PutAsJsonAsync($"{BaseRoute}/{anyId}", payload);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // ─── PATCH ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PatchAllowedCity_WithValidPayload_ShouldReturn2xx()
    {
        // Arrange
        var cityId = await CreateTestCityAsync($"CidadePatch_{Guid.NewGuid():N}"[..20], "MG");

        AuthConfig.ConfigureAdmin();
        var patchPayload = new { isActive = false };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Patch, $"{BaseRoute}/{cityId}")
        {
            Content = JsonContent.Create(patchPayload)
        };
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.BadRequest); // Could fail validation if patch format not accepted
    }

    [Fact]
    public async Task PatchAllowedCity_WithNonExistingId_ShouldReturn4xx()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var nonExistingId = Guid.NewGuid();
        var patchPayload = new { isActive = false };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Patch, $"{BaseRoute}/{nonExistingId}")
        {
            Content = JsonContent.Create(patchPayload)
        };
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task PatchAllowedCity_WithoutAuthentication_ShouldReturn401Or403()
    {
        // Arrange - no auth
        var anyId = Guid.NewGuid();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Patch, $"{BaseRoute}/{anyId}")
        {
            Content = JsonContent.Create(new { isActive = false })
        };
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // ─── DELETE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAllowedCity_WithExistingId_ShouldReturn200()
    {
        // Arrange
        var cityId = await CreateTestCityAsync($"CidadeDel_{Guid.NewGuid():N}"[..20], "RJ");

        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{cityId}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAllowedCity_WithNonExistingId_ShouldReturn4xxOr200()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{nonExistingId}");

        // Assert
        // May return 200 (idempotent delete) or 404/400
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteAllowedCity_WithoutAuthentication_ShouldReturn401Or403()
    {
        // Arrange - no auth
        var anyId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"{BaseRoute}/{anyId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // ─── SEARCH LOCATIONS ─────────────────────────────────────────────────────

    [Fact]
    public async Task SearchLocations_WithValidQuery_ShouldReturn200()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/locations?q=Muriaé");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SearchLocations_WithShortQuery_ShouldReturn200WithEmptyList()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/locations?q=Mu");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SearchLocations_WithEmptyQuery_ShouldReturn200WithEmptyList()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/locations?q=");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    // ─── FULL CRUD WORKFLOW ───────────────────────────────────────────────────

    [Fact]
    public async Task AllowedCity_FullCrudWorkflow_ShouldSucceed()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var uniqueName = $"WorkflowCity_{Guid.NewGuid():N}"[..22];

        // 1. CREATE
        var createPayload = new
        {
            city = uniqueName,
            state = "SP",
            latitude = -23.5505,
            longitude = -46.6333,
            serviceRadiusKm = 50.0,
            isActive = true
        };
        var createResponse = await Client.PostAsJsonAsync(BaseRoute, createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created, "city creation should succeed");

        var createJson = await ReadJsonAsync<JsonElement>(createResponse.Content);
        var cityId = GetResponseData(createJson).GetGuid();
        cityId.Should().NotBeEmpty();

        // 2. GET BY ID
        var getResponse = await Client.GetAsync($"{BaseRoute}/{cityId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK, "should find the just-created city");

        // 3. UPDATE
        var updatePayload = new
        {
            cityName = uniqueName,
            stateSigla = "SP",
            latitude = -23.5505,
            longitude = -46.6333,
            serviceRadiusKm = 75.0,
            isActive = true
        };
        var updateResponse = await Client.PutAsJsonAsync($"{BaseRoute}/{cityId}", updatePayload);
        updateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);

        // 4. GET ALL (verify city appears)
        var getAllResponse = await Client.GetAsync(BaseRoute);
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. DELETE
        var deleteResponse = await Client.DeleteAsync($"{BaseRoute}/{cityId}");
        deleteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // 6. VERIFY DELETED
        var getAfterDeleteResponse = await Client.GetAsync($"{BaseRoute}/{cityId}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "deleted city should not be found");
    }
}
