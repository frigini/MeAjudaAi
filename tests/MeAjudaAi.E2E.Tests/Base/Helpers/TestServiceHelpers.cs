using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.E2E.Tests.Base.Helpers;

public static class TestServiceHelpers
{
    public static void RemoveAllUnitOfWorkRegistrations(IServiceCollection services)
    {
        var uowDescriptors = services.Where(d => d.ServiceType == typeof(IUnitOfWork) && !d.IsKeyedService).ToList();
        foreach (var descriptor in uowDescriptors)
            services.Remove(descriptor);
    }
}
