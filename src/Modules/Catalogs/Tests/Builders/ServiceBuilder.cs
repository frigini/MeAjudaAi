using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.Builders;

namespace MeAjudaAi.Modules.Catalogs.Tests.Builders;

public class ServiceBuilder : BuilderBase<Service>
{
    private ServiceCategoryId? _categoryId;
    private string? _name;
    private string? _description;
    private bool _isActive = true;
    private int _displayOrder;

    public ServiceBuilder()
    {
        Faker = new Faker<Service>()
            .CustomInstantiator(f =>
            {
                var service = Service.Create(
                    _categoryId ?? new ServiceCategoryId(Guid.NewGuid()),
                    _name ?? f.Commerce.ProductName(),
                    _description ?? f.Commerce.ProductDescription(),
                    _displayOrder > 0 ? _displayOrder : f.Random.Int(1, 100)
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

    public ServiceBuilder WithDescription(string description)
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
        WithCustomAction(service => service.Activate());
        return this;
    }

    public ServiceBuilder AsInactive()
    {
        _isActive = false;
        WithCustomAction(service => service.Deactivate());
        return this;
    }
}
