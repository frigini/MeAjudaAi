// Enable parallel execution by isolating databases per test class
// [assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Módulos disponíveis para testes de integração
/// </summary>
[Flags]
public enum TestModule
{
    None = 0,
    Users = 1 << 0,
    Providers = 1 << 1,
    Documents = 1 << 2,
    ServiceCatalogs = 1 << 3,
    Locations = 1 << 4,
    SearchProviders = 1 << 5,
    Communications = 1 << 6,
    Payments = 1 << 7,
    Bookings = 1 << 8,
    Ratings = 1 << 9,
    All = Users | Providers | Documents | ServiceCatalogs | Locations | SearchProviders | Communications | Payments | Bookings | Ratings
    }


