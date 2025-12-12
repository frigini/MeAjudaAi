using MeAjudaAi.Shared.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Seeding;

/// <summary>
/// Implementação do seeder de dados de desenvolvimento
/// </summary>
public class DevelopmentDataSeeder : IDevelopmentDataSeeder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

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
        _logger.LogWarning("🔄 Forçando re-seed de dados (sobrescreverá existentes)...");
        await ExecuteSeedAsync(cancellationToken);
    }

    public async Task<bool> HasDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar se ServiceCatalogs tem categorias
            var serviceCatalogsContext = GetDbContext("ServiceCatalogs");
            if (serviceCatalogsContext != null)
            {
                var categoriesTable = serviceCatalogsContext.Model
                    .GetEntityTypes()
                    .FirstOrDefault(e => e.ClrType.Name == "Category");

                if (categoriesTable != null)
                {
                    var count = await serviceCatalogsContext.Database
                        .ExecuteSqlRawAsync("SELECT COUNT(*) FROM service_catalogs.categories", cancellationToken);

                    return count > 0;
                }
            }

            // Verificar se Locations tem cidades permitidas
            var locationsContext = GetDbContext("Locations");
            if (locationsContext != null)
            {
                var allowedCitiesTable = locationsContext.Model
                    .GetEntityTypes()
                    .FirstOrDefault(e => e.ClrType.Name == "AllowedCity");

                if (allowedCitiesTable != null)
                {
                    var count = await locationsContext.Database
                        .ExecuteSqlRawAsync("SELECT COUNT(*) FROM locations.allowed_cities", cancellationToken);

                    return count > 0;
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

    private async Task ExecuteSeedAsync()
    {
        try
        {
            await SeedServiceCatalogsAsync();
            await SeedLocationsAsync();

            _logger.LogInformation("✅ Seed de dados concluído com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro durante seed de dados");
            throw;
        }
    }

    private async Task SeedServiceCatalogsAsync()
    {
        _logger.LogInformation("📦 Seeding ServiceCatalogs...");

        var context = GetDbContext("ServiceCatalogs");
        if (context == null)
        {
            _logger.LogWarning("⚠️ ServiceCatalogsDbContext não encontrado, pulando seed");
            return;
        }

        // Categories
        var categories = new[]
        {
            new { Id = UuidGenerator.NewId(), Name = "Saúde", Description = "Serviços relacionados à saúde e bem-estar" },
            new { Id = UuidGenerator.NewId(), Name = "Educação", Description = "Serviços educacionais e de capacitação" },
            new { Id = UuidGenerator.NewId(), Name = "Assistência Social", Description = "Programas de assistência e suporte social" },
            new { Id = UuidGenerator.NewId(), Name = "Jurídico", Description = "Serviços jurídicos e advocatícios" },
            new { Id = UuidGenerator.NewId(), Name = "Habitação", Description = "Moradia e programas habitacionais" },
            new { Id = UuidGenerator.NewId(), Name = "Alimentação", Description = "Programas de segurança alimentar" }
        };

        foreach (var cat in categories)
        {
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO service_catalogs.categories (id, name, description, created_at, updated_at) 
                  VALUES ({0}, {1}, {2}, {3}, {4})
                  ON CONFLICT (name) DO NOTHING",
                cat.Id, cat.Name, cat.Description, DateTime.UtcNow, DateTime.UtcNow);
        }

        _logger.LogInformation("✅ ServiceCatalogs: {Count} categorias inseridas", categories.Length);

        // Services (usando ID da primeira categoria como exemplo)
        var healthCategoryId = categories[0].Id;
        var educationCategoryId = categories[1].Id;
        var foodCategoryId = categories[5].Id;
        var legalCategoryId = categories[3].Id;

        var services = new[]
        {
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Atendimento Psicológico Gratuito",
                Description = "Atendimento psicológico individual ou em grupo",
                CategoryId = healthCategoryId,
                Criteria = "Renda familiar até 3 salários mínimos",
                Documents = "{\"RG\",\"CPF\",\"Comprovante de residência\",\"Comprovante de renda\"}"
            },
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Curso de Informática Básica",
                Description = "Curso gratuito de informática e inclusão digital",
                CategoryId = educationCategoryId,
                Criteria = "Jovens de 14 a 29 anos",
                Documents = "{\"RG\",\"CPF\",\"Comprovante de escolaridade\"}"
            },
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Cesta Básica",
                Description = "Distribuição mensal de cestas básicas",
                CategoryId = foodCategoryId,
                Criteria = "Famílias em situação de vulnerabilidade",
                Documents = "{\"Cadastro único\",\"Comprovante de residência\"}"
            },
            new
            {
                Id = UuidGenerator.NewId(),
                Name = "Orientação Jurídica Gratuita",
                Description = "Atendimento jurídico para questões civis e trabalhistas",
                CategoryId = legalCategoryId,
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
                svc.Id, svc.Name, svc.Description, svc.CategoryId, svc.Criteria, svc.Documents, DateTime.UtcNow, DateTime.UtcNow);
        }

        _logger.LogInformation("✅ ServiceCatalogs: {Count} serviços inseridos", services.Length);
    }

    private async Task SeedLocationsAsync()
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
                city.Id, city.IbgeCode, city.CityName, city.State, DateTime.UtcNow, DateTime.UtcNow);
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
