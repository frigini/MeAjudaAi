using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Helpers;

public static class UnitOfWorkRegistrationHelper
{
    public static void RemoveAll(IServiceCollection services)
    {
        var uowDescriptors = services.Where(d => d.ServiceType == typeof(IUnitOfWork) && !d.IsKeyedService).ToList();
        foreach (var descriptor in uowDescriptors)
            services.Remove(descriptor);
    }
}
