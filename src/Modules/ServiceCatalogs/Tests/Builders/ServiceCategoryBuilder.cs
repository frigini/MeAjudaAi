using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;

public class ServiceCategoryBuilder : BuilderBase<ServiceCategory>
{
    private string? _name;
    private string? _description;
    private bool _isActive = true;
    private int? _displayOrder;

    public ServiceCategoryBuilder()
    {
        Faker = new Faker<ServiceCategory>()
            .CustomInstantiator(f =>
            {
                var category = ServiceCategory.Create(
                    _name ?? f.Commerce.Department(),
                    _description ?? f.Lorem.Sentence(),
                    _displayOrder ?? f.Random.Int(1, 100)
                );

                // Define o estado de ativo/inativo
                if (!_isActive)
                {
                    category.Deactivate();
                }

                return category;
            });
    }

    public ServiceCategoryBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ServiceCategoryBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ServiceCategoryBuilder WithDisplayOrder(int? displayOrder)
    {
        _displayOrder = displayOrder;
        return this;
    }

    public ServiceCategoryBuilder AsActive()
    {
        _isActive = true;
        // CustomInstantiator will ensure category is created active
        return this;
    }

    public ServiceCategoryBuilder AsInactive()
    {
        _isActive = false;
        // CustomInstantiator will call Deactivate() after creation
        return this;
    }
}
