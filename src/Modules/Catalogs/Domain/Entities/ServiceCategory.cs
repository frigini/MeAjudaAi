using MeAjudaAi.Modules.Catalogs.Domain.Events;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Catalogs.Domain.Entities;

/// <summary>
/// Represents a service category in the catalog (e.g., "Limpeza", "Reparos").
/// Categories organize services into logical groups for easier discovery.
/// </summary>
public sealed class ServiceCategory : AggregateRoot<ServiceCategoryId>
{
    /// <summary>
    /// Name of the category.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description explaining what services belong to this category.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indicates if this category is currently active and available for use.
    /// Deactivated categories cannot be assigned to new services.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Optional display order for UI sorting.
    /// </summary>
    public int DisplayOrder { get; private set; }

    // EF Core constructor
    private ServiceCategory() { }

    /// <summary>
    /// Creates a new service category.
    /// </summary>
    /// <param name="name">Category name (required, 1-100 characters)</param>
    /// <param name="description">Optional category description (max 500 characters)</param>
    /// <param name="displayOrder">Display order for sorting (default: 0)</param>
    /// <exception cref="CatalogDomainException">Thrown when validation fails</exception>
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
    /// Updates the category information.
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
    /// Activates the category, making it available for use.
    /// </summary>
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        MarkAsUpdated();
        AddDomainEvent(new ServiceCategoryActivatedDomainEvent(Id));
    }

    /// <summary>
    /// Deactivates the category, preventing it from being assigned to new services.
    /// Existing services retain their category assignment.
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
