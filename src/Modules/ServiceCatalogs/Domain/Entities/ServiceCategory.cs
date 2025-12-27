using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;

/// <summary>
/// Representa uma categoria de serviço no catálogo (ex: "Limpeza", "Reparos").
/// Categorias organizam serviços em grupos lógicos para facilitar a descoberta.
/// </summary>
public sealed class ServiceCategory : AggregateRoot<ServiceCategoryId>
{
    /// <summary>
    /// Nome da categoria.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Descrição opcional explicando quais serviços pertencem a esta categoria.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indica se esta categoria está atualmente ativa e disponível para uso.
    /// Categorias desativadas não podem ser atribuídas a novos serviços.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Ordem de exibição opcional para ordenação na UI.
    /// </summary>
    public int DisplayOrder { get; private set; }

    // EF Core constructor
    private ServiceCategory() { }

    /// <summary>
    /// Cria uma nova categoria de serviço.
    /// </summary>
    /// <param name="name">Nome da categoria (obrigatório, 1-100 caracteres)</param>
    /// <param name="description">Descrição opcional da categoria (máx 500 caracteres)</param>
    /// <param name="displayOrder">Ordem de exibição para ordenação (padrão: 0)</param>
    /// <exception cref="CatalogDomainException">Lançada quando a validação falha</exception>
    public static ServiceCategory Create(string name, string? description = null, int displayOrder = 0)
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidateDisplayOrder(displayOrder);

        var category = new ServiceCategory
        {
            Id = ServiceCategoryId.New(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            DisplayOrder = displayOrder
        };

        category.AddDomainEvent(new ServiceCategoryCreatedDomainEvent(category.Id));
        return category;
    }

    /// <summary>
    /// Atualiza as informações da categoria.
    /// </summary>
    public void Update(string name, string? description = null, int displayOrder = 0)
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidateDisplayOrder(displayOrder);

        Name = name.Trim();
        Description = description?.Trim();
        DisplayOrder = displayOrder;
        MarkAsUpdated();

        AddDomainEvent(new ServiceCategoryUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Ativa a categoria, tornando-a disponível para uso.
    /// </summary>
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        MarkAsUpdated();
        AddDomainEvent(new ServiceCategoryActivatedDomainEvent(Id));
    }

    /// <summary>
    /// Desativa a categoria, impedindo que seja atribuída a novos serviços.
    /// Serviços existentes mantêm sua atribuição de categoria.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        MarkAsUpdated();
        AddDomainEvent(new ServiceCategoryDeactivatedDomainEvent(Id));
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new CatalogDomainException("Category name is required.");

        if (name.Trim().Length > ValidationConstants.CatalogLimits.ServiceCategoryNameMaxLength)
            throw new CatalogDomainException($"Category name cannot exceed {ValidationConstants.CatalogLimits.ServiceCategoryNameMaxLength} characters.");
    }

    private static void ValidateDescription(string? description)
    {
        if (description is not null && description.Trim().Length > ValidationConstants.CatalogLimits.ServiceCategoryDescriptionMaxLength)
            throw new CatalogDomainException($"Category description cannot exceed {ValidationConstants.CatalogLimits.ServiceCategoryDescriptionMaxLength} characters.");
    }

    private static void ValidateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new CatalogDomainException("Display order cannot be negative.");
    }
}
