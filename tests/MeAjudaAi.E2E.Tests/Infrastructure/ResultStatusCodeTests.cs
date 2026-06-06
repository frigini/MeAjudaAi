using System.Net.Http.Json;
using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

public class ResultStatusCodeTests : BaseTestContainerTest
{
    [Fact]
    public async Task ResultFailure_ShouldReturn400()
    {
        AuthenticateAsAdmin();
        // Um comando que sabemos que falha e retorna Result.Failure (ex: nome vazio)
        var categoryRequest = new { Name = "", Description = "Desc", DisplayOrder = 1 };
        var response = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryRequest);
        
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"isSuccess\":false");
    }
}
