using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Seeding;

/// <summary>
/// Implementação do seeder de dados de desenvolvimento
/// </summary>
[ExcludeFromCodeCoverage]
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

    // IDs estáveis para Documentos dos Prestadores
    private static readonly Guid Provider1DocumentId = Guid.Parse("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa");
    private static readonly Guid Provider2DocumentId = Guid.Parse("bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb");
    private static readonly Guid Provider3DocumentId = Guid.Parse("cccccccc-3333-3333-3333-cccccccccccc");

    // Prestadores de Linhares
    private static readonly Guid ProviderLinhares1Id = Guid.Parse("10000001-1111-1111-1111-111111111111");
    private static readonly Guid ProviderLinhares2Id = Guid.Parse("10000002-2222-2222-2222-222222222222");
    private static readonly Guid ProviderLinhares3Id = Guid.Parse("10000003-3333-3333-3333-333333333333");
    private static readonly Guid ProviderLinhares4Id = Guid.Parse("10000004-4444-4444-4444-444444444444");
    private static readonly Guid ProviderLinhares5Id = Guid.Parse("10000005-5555-5555-5555-555555555555");
    private static readonly Guid ProviderLinhares6Id = Guid.Parse("10000006-6666-6666-6666-666666666666");
    private static readonly Guid ProviderLinhares7Id = Guid.Parse("10000007-7777-7777-7777-777777777777");
    private static readonly Guid ProviderLinhares8Id = Guid.Parse("10000008-8888-8888-8888-888888888888");
    private static readonly Guid ProviderLinhares9Id = Guid.Parse("10000009-9999-9999-9999-999999999999");
    private static readonly Guid ProviderLinhares10Id = Guid.Parse("10000010-0000-0000-0000-000000000000");

    // IDs User/Document Linhares (Estáveis)
    private static readonly Guid ProviderLinhares1UserId = Guid.Parse("10000001-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ProviderLinhares1DocumentId = Guid.Parse("10000001-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly Guid ProviderLinhares2UserId = Guid.Parse("10000002-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ProviderLinhares2DocumentId = Guid.Parse("10000002-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly Guid ProviderLinhares3UserId = Guid.Parse("10000003-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ProviderLinhares3DocumentId = Guid.Parse("10000003-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly Guid ProviderLinhares4UserId = Guid.Parse("10000004-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ProviderLinhares4DocumentId = Guid.Parse("10000004-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly Guid ProviderLinhares5UserId = Guid.Parse("10000005-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ProviderLinhares5DocumentId = Guid.Parse("10000005-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly Guid ProviderLinhares6UserId = Guid.Parse("10000006-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ProviderLinhares6DocumentId = Guid.Parse("10000006-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly Guid ProviderLinhares7UserId = Guid.Parse("10000007-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ProviderLinhares7DocumentId = Guid.Parse("10000007-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly Guid ProviderLinhares8UserId = Guid.Parse("10000008-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ProviderLinhares8DocumentId = Guid.Parse("10000008-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly Guid ProviderLinhares9UserId = Guid.Parse("10000009-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ProviderLinhares9DocumentId = Guid.Parse("10000009-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly Guid ProviderLinhares10UserId = Guid.Parse("10000010-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ProviderLinhares10DocumentId = Guid.Parse("10000010-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public DevelopmentDataSeeder(
        IServiceProvider serviceProvider,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedIfEmptyAsync(CancellationToken cancellationToken = default)
    {
        // Removida verificação de HasData para garantir que sempre tentamos semear/atualizar dados de desenvolvimento.
        // Os métodos de seed individuais usam ON CONFLICT para serem idempotentes.
        
        _logger.LogInformation("🌱 Starting development data seed (Idempotent)...");
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
                        var hasCategories = await (Task<bool>)genericMethod.Invoke(null, new object[] { dbSet, cancellationToken })!;
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
                        var hasCities = await (Task<bool>)genericMethod.Invoke(null, new object[] { dbSet, cancellationToken })!;
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
            await SeedProviderServicesAsync(cancellationToken);

            _logger.LogInformation("🔍 Ensuring search providers read model...");
            await SeedSearchProvidersAsync(cancellationToken);

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

        // Construir idMap para capturar IDs reais do upsert
        var idMap = new Dictionary<string, Guid>();
        foreach (var cat in categories)
        {
            // PostgreSQL ON CONFLICT ... RETURNING sempre retorna o id (seja inserido ou atualizado)
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
                // Fallback: consultar categoria existente por nome se RETURNING falhar
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
                Name = "Confeitaria",
                Description = "Bolos, doces e salgados para festas e eventos",
                CategoryId = idMap.GetValueOrDefault("Alimentação", FoodCategoryId),
                DisplayOrder = 4
            },

            // Note: DisplayOrder is scoped per category, so same values across different categories are allowed
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Orientação Jurídica Gratuita",
                Description = "Atendimento jurídico para questões civis e trabalhistas",
                CategoryId = idMap.GetValueOrDefault("Jurídico", LegalCategoryId),
                DisplayOrder = 4 // Same as Confeitaria, but different category
            },
            // Housing / Services
            new { Id = UuidGenerator.NewId(), Name = "Pedreiro", Description = "Construção e reformas em geral", CategoryId = idMap.GetValueOrDefault("Habitação", HousingCategoryId), DisplayOrder = 10 },
            new { Id = UuidGenerator.NewId(), Name = "Eletricista", Description = "Instalações e reparos elétricos", CategoryId = idMap.GetValueOrDefault("Habitação", HousingCategoryId), DisplayOrder = 11 },
            new { Id = UuidGenerator.NewId(), Name = "Encanador", Description = "Instalações hidráulicas e reparos", CategoryId = idMap.GetValueOrDefault("Habitação", HousingCategoryId), DisplayOrder = 12 },
            new { Id = UuidGenerator.NewId(), Name = "Pintor", Description = "Pintura residencial e comercial", CategoryId = idMap.GetValueOrDefault("Habitação", HousingCategoryId), DisplayOrder = 13 },
            new { Id = UuidGenerator.NewId(), Name = "Jardineiro", Description = "Manutenção de jardins e áreas verdes", CategoryId = idMap.GetValueOrDefault("Habitação", HousingCategoryId), DisplayOrder = 14 },
            new { Id = UuidGenerator.NewId(), Name = "Montador de Móveis", Description = "Montagem e desmontagem de móveis", CategoryId = idMap.GetValueOrDefault("Habitação", HousingCategoryId), DisplayOrder = 15 },
            new { Id = UuidGenerator.NewId(), Name = "Faxina", Description = "Limpeza residencial e comercial", CategoryId = idMap.GetValueOrDefault("Habitação", HousingCategoryId), DisplayOrder = 16 },
            new { Id = UuidGenerator.NewId(), Name = "Frete e Mudança", Description = "Serviços de transporte e mudança", CategoryId = idMap.GetValueOrDefault("Habitação", HousingCategoryId), DisplayOrder = 17 },
            new { Id = UuidGenerator.NewId(), Name = "Assistência Técnica", Description = "Reparo de computadores e eletrônicos", CategoryId = idMap.GetValueOrDefault("Habitação", HousingCategoryId), DisplayOrder = 18 },
            new { Id = UuidGenerator.NewId(), Name = "Serviço com nome grande", Description = "Teste de layout", CategoryId = idMap.GetValueOrDefault("Habitação", HousingCategoryId), DisplayOrder = 90 },
            new { Id = UuidGenerator.NewId(), Name = "Serviço 3", Description = "Teste de layout", CategoryId = idMap.GetValueOrDefault("Habitação", HousingCategoryId), DisplayOrder = 91 }
        };

        foreach (var svc in services)
        {
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO service_catalogs.services (id, name, description, category_id, is_active, display_order, created_at, updated_at) 
                  VALUES ({0}, {1}, {2}, {3}, true, {4}, {5}, {6})
                  ON CONFLICT (name) DO NOTHING",
                new object[] { svc.Id, svc.Name, svc.Description, svc.CategoryId, svc.DisplayOrder, DateTime.UtcNow, DateTime.UtcNow },
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
            new { Id = UuidGenerator.NewId(), IbgeCode = 1302603, CityName = "Manaus", State = "AM", Lat = -3.1190, Lon = -60.0217, Radius = 50 },
            new { Id = UuidGenerator.NewId(), IbgeCode = 3203205, CityName = "Linhares", State = "ES", Lat = -19.3909, Lon = -40.0715, Radius = 30 }
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
                new object[] { city.Id, city.IbgeCode, city.CityName, city.State, DateTime.UtcNow, DateTime.UtcNow, "system", "system", city.Lat, city.Lon, city.Radius },
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
                DocumentType = "CPF",
                AdditionalPhoneNumbers = new[] { "11987654322", "1133334444" }
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
                DocumentNumber = "66666666000199",
                DocumentType = "CNPJ",
                AdditionalPhoneNumbers = Array.Empty<string>()
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
                DocumentType = "CPF",
                AdditionalPhoneNumbers = Array.Empty<string>()
            },
            // Provedores de Linhares (Correspondendo ao SeedSearchProvidersAsync)
            new { Id = ProviderLinhares1Id, UserId = ProviderLinhares1UserId, DocumentId = ProviderLinhares1DocumentId, Name = "Carlos Construções", Type = "Individual", Status = "Active", VerificationStatus = "Verified", LegalName = "Carlos Santos", FantasyName = (string?)"Carlos Construções", Description = "Pedreiro especializado em acabamentos e reformas gerais.", Email = "carlos@example.com", PhoneNumber = "27999881111", Website = (string?)null, Street = "Av. Goverador Lindenberg", Number = "100", Complement = (string?)null, Neighborhood = "Centro", City = "Linhares", State = "ES", ZipCode = "29900000", Country = "Brasil", DocumentNumber = "11122233344", DocumentType = "CPF", AdditionalPhoneNumbers = Array.Empty<string>() },
            new { Id = ProviderLinhares2Id, UserId = ProviderLinhares2UserId, DocumentId = ProviderLinhares2DocumentId, Name = "Wanderson Cardoso", Type = "Individual", Status = "Active", VerificationStatus = "Verified", LegalName = "Wanderson Cardoso", FantasyName = (string?)"Wanderson Cardoso", Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.", Email = "emailmuitogrande@gmail.com", PhoneNumber = "(00) 0 0000 - 0000", Website = (string?)null, Street = "Rua da Conceição", Number = "200", Complement = (string?)"Loja 1", Neighborhood = "Centro", City = "Linhares", State = "ES", ZipCode = "29900000", Country = "Brasil", DocumentNumber = "12345678000199", DocumentType = "CNPJ", AdditionalPhoneNumbers = new[] { "(00) 0 0000 - 0000", "(00) 0 0000 - 0000" } },
            new { Id = ProviderLinhares3Id, UserId = ProviderLinhares3UserId, DocumentId = ProviderLinhares3DocumentId, Name = "Hidráulica Silva", Type = "Individual", Status = "Active", VerificationStatus = "Verified", LegalName = "José Silva", FantasyName = (string?)"Hidráulica Silva", Description = "Conserto de vazamentos, instalação de tubulações e caixas d'água.", Email = "silva@hidraulica.com", PhoneNumber = "27999883333", Website = (string?)null, Street = "Av. Vitória", Number = "300", Complement = (string?)null, Neighborhood = "Interlagos", City = "Linhares", State = "ES", ZipCode = "29900000", Country = "Brasil", DocumentNumber = "22233300011", DocumentType = "CPF", AdditionalPhoneNumbers = Array.Empty<string>() },
            new { Id = ProviderLinhares4Id, UserId = ProviderLinhares4UserId, DocumentId = ProviderLinhares4DocumentId, Name = "Pinturas Premium", Type = "Individual", Status = "Active", VerificationStatus = "Verified", LegalName = "Marcos Pintor", FantasyName = (string?)"Pinturas Premium", Description = "Pintura residencial, texturas, grafiato e efeitos especiais.", Email = "marcos@pinturas.com", PhoneNumber = "27999884444", Website = (string?)null, Street = "Rua Ipê", Number = "400", Complement = (string?)null, Neighborhood = "Bela Vista", City = "Linhares", State = "ES", ZipCode = "29900000", Country = "Brasil", DocumentNumber = "33344455566", DocumentType = "CPF", AdditionalPhoneNumbers = Array.Empty<string>() },
            new { Id = ProviderLinhares5Id, UserId = ProviderLinhares5UserId, DocumentId = ProviderLinhares5DocumentId, Name = "Jardins & Cia", Type = "Company", Status = "Active", VerificationStatus = "Verified", LegalName = "Jardins e Paisagismo Ltda", FantasyName = (string?)"Jardins & Cia", Description = "Manutenção de jardins, poda de árvores e paisagismo.", Email = "contato@jardins.com", PhoneNumber = "27999885555", Website = (string?)"https://jardineiros.com.br", Street = "Av. Lagoa Juparanã", Number = "500", Complement = (string?)null, Neighborhood = "Três Barras", City = "Linhares", State = "ES", ZipCode = "29900000", Country = "Brasil", DocumentNumber = "98765432000100", DocumentType = "CNPJ", AdditionalPhoneNumbers = Array.Empty<string>() },
            new { Id = ProviderLinhares6Id, UserId = ProviderLinhares6UserId, DocumentId = ProviderLinhares6DocumentId, Name = "Limpeza Total", Type = "Company", Status = "Active", VerificationStatus = "Verified", LegalName = "Limpeza Total Serviços", FantasyName = (string?)"Limpeza Total", Description = "Limpeza pós-obra, faxinas residenciais e higienização de estofados.", Email = "sac@limpezatotal.com", PhoneNumber = "27999886666", Website = (string?)"https://limpezatotal.com.br", Street = "Rua dos Jacarandás", Number = "600", Complement = (string?)null, Neighborhood = "Movelar", City = "Linhares", State = "ES", ZipCode = "29900000", Country = "Brasil", DocumentNumber = "11223344000155", DocumentType = "CNPJ", AdditionalPhoneNumbers = Array.Empty<string>() },
            new { Id = ProviderLinhares7Id, UserId = ProviderLinhares7UserId, DocumentId = ProviderLinhares7DocumentId, Name = "Montador Express", Type = "Individual", Status = "Active", VerificationStatus = "Verified", LegalName = "Ricardo Montador", FantasyName = "Montador Express", Description = "Montagem de móveis comprados na internet. Rápido e cuidadoso.", Email = "ricardo@montador.com", PhoneNumber = "27999887777", Website = (string?)null, Street = "Rua Araucária", Number = "700", Complement = (string?)null, Neighborhood = "Planalto", City = "Linhares", State = "ES", ZipCode = "29900000", Country = "Brasil", DocumentNumber = "44455566677", DocumentType = "CPF", AdditionalPhoneNumbers = Array.Empty<string>() },
            new { Id = ProviderLinhares8Id, UserId = ProviderLinhares8UserId, DocumentId = ProviderLinhares8DocumentId, Name = "Fretes do João", Type = "Individual", Status = "Active", VerificationStatus = "Verified", LegalName = "João Freteiro", FantasyName = "Fretes do João", Description = "Pequenos fretes e mudanças dentro de Linhares e região.", Email = "joao@fretes.com", PhoneNumber = "27999888888", Website = (string?)null, Street = "Av. Samuel Batista Cruz", Number = "800", Complement = (string?)null, Neighborhood = "Shell", City = "Linhares", State = "ES", ZipCode = "29900000", Country = "Brasil", DocumentNumber = "55566677788", DocumentType = "CPF", AdditionalPhoneNumbers = Array.Empty<string>() },
            new { Id = ProviderLinhares9Id, UserId = ProviderLinhares9UserId, DocumentId = ProviderLinhares9DocumentId, Name = "SOS Computadores", Type = "Individual", Status = "Active", VerificationStatus = "Verified", LegalName = "Paulo Técnico", FantasyName = "SOS Computadores", Description = "Formatação, remoção de vírus e reparo de computadores e notebooks.", Email = "paulo@soscomp.com", PhoneNumber = "27999889999", Website = (string?)null, Street = "Rua Capitão José Maria", Number = "900", Complement = (string?)"Sala 2", Neighborhood = "Centro", City = "Linhares", State = "ES", ZipCode = "29900000", Country = "Brasil", DocumentNumber = "66677788899", DocumentType = "CPF", AdditionalPhoneNumbers = Array.Empty<string>() },
            new { Id = ProviderLinhares10Id, UserId = ProviderLinhares10UserId, DocumentId = ProviderLinhares10DocumentId, Name = "Dona Maria Bolos", Type = "Individual", Status = "Active", VerificationStatus = "Verified", LegalName = "Maria Boleira", FantasyName = "Dona Maria Bolos", Description = "Bolos caseiros e doces para festas. Encomendas com antecedência.", Email = "maria@bolos.com", PhoneNumber = "27999880000", Website = (string?)"https://instagram.com/donamaria", Street = "Rua Monsenhor Pedrinha", Number = "1000", Complement = (string?)null, Neighborhood = "Araçá", City = "Linhares", State = "ES", ZipCode = "29900000", Country = "Brasil", DocumentNumber = "77788899900", DocumentType = "CPF", AdditionalPhoneNumbers = Array.Empty<string>() }
        };

        foreach (var provider in providers)
        {
            // Inserir Provedor
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO providers.providers (
                    id, user_id, name, type, status, verification_status, 
                    legal_name, fantasy_name, description,
                    email, phone_number, additional_phone_numbers, website,
                    street, number, complement, neighborhood, city, state, zip_code, country,
                    is_deleted, created_at, updated_at
                  ) 
                  VALUES (
                    {0}, {1}, {2}, {3}, {4}, {5},
                    {6}, {7}, {8},
                    {9}, {10}, {22}::jsonb, {11},
                    {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19},
                    false, {20}, {21}
                  )
                  ON CONFLICT (id) DO UPDATE SET
                    user_id = EXCLUDED.user_id,
                    name = EXCLUDED.name,
                    type = EXCLUDED.type,
                    status = EXCLUDED.status,
                    verification_status = EXCLUDED.verification_status,
                    legal_name = EXCLUDED.legal_name,
                    fantasy_name = EXCLUDED.fantasy_name,
                    description = EXCLUDED.description,
                    email = EXCLUDED.email,
                    phone_number = EXCLUDED.phone_number,
                    additional_phone_numbers = EXCLUDED.additional_phone_numbers,
                    website = EXCLUDED.website,
                    street = EXCLUDED.street,
                    number = EXCLUDED.number,
                    complement = EXCLUDED.complement,
                    neighborhood = EXCLUDED.neighborhood,
                    city = EXCLUDED.city,
                    state = EXCLUDED.state,
                    zip_code = EXCLUDED.zip_code,
                    country = EXCLUDED.country,
                    updated_at = EXCLUDED.updated_at",
                new object[] {
                    provider.Id, provider.UserId, provider.Name, provider.Type, provider.Status, provider.VerificationStatus,
                    provider.LegalName, provider.FantasyName, provider.Description,
                    provider.Email, provider.PhoneNumber, provider.Website,
                    provider.Street, provider.Number, provider.Complement, provider.Neighborhood, provider.City, provider.State, provider.ZipCode, provider.Country,
                    DateTime.UtcNow, DateTime.UtcNow,
                    JsonSerializer.Serialize(provider.AdditionalPhoneNumbers)
                },
                cancellationToken);

            // Inserir Documento (Primário)
            // Nota: Utilizamos SQL parametrizado com EF Core para garantir segurança e prevenir injeção de SQL.
            // Chave primária composta: (provider_id, document_type)
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO providers.document (
                    provider_id, number, document_type, is_primary
                  )
                  VALUES ({0}, {1}, {2}, true)
                  ON CONFLICT (provider_id, document_type) DO NOTHING",
                new object[] { provider.Id, provider.DocumentNumber, provider.DocumentType },
                cancellationToken);
        }

        _logger.LogInformation("✅ Providers: {Count} providers processed with documents", providers.Length);
    }

    private async Task SeedProviderServicesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛠️ Seeding ProviderServices...");

        var context = GetDbContext("Providers");
        var servicesContext = GetDbContext("ServiceCatalogs");

        if (context == null || servicesContext == null)
        {
            _logger.LogWarning("⚠️ ProvidersDbContext or ServiceCatalogsDbContext not found, skipping provider services seed");
            return;
        }

        // Mapping based on providersToSync in SeedSearchProvidersAsync plus manual entries for test providers
        var providerServices = new[]
        {
            new { ProviderId = Provider1Id, ServiceName = "Atendimento Psicológico Gratuito" }, // Saúde
            new { ProviderId = ProviderLinhares1Id, ServiceName = "Pedreiro" },
            new { ProviderId = ProviderLinhares2Id, ServiceName = "Serviço com nome grande" },
            new { ProviderId = ProviderLinhares2Id, ServiceName = "Serviço 3" },
            new { ProviderId = ProviderLinhares3Id, ServiceName = "Encanador" },
            new { ProviderId = ProviderLinhares4Id, ServiceName = "Pintor" },
            new { ProviderId = ProviderLinhares5Id, ServiceName = "Jardineiro" },
            new { ProviderId = ProviderLinhares6Id, ServiceName = "Faxina" },
            new { ProviderId = ProviderLinhares7Id, ServiceName = "Montador de Móveis" },
            new { ProviderId = ProviderLinhares8Id, ServiceName = "Frete e Mudança" },
            new { ProviderId = ProviderLinhares9Id, ServiceName = "Assistência Técnica" },
            new { ProviderId = ProviderLinhares10Id, ServiceName = "Confeitaria" }
        };

        var count = 0;
        foreach (var ps in providerServices)
        {
            // 1. Get ServiceId by name
            var serviceId = await servicesContext.Database.SqlQueryRaw<Guid>(
                "SELECT id AS \"Value\" FROM service_catalogs.services WHERE name = {0}", ps.ServiceName)
                .FirstOrDefaultAsync(cancellationToken);

            if (serviceId == Guid.Empty)
            {
                 _logger.LogWarning("⚠️ Service '{ServiceName}' not found. Skipping linkage for Provider {ProviderId}", ps.ServiceName, ps.ProviderId);
                 continue;
            }

            // 2. Insert into provider_services
            // Note: service_name is denormalized here too
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO providers.provider_services (provider_id, service_id, service_name, added_at)
                  VALUES ({0}, {1}, {2}, {3})
                  ON CONFLICT (provider_id, service_id) DO UPDATE
                  SET service_name = EXCLUDED.service_name",
                new object[] { ps.ProviderId, serviceId, ps.ServiceName, DateTime.UtcNow },
                cancellationToken);
            
            count++;
        }

        _logger.LogInformation("✅ ProviderServices: {Count} services linked to providers", count);
    }

    private async Task SeedSearchProvidersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Seeding SearchProviders (Read Model)...");

        var context = GetDbContext("SearchProviders");
        if (context == null)
        {
            _logger.LogWarning("⚠️ SearchProvidersDbContext not found, skipping search seed");
            return;
        }

        // Recuperar DbContexts de Origem
        var servicesContext = GetDbContext("ServiceCatalogs");

        if (servicesContext == null)
        {
             _logger.LogWarning("⚠️ ServiceCatalogsDbContext not found, skipping search seed");
             return;
        }

        // 1. Obter coordenadas de Linhares (hardcoded para performance neste seed)
        var cityLat = -19.3909;
        var cityLon = -40.0715;

        // 2. Definir lista de prestadores para sincronizar (focando nos de Linhares por enquanto)
        // Nota: Idealmente leríamos do banco de Prestadores, mas aqui vamos usar os IDs conhecidos
        // e 'simular' os dados que acabamos de inserir.
        var providersToSync = new[]
        {
            new { Id = ProviderLinhares1Id, Name = "Carlos Construções", Description = "Pedreiro especializado em acabamentos e reformas gerais.", Tier = 1, Rating = 4.8, Reviews = 15, ServiceName = "Pedreiro" },
            new { Id = ProviderLinhares2Id, Name = "Wanderson Cardoso", Description = "Lorem Ipsum is simply dummy text...", Tier = 2, Rating = 4.9, Reviews = 42, ServiceName = "Serviço com nome grande" },
            new { Id = ProviderLinhares3Id, Name = "Hidráulica Silva", Description = "Conserto de vazamentos, instalação de tubulações e caixas d'água.", Tier = 0, Rating = 4.5, Reviews = 8, ServiceName = "Encanador" },
            new { Id = ProviderLinhares4Id, Name = "Pinturas Premium", Description = "Pintura residencial, texturas, grafiato e efeitos especiais.", Tier = 1, Rating = 4.7, Reviews = 23, ServiceName = "Pintor" },
            new { Id = ProviderLinhares5Id, Name = "Jardins & Cia", Description = "Manutenção de jardins, poda de árvores e paisagismo.", Tier = 0, Rating = 5.0, Reviews = 5, ServiceName = "Jardineiro" },
            new { Id = ProviderLinhares6Id, Name = "Limpeza Total", Description = "Limpeza pós-obra, faxinas residenciais e higienização de estofados.", Tier = 2, Rating = 4.6, Reviews = 31, ServiceName = "Faxina" },
            new { Id = ProviderLinhares7Id, Name = "Montador Express", Description = "Montagem de móveis comprados na internet. Rápido e cuidadoso.", Tier = 0, Rating = 4.8, Reviews = 12, ServiceName = "Montador de Móveis" },
            new { Id = ProviderLinhares8Id, Name = "Fretes do João", Description = "Pequenos fretes e mudanças dentro de Linhares e região.", Tier = 0, Rating = 4.4, Reviews = 9, ServiceName = "Frete e Mudança" }, 
            new { Id = ProviderLinhares9Id, Name = "SOS Computadores", Description = "Formatação, remoção de vírus e reparo de computadores e notebooks.", Tier = 1, Rating = 4.9, Reviews = 56, ServiceName = "Assistência Técnica" }, 
            new { Id = ProviderLinhares10Id, Name = "Dona Maria Bolos", Description = "Bolos caseiros e doces para festas. Encomendas com antecedência.", Tier = 0, Rating = 5.0, Reviews = 110, ServiceName = "Confeitaria" } // Fallback
        };

        // Obter IDs dos serviços
        // Build a map of provider ID to all their service IDs
        var providerServiceMap = new Dictionary<Guid, List<Guid>>();
        
        // Get all service IDs from providerServices array (defined in SeedProviderServicesAsync)
        var allProviderServices = new[]
        {
            new { ProviderId = Provider1Id, ServiceName = "Atendimento Psicológico Gratuito" },
            new { ProviderId = ProviderLinhares1Id, ServiceName = "Pedreiro" },
            new { ProviderId = ProviderLinhares2Id, ServiceName = "Serviço com nome grande" },
            new { ProviderId = ProviderLinhares2Id, ServiceName = "Serviço 3" }, // Multiple services for same provider
            new { ProviderId = ProviderLinhares3Id, ServiceName = "Encanador" },
            new { ProviderId = ProviderLinhares4Id, ServiceName = "Pintor" },
            new { ProviderId = ProviderLinhares5Id, ServiceName = "Jardineiro" },
            new { ProviderId = ProviderLinhares6Id, ServiceName = "Faxina" },
            new { ProviderId = ProviderLinhares7Id, ServiceName = "Montador de Móveis" },
            new { ProviderId = ProviderLinhares8Id, ServiceName = "Frete e Mudança" },
            new { ProviderId = ProviderLinhares9Id, ServiceName = "Assistência Técnica" },
            new { ProviderId = ProviderLinhares10Id, ServiceName = "Confeitaria" }
        };

        // Resolve all service IDs and group by provider
        foreach (var ps in allProviderServices)
        {
            var serviceId = await servicesContext.Database.SqlQueryRaw<Guid>(
                "SELECT id AS \"Value\" FROM service_catalogs.services WHERE name = {0}", ps.ServiceName)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (serviceId == Guid.Empty)
            {
                _logger.LogWarning("⚠️ Service '{ServiceName}' not found. Skipping.", ps.ServiceName);
                continue;
            }

            if (!providerServiceMap.ContainsKey(ps.ProviderId))
                providerServiceMap[ps.ProviderId] = new List<Guid>();
            
            providerServiceMap[ps.ProviderId].Add(serviceId);
        }

        var syncedCount = 0;
        foreach (var p in providersToSync)
        {
            // Get all service IDs for this provider
            if (!providerServiceMap.TryGetValue(p.Id, out var serviceIds) || serviceIds.Count == 0)
            {
                _logger.LogWarning("⚠️ No services found for provider '{ProviderName}'. Skipping search index seed.", p.Name);
                continue;
            }

            // Gera coordenadas determinísticas baseadas no ID do prestador para manter a localização estável entre seeds.
            // GetHashCode não é garantido entre versões do .NET, mas é suficiente para dados de desenvolvimento.
            var random = new Random(p.Id.GetHashCode());
            var latOffset = (random.NextDouble() - 0.5) * 0.05; // +/- 0.025 graus (~2.5km)
            var lonOffset = (random.NextDouble() - 0.5) * 0.05;

            var lat = cityLat + latOffset;
            var lon = cityLon + lonOffset;

            // Inserir no SearchProviders
             var rowsAffected = await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO search_providers.searchable_providers (
                    id, provider_id, name, description, 
                    location, 
                    subscription_tier, average_rating, total_reviews, 
                    service_ids, is_active, city, state, created_at, updated_at
                  ) 
                  VALUES (
                    {0}, {1}, {2}, {3}, 
                    ST_SetSRID(ST_MakePoint({4}, {5}), 4326), 
                    {6}, {7}, {8}, 
                    {9}, true, {10}, {11}, {12}, {13}
                  )
                  ON CONFLICT (provider_id) DO UPDATE SET
                    name = EXCLUDED.name,
                    description = EXCLUDED.description,
                    location = EXCLUDED.location, 
                    average_rating = EXCLUDED.average_rating,
                    total_reviews = EXCLUDED.total_reviews,
                    service_ids = EXCLUDED.service_ids,
                    subscription_tier = EXCLUDED.subscription_tier,
                    is_active = EXCLUDED.is_active,
                    updated_at = EXCLUDED.updated_at",
                new object[] {
                    UuidGenerator.NewId(), p.Id, p.Name, p.Description,
                    lon, lat, // PostGIS usa Longitude, Latitude
                    p.Tier, (decimal)p.Rating, p.Reviews,
                    serviceIds.ToArray(), "Linhares", "ES", DateTime.UtcNow, DateTime.UtcNow
                },
                cancellationToken);
            
            if (rowsAffected > 0)
                syncedCount++;
        }

        _logger.LogInformation("✅ SearchProviders: {Count} providers synced to read model", syncedCount);
    }

    /// <summary>
    /// Obtém um DbContext para o módulo especificado usando reflexão.
    /// Convenção de nomenclatura: MeAjudaAi.Modules.{moduleName}.Infrastructure.Persistence.{moduleName}DbContext
    /// Exemplo: "Users" → MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext
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
