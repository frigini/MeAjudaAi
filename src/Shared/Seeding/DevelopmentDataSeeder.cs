using MeAjudaAi.Shared.Time;
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
            _logger.LogInformation("🔍 Banco de dados já possui dados, pulando seed");
            return;
        }

        _logger.LogInformation("🌱 Banco vazio detectado, iniciando seed de dados de desenvolvimento...");
        await ExecuteSeedAsync(cancellationToken);
    }

    public async Task ForceSeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("🔄 Executando seed de dados (garante dados mínimos)...");
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
                            .First(m => m.Name == "AnyAsync" && m.GetParameters().Length == 2)
                            .MakeGenericMethod(categoryType.ClrType);

                        var hasCategories = await (Task<bool>)anyMethod.Invoke(null, [dbSet, cancellationToken])!;
                        return hasCategories;
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
                            .First(m => m.Name == "AnyAsync" && m.GetParameters().Length == 2)
                            .MakeGenericMethod(allowedCityType.ClrType);

                        var hasCities = await (Task<bool>)anyMethod.Invoke(null, [dbSet, cancellationToken])!;
                        return hasCities;
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Erro ao verificar dados existentes, assumindo banco vazio");
            return false;
        }
    }

    private async Task ExecuteSeedAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SeedServiceCatalogsAsync(cancellationToken);
            await SeedLocationsAsync(cancellationToken);

            _logger.LogInformation("✅ Seed de dados concluído com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro durante seed de dados");
            throw;
        }
    }

    private async Task SeedServiceCatalogsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("📦 Seeding ServiceCatalogs...");

        var context = GetDbContext("ServiceCatalogs");
        if (context == null)
        {
            _logger.LogWarning("⚠️ ServiceCatalogsDbContext não encontrado, pulando seed");
            return;
        }

        // Categories com IDs estáveis - usar RETURNING id para capturar IDs reais
        var categories = new[]
        {
            new { Id = HealthCategoryId, Name = "Saúde", Description = "Serviços relacionados à saúde e bem-estar" },
            new { Id = EducationCategoryId, Name = "Educação", Description = "Serviços educacionais e de capacitação" },
            new { Id = SocialCategoryId, Name = "Assistência Social", Description = "Programas de assistência e suporte social" },
            new { Id = LegalCategoryId, Name = "Jurídico", Description = "Serviços jurídicos e advocatícios" },
            new { Id = HousingCategoryId, Name = "Habitação", Description = "Moradia e programas habitacionais" },
            new { Id = FoodCategoryId, Name = "Alimentação", Description = "Programas de segurança alimentar" }
        };

        // Build idMap to capture actual IDs from upsert
        var idMap = new Dictionary<string, Guid>();
        foreach (var cat in categories)
        {
            var result = await context.Database.SqlQueryRaw<Guid>(
                @"INSERT INTO service_catalogs.categories (id, name, description, created_at, updated_at) 
                  VALUES ({0}, {1}, {2}, {3}, {4})
                  ON CONFLICT (name) DO UPDATE SET description = {2}, updated_at = {4}
                  RETURNING id",
                cat.Id, cat.Name, cat.Description, DateTime.UtcNow, DateTime.UtcNow)
                .ToListAsync(cancellationToken);
            
            if (result.Count > 0)
            {
                idMap[cat.Name] = result[0];
            }
        }

        _logger.LogInformation("✅ ServiceCatalogs: {Count} categorias inseridas/atualizadas", categories.Length);

        // Services usando IDs reais das categorias do idMap
        var services = new[]
        {
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Atendimento Psicológico Gratuito",
                Description = "Atendimento psicológico individual ou em grupo",
                CategoryId = HealthCategoryId,
                Criteria = "Renda familiar até 3 salários mínimos",
                Documents = "{\"RG\",\"CPF\",\"Comprovante de residência\",\"Comprovante de renda\"}"
            },
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Curso de Informática Básica",
                Description = "Curso gratuito de informática e inclusão digital",
                CategoryId = EducationCategoryId,
                Criteria = "Jovens de 14 a 29 anos",
                Documents = "{\"RG\",\"CPF\",\"Comprovante de escolaridade\"}"
            },
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Cesta Básica",
                Description = "Distribuição mensal de cestas básicas",
                CategoryId = FoodCategoryId,
                Criteria = "Famílias em situação de vulnerabilidade",
                Documents = "{\"Cadastro único\",\"Comprovante de residência\"}"
            },
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Orientação Jurídica Gratuita",
                Description = "Atendimento jurídico para questões civis e trabalhistas",
                CategoryId = LegalCategoryId,
                Criteria = "Renda familiar até 2 salários mínimos",
                Documents = "{\"RG\",\"CPF\",\"Documentos relacionados ao caso\"}"
            }
        };

        foreach (var svc in services)
        {
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO service_catalogs.services (id, name, description, category_id, eligibility_criteria, required_documents, created_at, updated_at, is_active) 
                  VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, true)
                  ON CONFLICT (name) DO NOTHING",
                [svc.Id, svc.Name, svc.Description, svc.CategoryId, svc.Criteria, svc.Documents, DateTime.UtcNow, DateTime.UtcNow],
                cancellationToken);
        }

        _logger.LogInformation("✅ ServiceCatalogs: {Count} serviços inseridos", services.Length);
    }

    private async Task SeedLocationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("📍 Seeding Locations (AllowedCities)...");

        var context = GetDbContext("Locations");
        if (context == null)
        {
            _logger.LogWarning("⚠️ LocationsDbContext não encontrado, pulando seed");
            return;
        }

        var cities = new[]
        {
            new { Id = UuidGenerator.NewId(), IbgeCode = "3143906", CityName = "Muriaé", State = "MG" },
            new { Id = UuidGenerator.NewId(), IbgeCode = "3550308", CityName = "São Paulo", State = "SP" },
            new { Id = UuidGenerator.NewId(), IbgeCode = "3304557", CityName = "Rio de Janeiro", State = "RJ" },
            new { Id = UuidGenerator.NewId(), IbgeCode = "3106200", CityName = "Belo Horizonte", State = "MG" },
            new { Id = UuidGenerator.NewId(), IbgeCode = "4106902", CityName = "Curitiba", State = "PR" },
            new { Id = UuidGenerator.NewId(), IbgeCode = "4314902", CityName = "Porto Alegre", State = "RS" },
            new { Id = UuidGenerator.NewId(), IbgeCode = "5300108", CityName = "Brasília", State = "DF" },
            new { Id = UuidGenerator.NewId(), IbgeCode = "2927408", CityName = "Salvador", State = "BA" },
            new { Id = UuidGenerator.NewId(), IbgeCode = "2304400", CityName = "Fortaleza", State = "CE" },
            new { Id = UuidGenerator.NewId(), IbgeCode = "2611606", CityName = "Recife", State = "PE" },
            new { Id = UuidGenerator.NewId(), IbgeCode = "1302603", CityName = "Manaus", State = "AM" }
        };

        foreach (var city in cities)
        {
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO locations.allowed_cities (id, ibge_code, city_name, state, is_active, created_at, updated_at) 
                  VALUES ({0}, {1}, {2}, {3}, true, {4}, {5})
                  ON CONFLICT (ibge_code) DO NOTHING",
                [city.Id, city.IbgeCode, city.CityName, city.State, DateTime.UtcNow, DateTime.UtcNow],
                cancellationToken);
        }

        _logger.LogInformation("✅ Locations: {Count} cidades inseridas", cities.Length);
    }

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

            _logger.LogWarning("⚠️ DbContext não encontrado para módulo {ModuleName}", moduleName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao obter DbContext para {ModuleName}", moduleName);
            return null;
        }
    }
}
