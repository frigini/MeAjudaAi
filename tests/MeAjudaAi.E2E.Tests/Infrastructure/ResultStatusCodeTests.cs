using MeAjudaAi.E2E.Tests.Base;
using System.Net.Http.Json;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

public class ResultStatusCodeTests(TestContainerFixture fixture) : BaseE2ETest<TestContainerFixture>(fixture)
{
    [Fact]
    public async Task ResultFailure_ShouldReturn400()
    {
        TestContainerFixture.AuthenticateAsAdmin();
        var categoryRequest = new { Name = "", Description = "Desc", DisplayOrder = 1 };
        var response = await Fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryRequest);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"isSuccess\":false");
    }
}
