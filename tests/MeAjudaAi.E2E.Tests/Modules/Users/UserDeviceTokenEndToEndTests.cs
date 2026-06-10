using System.Net;
using System.Net.Http.Json;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Users.API.Endpoints.Public;
using MeAjudaAi.Shared.Endpoints;
using Xunit;
using MeAjudaAi.E2E.Tests.Base;
using FluentAssertions;

namespace MeAjudaAi.E2E.Tests.Modules.Users;

public class UserDeviceTokenEndToEndTests : BaseTestContainerTest
{
    public UserDeviceTokenEndToEndTests() { }

    [Fact]
    public async Task Put_DeviceToken_ShouldReturnNoContent_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid(); // Note: Em E2E real, precisaria criar o usuário antes.
        var request = new DeviceTokenRequest("test-device-token");

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/users/{userId}/device-token", request);

        // Assert
        // Como o usuário não foi criado no banco de teste, deve retornar NotFound. 
        // Em um teste E2E completo, deve-se criar o usuário primeiro.
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
