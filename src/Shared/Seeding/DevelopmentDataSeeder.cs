using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace MeAjudaAi.Shared.Seeding;

/// <summary>
/// Implementação do seeder de dados de desenvolvimento
/// </summary>
public class DevelopmentDataSeeder : IDevelopmentDataSeeder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

    // IDs estáveis para categorias (para evitar FK failures em re-runs)
    private static readonly Guid HealthCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid EducationCategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SocialCategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid LegalCategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid HousingCategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid FoodCategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    // IDs estáveis para Providers de teste
    private static readonly Guid Provider1UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid Provider2UserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid Provider3UserId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid Provider1Id = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid Provider2Id = Guid.Parse("66666666-7777-8888-9999-000000000000");
    private static readonly Guid Provider3Id = Guid.Parse("77777777-8888-9999-0000-111111111111");

    // IDs estáveis para Documentos dos Providers
    private static readonly Guid Provider1DocumentId = Guid.Parse("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa");
    private static readonly Guid Provider2DocumentId = Guid.Parse("bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb");
    private static readonly Guid Provider3DocumentId = Guid.Parse("cccccccc-3333-3333-3333-cccccccccccc");

    public DevelopmentDataSeeder(
        IServiceProvider serviceProvider,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedIfEmptyAsync(CancellationToken cancellationToken = default)
    {
        var hasData = await HasDataAsync(cancellationToken);

        if (hasData)
        {
            _logger.LogInformation("🔍 Database already has data, skipping seed");
            return;
        }

        _logger.LogInformation("🌱 Empty database detected, starting development data seed...");
        await ExecuteSeedAsync(cancellationToken);
    }

    public async Task ForceSeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("🔄 Running data seed (ensuring minimum data)...");
        await ExecuteSeedAsync(cancellationToken);
    }

    public async Task<bool> HasDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar se ServiceCatalogs tem categorias usando LINQ
            var serviceCatalogsContext = GetDbContext("ServiceCatalogs");
            if (serviceCatalogsContext != null)
            {
                var categoryType = serviceCatalogsContext.Model
                    .GetEntityTypes()
                    .FirstOrDefault(e => e.ClrType.Name == "Category");

                if (categoryType != null)
                {
                    var dbSet = serviceCatalogsContext.GetType()
                        .GetProperties()
                        .FirstOrDefault(p => p.PropertyType.IsGenericType &&
                                           p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                                           p.PropertyType.GetGenericArguments()[0].Name == "Category")?
                        .GetValue(serviceCatalogsContext);

                    if (dbSet != null)
                    {
                        var anyMethod = typeof(EntityFrameworkQueryableExtensions)
                            .GetMethods()
                            .FirstOrDefault(m => m.Name == "AnyAsync" && m.GetParameters().Length == 2);

                        if (anyMethod == null)
                        {
                            _logger.LogWarning("⚠️ AnyAsync method not found via reflection for ServiceCatalogs");
                            return false;
                        }

                        var genericMethod = anyMethod.MakeGenericMethod(categoryType.ClrType);
                        var hasCategories = await (Task<bool>)genericMethod.Invoke(null, [dbSet, cancellationToken])!;
                        if (hasCategories)
                            return true;
                    }
                }
            }

            // Verificar se Locations tem cidades permitidas usando LINQ
            var locationsContext = GetDbContext("Locations");
            if (locationsContext != null)
            {
                var allowedCityType = locationsContext.Model
                    .GetEntityTypes()
                    .FirstOrDefault(e => e.ClrType.Name == "AllowedCity");

                if (allowedCityType != null)
                {
                    var dbSet = locationsContext.GetType()
                        .GetProperties()
                        .FirstOrDefault(p => p.PropertyType.IsGenericType &&
                                           p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                                           p.PropertyType.GetGenericArguments()[0].Name == "AllowedCity")?
                        .GetValue(locationsContext);

                    if (dbSet != null)
                    {
                        var anyMethod = typeof(EntityFrameworkQueryableExtensions)
                            .GetMethods()
                            .FirstOrDefault(m => m.Name == "AnyAsync" && m.GetParameters().Length == 2);

                        if (anyMethod == null)
                        {
                            _logger.LogWarning("⚠️ AnyAsync method not found via reflection for Locations");
                            return false;
                        }

                        var genericMethod = anyMethod.MakeGenericMethod(allowedCityType.ClrType);
                        var hasCities = await (Task<bool>)genericMethod.Invoke(null, [dbSet, cancellationToken])!;
                        if (hasCities)
                            return true;
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error checking existing data ({ExceptionType}), assuming empty database", ex.GetType().Name);
            return false;
        }
    }

    private async Task ExecuteSeedAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SeedServiceCatalogsAsync(cancellationToken);
            await SeedLocationsAsync(cancellationToken);
            
            // Sempre semeia providers (usa ON CONFLICT DO NOTHING)
            _logger.LogInformation("🏢 Ensuring provider seed data...");
            await SeedProvidersAsync(cancellationToken);

            _logger.LogInformation("✅ Data seed completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during data seeding");
            throw new InvalidOperationException(
                "Failed to seed development data (ServiceCatalogs, Users, Providers, Documents, Locations)",
                ex);
        }
    }

    private async Task SeedServiceCatalogsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("📦 Seeding ServiceCatalogs...");

        var context = GetDbContext("ServiceCatalogs");
        if (context == null)
        {
            _logger.LogWarning("⚠️ ServiceCatalogsDbContext not found, skipping seed");
            return;
        }

        // Categories com IDs estáveis - usar RETURNING id para capturar IDs reais
        var categories = new[]
        {
            new { Id = HealthCategoryId, Name = "Saúde", Description = "Serviços relacionados à saúde e bem-estar", DisplayOrder = 1 },
            new { Id = EducationCategoryId, Name = "Educação", Description = "Serviços educacionais e de capacitação", DisplayOrder = 2 },
            new { Id = SocialCategoryId, Name = "Assistência Social", Description = "Programas de assistência e suporte social", DisplayOrder = 3 },
            new { Id = LegalCategoryId, Name = "Jurídico", Description = "Serviços jurídicos e advocatícios", DisplayOrder = 4 },
            new { Id = HousingCategoryId, Name = "Habitação", Description = "Moradia e programas habitacionais", DisplayOrder = 5 },
            new { Id = FoodCategoryId, Name = "Alimentação", Description = "Programas de segurança alimentar", DisplayOrder = 6 }
        };

        // Build idMap to capture actual IDs from upsert
        var idMap = new Dictionary<string, Guid>();
        foreach (var cat in categories)
        {
            // PostgreSQL ON CONFLICT ... RETURNING always returns the id (whether inserted or updated)
            var result = await context.Database.SqlQueryRaw<Guid>(
                @"INSERT INTO service_catalogs.service_categories (id, name, description, is_active, display_order, created_at, updated_at) 
                  VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})
                  ON CONFLICT (name) DO UPDATE 
                    SET description = EXCLUDED.description, 
                        is_active = EXCLUDED.is_active,
                        display_order = EXCLUDED.display_order,
                        updated_at = EXCLUDED.updated_at
                  RETURNING id",
                cat.Id, cat.Name, cat.Description, true, cat.DisplayOrder, DateTime.UtcNow, DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            if (result.Count > 0)
            {
                idMap[cat.Name] = result[0];
            }
            else
            {
                // Fallback: query existing category by name if RETURNING failed
                var existingId = await context.Database.SqlQueryRaw<Guid>(
                    "SELECT id FROM service_catalogs.service_categories WHERE name = {0}",
                    cat.Name)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingId != Guid.Empty)
                {
                    idMap[cat.Name] = existingId;
                }
            }
        }

        _logger.LogInformation("✅ ServiceCatalogs: {Count} categories inserted/updated", categories.Length);

        // Services usando IDs reais das categorias do idMap
        var services = new[]
        {
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Atendimento Psicológico Gratuito",
                Description = "Atendimento psicológico individual ou em grupo",
                CategoryId = idMap.GetValueOrDefault("Saúde", HealthCategoryId),
                DisplayOrder = 1
            },
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Curso de Informática Básica",
                Description = "Curso gratuito de informática e inclusão digital",
                CategoryId = idMap.GetValueOrDefault("Educação", EducationCategoryId),
                DisplayOrder = 2
            },
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Cesta Básica",
                Description = "Distribuição mensal de cestas básicas",
                CategoryId = idMap.GetValueOrDefault("Alimentação", FoodCategoryId),
                DisplayOrder = 3
            },
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Orientação Jurídica Gratuita",
                Description = "Atendimento jurídico para questões civis e trabalhistas",
                CategoryId = idMap.GetValueOrDefault("Jurídico", LegalCategoryId),
                DisplayOrder = 4
            }
        };

        foreach (var svc in services)
        {
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO service_catalogs.services (id, name, description, category_id, is_active, display_order, created_at, updated_at) 
                  VALUES ({0}, {1}, {2}, {3}, true, {4}, {5}, {6})
                  ON CONFLICT (name) DO NOTHING",
                [svc.Id, svc.Name, svc.Description, svc.CategoryId, svc.DisplayOrder, DateTime.UtcNow, DateTime.UtcNow],
                cancellationToken);
        }

        _logger.LogInformation("✅ ServiceCatalogs: {Count} services processed (new inserted, existing ignored)", services.Length);
    }

    private async Task SeedLocationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("📍 Seeding Locations (AllowedCities)...");

        var context = GetDbContext("Locations");
        if (context == null)
        {
            _logger.LogWarning("⚠️ LocationsDbContext not found, skipping seed");
            return;
        }

        var cities = new[]
        {
            new { Id = UuidGenerator.NewId(), IbgeCode = 3143906, CityName = "Muriaé", State = "MG", Lat = -21.1294, Lon = -42.3686, Radius = 30 },
            new { Id = UuidGenerator.NewId(), IbgeCode = 3550308, CityName = "São Paulo", State = "SP", Lat = -23.5505, Lon = -46.6333, Radius = 50 },
            new { Id = UuidGenerator.NewId(), IbgeCode = 3304557, CityName = "Rio de Janeiro", State = "RJ", Lat = -22.9068, Lon = -43.1729, Radius = 40 },
            new { Id = UuidGenerator.NewId(), IbgeCode = 3106200, CityName = "Belo Horizonte", State = "MG", Lat = -19.9167, Lon = -43.9345, Radius = 40 },
            new { Id = UuidGenerator.NewId(), IbgeCode = 4106902, CityName = "Curitiba", State = "PR", Lat = -25.4244, Lon = -49.2654, Radius = 35 },
            new { Id = UuidGenerator.NewId(), IbgeCode = 4314902, CityName = "Porto Alegre", State = "RS", Lat = -30.0346, Lon = -51.2177, Radius = 30 },
            new { Id = UuidGenerator.NewId(), IbgeCode = 5300108, CityName = "Brasília", State = "DF", Lat = -15.7975, Lon = -47.8919, Radius = 40 },
            new { Id = UuidGenerator.NewId(), IbgeCode = 2927408, CityName = "Salvador", State = "BA", Lat = -12.9777, Lon = -38.5016, Radius = 35 },
            new { Id = UuidGenerator.NewId(), IbgeCode = 2304400, CityName = "Fortaleza", State = "CE", Lat = -3.7319, Lon = -38.5267, Radius = 30 },
            new { Id = UuidGenerator.NewId(), IbgeCode = 2611606, CityName = "Recife", State = "PE", Lat = -8.0476, Lon = -34.8770, Radius = 25 },
            new { Id = UuidGenerator.NewId(), IbgeCode = 1302603, CityName = "Manaus", State = "AM", Lat = -3.1190, Lon = -60.0217, Radius = 50 }
        };

        foreach (var city in cities)
        {
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO locations.allowed_cities (id, ibge_code, city_name, state_sigla, is_active, created_at, updated_at, created_by, updated_by, latitude, longitude, service_radius_km) 
                  VALUES ({0}, {1}, {2}, {3}, true, {4}, {5}, {6}, {7}, {8}, {9}, {10})
                  ON CONFLICT (city_name, state_sigla) DO UPDATE SET 
                    latitude = EXCLUDED.latitude,
                    longitude = EXCLUDED.longitude,
                    service_radius_km = EXCLUDED.service_radius_km",
                [city.Id, city.IbgeCode, city.CityName, city.State, DateTime.UtcNow, DateTime.UtcNow, "system", "system", city.Lat, city.Lon, city.Radius],
                cancellationToken);
        }

        _logger.LogInformation("✅ Locations: {Count} cities processed (updated coordinates/radius if existed)", cities.Length);
    }

    private async Task SeedProvidersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🏢 Seeding Providers...");

        var context = GetDbContext("Providers");
        if (context == null)
        {
            _logger.LogWarning("⚠️ ProvidersDbContext not found, skipping seed");
            return;
        }

        var providers = new[]
        {
            new
            {
                Id = Provider1Id,
                UserId = Provider1UserId,
                DocumentId = Provider1DocumentId,
                Name = "João Silva",
                Type = "Individual", 
                Status = "Active",
                VerificationStatus = "Pending",
                LegalName = "João Silva - Psicólogo",
                FantasyName = (string?)null,
                Description = "Psicólogo clínico com 10 anos de experiência em atendimento individual e familiar",
                Email = "joao.silva@provider.com",
                PhoneNumber = "11987654321",
                Website = (string?)"https://joaosilva.com.br",
                Street = "Av. Paulista",
                Number = "1000",
                Complement = (string?)"Sala 101",
                Neighborhood = "Bela Vista",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01310100",
                Country = "Brasil",
                // Documents
                DocumentNumber = "11111111111",
                DocumentType = "CPF"
            },
            new
            {
                Id = Provider2Id,
                UserId = Provider2UserId,
                DocumentId = Provider2DocumentId,
                Name = "Maria Santos",
                Type = "Company",
                Status = "Active",
                VerificationStatus = "Verified",
                LegalName = "Santos Serviços Sociais Ltda",
                FantasyName = (string?)"Santos Social",
                Description = "Assistência social especializada em famílias em situação de vulnerabilidade social",
                Email = "contato@santos.com.br",
                PhoneNumber = "11912345678",
                Website = (string?)"https://santos.com.br",
                Street = "Rua da Consolação",
                Number = "500",
                Complement = (string?)null,
                Neighborhood = "Consolação",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01301000",
                Country = "Brasil",
                // Documents
                DocumentNumber = "66666666000199",
                DocumentType = "CNPJ"
            },
            new
            {
                Id = Provider3Id,
                UserId = Provider3UserId,
                DocumentId = Provider3DocumentId,
                Name = "Pedro Oliveira",
                Type = "Individual",
                Status = "Suspended",
                VerificationStatus = "Rejected",
                LegalName = "Pedro Oliveira Reformas",
                FantasyName = (string?)null,
                Description = "Reformas e pequenos reparos residenciais.",
                Email = "pedro.obras@gmail.com",
                PhoneNumber = "21999887766",
                Website = (string?)null,
                Street = "Rua das Laranjeiras",
                Number = "123",
                Complement = (string?)"Casa 2",
                Neighborhood = "Laranjeiras",
                City = "Rio de Janeiro",
                State = "RJ",
                ZipCode = "22240000",
                Country = "Brasil",
                // Documents
                DocumentNumber = "22233344455",
                DocumentType = "CPF"
            }
        };

        foreach (var provider in providers)
        {
            // Inserir Provedor
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO providers.providers (
                    id, user_id, name, type, status, verification_status, 
                    legal_name, fantasy_name, description,
                    email, phone_number, website,
                    street, number, complement, neighborhood, city, state, zip_code, country,
                    is_deleted, created_at, updated_at
                  ) 
                  VALUES (
                    {0}, {1}, {2}, {3}, {4}, {5},
                    {6}, {7}, {8},
                    {9}, {10}, {11},
                    {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19},
                    false, {20}, {21}
                  )
                  ON CONFLICT (user_id) DO NOTHING",
                [
                    provider.Id, provider.UserId, provider.Name, provider.Type, provider.Status, provider.VerificationStatus,
                    provider.LegalName, provider.FantasyName, provider.Description,
                    provider.Email, provider.PhoneNumber, provider.Website,
                    provider.Street, provider.Number, provider.Complement, provider.Neighborhood, provider.City, provider.State, provider.ZipCode, provider.Country,
                    DateTime.UtcNow, DateTime.UtcNow
                ],
                cancellationToken);

            // Inserir Documento (Primário)
            // Nota: Utilizamos SQL parametrizado com EF Core para garantir segurança e prevenir injeção de SQL.
            // provider_id e id são chave primária composta
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO providers.document (
                    provider_id, id, number, document_type, is_primary
                  )
                  VALUES ({0}, {1}, {2}, {3}, true)
                  ON CONFLICT (provider_id, id) DO NOTHING",
                [provider.Id, provider.DocumentId, provider.DocumentNumber, provider.DocumentType],
                cancellationToken);
        }

        _logger.LogInformation("✅ Providers: {Count} providers processed with documents", providers.Length);
    }

    /// <summary>
    /// Retrieves a DbContext for the specified module using reflection.
    /// Naming convention: MeAjudaAi.Modules.{moduleName}.Infrastructure.Persistence.{moduleName}DbContext
    /// Example: "Users" → MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext
    /// </summary>
    private DbContext? GetDbContext(string moduleName)
    {
        try
        {
            var contextTypeName = $"MeAjudaAi.Modules.{moduleName}.Infrastructure.Persistence.{moduleName}DbContext";
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var contextType = assembly.GetType(contextTypeName);
                if (contextType != null)
                {
                    return _serviceProvider.GetService(contextType) as DbContext;
                }
            }

            _logger.LogWarning("⚠️ DbContext not found for module {ModuleName}", moduleName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obtaining DbContext for {ModuleName}", moduleName);
            return null;
        }
    }
}
