using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.Builders;

namespace MeAjudaAi.Modules.Catalogs.Tests.Builders;

public class ServiceCategoryBuilder : BuilderBase<ServiceCategory>
{
    private string? _name;
    private string? _description;
    private bool _isActive = true;
    private int _displayOrder;

    public ServiceCategoryBuilder()
    {
        Faker = new Faker<ServiceCategory>()
            .CustomInstantiator(f =>
            {
                var category = ServiceCategory.Create(
                    _name ?? f.Commerce.Department(),
                    _description ?? f.Lorem.Sentence(),
                    _displayOrder > 0 ? _displayOrder : f.Random.Int(1, 100)
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

    public ServiceCategoryBuilder WithDisplayOrder(int displayOrder)
    {
        _displayOrder = displayOrder;
        return this;
    }

    public ServiceCategoryBuilder AsActive()
    {
        _isActive = true;
        WithCustomAction(category => category.Activate());
        return this;
    }

    public ServiceCategoryBuilder AsInactive()
    {
        _isActive = false;
        WithCustomAction(category => category.Deactivate());
        return this;
    }

    public ServiceCategoryBuilder WithCreatedAt(DateTime createdAt)
    {
        WithCustomAction(category =>
        {
            var createdAtField = typeof(ServiceCategory).BaseType?.GetField("<CreatedAt>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            createdAtField?.SetValue(category, createdAt);
        });
        return this;
    }

    public ServiceCategoryBuilder WithUpdatedAt(DateTime? updatedAt)
    {
        WithCustomAction(category =>
        {
            var updatedAtField = typeof(ServiceCategory).BaseType?.GetField("<UpdatedAt>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            updatedAtField?.SetValue(category, updatedAt);
        });
        return this;
    }
}
