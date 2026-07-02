using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Locations.Tests.Integration.Infrastructure;

public abstract class LocationsIntegrationTestBase : BaseIntegrationTest
{
    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"locations_test_{GetType().Name}",
                Username = "test_user",
                Password = "test_password",
                Schema = "locations"
            },
            Cache = new TestCacheOptions { Enabled = false },
            ExternalServices = new TestExternalServicesOptions
            {
                UseMessageBusMock = true
            }
        };
    }

    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddLocationsTestInfrastructure(options);
    }
}
