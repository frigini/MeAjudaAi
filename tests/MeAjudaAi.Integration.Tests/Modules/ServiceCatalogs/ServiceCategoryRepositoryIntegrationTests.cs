using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.DependencyInjection;
using ServiceCategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

public class ServiceCategoryRepositoryIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.ServiceCatalogs;

    private readonly Faker _faker = new("pt_BR");

    [Fact]
    public async Task AddAsync_WithValidCategory_ShouldPersistToDatabase()
    {
        ServiceCategoryEntity category;
        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            category = ServiceCategory.Create(
                _faker.Commerce.Department(),
                _faker.Lorem.Sentence());

            uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>().Add(category);
            await uow.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var persisted = await uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>()
                .TryFindAsync(category.Id);

            persisted.Should().NotBeNull();
            persisted!.Name.Should().Be(category.Name);
        }
    }

    [Fact]
    public async Task TryFindAsync_WithExistingCategory_ShouldReturnCategory()
    {
        ServiceCategoryEntity category;
        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            category = ServiceCategoryEntity.Create(_faker.Commerce.ProductName());
            uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>().Add(category);
            await uow.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var result = await uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>()
                .TryFindAsync(category.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(category.Id);
        }
    }

    [Fact]
    public async Task UpdateAsync_WithValidChanges_ShouldPersistChanges()
    {
        ServiceCategoryEntity category;
        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            category = ServiceCategoryEntity.Create("Nome Original");
            uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>().Add(category);
            await uow.SaveChangesAsync();

            category.Update("Nome Atualizado", "Descrição Atualizada", 1);
            await uow.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var persisted = await uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>()
                .TryFindAsync(category.Id);

            persisted.Should().NotBeNull();
            persisted!.Name.Should().Be("Nome Atualizado");
        }
    }

    [Fact]
    public async Task DeleteAsync_WithExistingCategory_ShouldRemoveFromDatabase()
    {
        ServiceCategoryEntity category;
        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            category = ServiceCategoryEntity.Create("Categoria para Deletar");
            uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>().Add(category);
            await uow.SaveChangesAsync();

            uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>().Delete(category);
            await uow.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var persisted = await uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>()
                .TryFindAsync(category.Id);

            persisted.Should().BeNull();
        }
    }

    private ServiceCategoryEntity CreateValidCategory() =>
        ServiceCategoryEntity.Create(
            _faker.Commerce.Department(),
            _faker.Lorem.Sentence(),
            _faker.Random.Int(0, 100));
}