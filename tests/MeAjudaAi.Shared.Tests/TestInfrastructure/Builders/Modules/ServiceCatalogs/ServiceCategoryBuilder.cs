using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.ServiceCatalogs;

[ExcludeFromCodeCoverage]
public class ServiceCategoryBuilder : BaseBuilder<ServiceCategory>
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

    public ServiceCategoryBuilder WithDescription(string? description)
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
        return this;
    }

    public ServiceCategoryBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }
}
