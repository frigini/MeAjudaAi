using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;

public class ServiceBuilder : BaseBuilder<Service>
{
    private ServiceCategoryId? _categoryId;
    private string? _name;
    private string? _description;
    private bool _isActive = true;
    private int? _displayOrder = null;

    public ServiceBuilder()
    {
        Faker = new Faker<Service>()
            .CustomInstantiator(f =>
            {
                var generatedName = f.Commerce.ProductName();
                var generatedDescription = f.Commerce.ProductDescription();

                var service = Service.Create(
                    _categoryId ?? ServiceCategoryId.New(),
                    _name ?? (generatedName.Length <= 150 ? generatedName : generatedName[..150]),
                    _description ?? (generatedDescription.Length <= 1000 ? generatedDescription : generatedDescription[..1000]),
                    _displayOrder ?? f.Random.Int(0, 100)
                );

                // Define o estado de ativo/inativo
                if (!_isActive)
                {
                    service.Deactivate();
                }

                return service;
            });
    }

    public ServiceBuilder WithCategoryId(ServiceCategoryId categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public ServiceBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = new ServiceCategoryId(categoryId);
        return this;
    }

    public ServiceBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ServiceBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ServiceBuilder WithDisplayOrder(int displayOrder)
    {
        _displayOrder = displayOrder;
        return this;
    }

    public ServiceBuilder AsActive()
    {
        _isActive = true;
        return this;
    }

    public ServiceBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }
}
