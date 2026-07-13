namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Collections;

/// <summary>
/// Collection definitions consolidadas para todos os módulos.
/// DisableParallelization impede que classes compartilhando o mesmo DB executem em paralelo.
/// </summary>

[CollectionDefinition("UsersIntegrationTests", DisableParallelization = true)]
public class UsersIntegrationTestCollection { }

[CollectionDefinition("ProvidersIntegrationTests", DisableParallelization = true)]
public class ProvidersIntegrationTestCollection { }

[CollectionDefinition("BookingsIntegrationTests", DisableParallelization = true)]
public class BookingsIntegrationTestCollection { }

[CollectionDefinition("PaymentsIntegrationTests", DisableParallelization = true)]
public class PaymentsIntegrationTestCollection { }

[CollectionDefinition("CommunicationsIntegrationTests", DisableParallelization = true)]
public class CommunicationsIntegrationTestCollection { }

[CollectionDefinition("DocumentsIntegrationTests", DisableParallelization = true)]
public class DocumentsIntegrationTestCollection { }

[CollectionDefinition("LocationsIntegrationTests", DisableParallelization = true)]
public class LocationsIntegrationTestCollection { }

[CollectionDefinition("RatingsIntegrationTests", DisableParallelization = true)]
public class RatingsIntegrationTestCollection { }

[CollectionDefinition("SearchProvidersIntegrationTests", DisableParallelization = true)]
public class SearchProvidersIntegrationTestCollection { }

[CollectionDefinition("ServiceCatalogsIntegrationTests", DisableParallelization = true)]
public class ServiceCatalogsIntegrationTestCollection { }

[CollectionDefinition("EnvironmentVariableTests", DisableParallelization = true)]
public class EnvironmentVariableTestsCollection { }
