using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.DependencyInjection;
using ServiceEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;
using ServiceCategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

public class ServiceRepositoryIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.ServiceCatalogs;

    private readonly Faker _faker = new("pt_BR");

    [Fact]
    public async Task AddAsync_WithValidService_ShouldPersistToDatabase()
    {
        ServiceEntity service;
        ServiceCategoryEntity category;
        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            category = CreateValidCategory();
            uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>().Add(category);

            service = ServiceEntity.Create(
                category.Id,
                _faker.Commerce.ProductName(),
                _faker.Lorem.Sentence());

            uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>().Add(service);
            await uow.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var persisted = await uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>()
                .TryFindAsync(service.Id);

            persisted.Should().NotBeNull();
            persisted!.Name.Should().Be(service.Name);
        }
    }

    [Fact]
    public async Task TryFindAsync_WithExistingService_ShouldReturnService()
    {
        ServiceEntity service;
        ServiceCategoryEntity category;
        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            category = CreateValidCategory();
            uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>().Add(category);
            await uow.SaveChangesAsync();

            service = ServiceEntity.Create(category.Id, _faker.Commerce.ProductName());
            uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>().Add(service);
            await uow.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var result = await uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>()
                .TryFindAsync(service.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(service.Id);
        }
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleServices_ShouldReturnAll()
    {
        ServiceEntity s1, s2;
        ServiceCategoryEntity category;
        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            category = CreateValidCategory();
            uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>().Add(category);
            await uow.SaveChangesAsync();

            s1 = ServiceEntity.Create(category.Id, "Serviço " + Guid.NewGuid());
            s2 = ServiceEntity.Create(category.Id, "Serviço " + Guid.NewGuid());

            uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>().Add(s1);
            uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>().Add(s2);
            await uow.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var queries = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.IServiceQueries>();
            var services = await queries.GetAllAsync(false);

            services.Should().Contain(s => s.Id == s1.Id);
            services.Should().Contain(s => s.Id == s2.Id);
        }
    }

    [Fact]
    public async Task UpdateAsync_WithValidChanges_ShouldPersistChanges()
    {
        ServiceEntity service;
        ServiceCategoryEntity category;
        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            category = CreateValidCategory();
            uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>().Add(category);
            await uow.SaveChangesAsync();

            service = ServiceEntity.Create(category.Id, "Nome Original");
            uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>().Add(service);
            await uow.SaveChangesAsync();

            service.Update("Nome Atualizado", "Descrição Atualizada", 1);
            await uow.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var persisted = await uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>()
                .TryFindAsync(service.Id);

            persisted.Should().NotBeNull();
            persisted!.Name.Should().Be("Nome Atualizado");
        }
    }

    [Fact]
    public async Task DeleteAsync_WithExistingService_ShouldRemoveFromDatabase()
    {
        ServiceEntity service;
        ServiceCategoryEntity category;
        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            category = CreateValidCategory();
            uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>().Add(category);
            await uow.SaveChangesAsync();

            service = ServiceEntity.Create(category.Id, "Serviço para Deletar");
            uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>().Add(service);
            await uow.SaveChangesAsync();

            uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>().Delete(service);
            await uow.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var persisted = await uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>()
                .TryFindAsync(service.Id);

            persisted.Should().BeNull();
        }
    }

    private ServiceCategoryEntity CreateValidCategory() =>
        ServiceCategoryEntity.Create(
            _faker.Commerce.Department(),
            _faker.Lorem.Sentence(),
            _faker.Random.Int(0, 100));
}