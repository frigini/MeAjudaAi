using MeAjudaAi.Modules.Catalogs.Domain.Events;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Catalogs.Domain.Entities;

/// <summary>
/// Represents a specific service that providers can offer (e.g., "Limpeza de Apartamento", "Conserto de Torneira").
/// Services belong to a category and can be activated/deactivated by administrators.
/// </summary>
public sealed class Service : AggregateRoot<ServiceId>
{
    /// <summary>
    /// ID of the category this service belongs to.
    /// </summary>
    public ServiceCategoryId CategoryId { get; private set; } = null!;

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description explaining what this service includes.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indicates if this service is currently active and available for providers to offer.
    /// Deactivated services are hidden from the catalog.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Optional display order within the category for UI sorting.
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation property (loaded explicitly when needed)
    public ServiceCategory? Category { get; private set; }

    // EF Core constructor
    private Service() { }

    /// <summary>
    /// Creates a new service within a category.
    /// </summary>
    /// <param name="categoryId">ID of the parent category</param>
    /// <param name="name">Service name (required, 1-150 characters)</param>
    /// <param name="description">Optional service description (max 1000 characters)</param>
    /// <param name="displayOrder">Display order for sorting (default: 0)</param>
    /// <exception cref="CatalogDomainException">Thrown when validation fails</exception>
    public static Service Create(ServiceCategoryId categoryId, string name, string? description = null, int displayOrder = 0)
    {
        if (categoryId is null)
            throw new CatalogDomainException("Category ID is required.");

        ValidateName(name);
        ValidateDescription(description);
        ValidateDisplayOrder(displayOrder);

        var service = new Service
        {
            Id = ServiceId.New(),
            CategoryId = categoryId,
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            DisplayOrder = displayOrder
        };

        service.AddDomainEvent(new ServiceCreatedDomainEvent(service.Id, categoryId));
        return service;
    }

    /// <summary>
    /// Updates the service information.
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

        AddDomainEvent(new ServiceUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Changes the category of this service.
    /// </summary>
    public void ChangeCategory(ServiceCategoryId newCategoryId)
    {
        if (newCategoryId is null)
            throw new CatalogDomainException("Category ID is required.");

        if (CategoryId.Value == newCategoryId.Value)
            return;

        var oldCategoryId = CategoryId;
        CategoryId = newCategoryId;
        MarkAsUpdated();

        AddDomainEvent(new ServiceCategoryChangedDomainEvent(Id, oldCategoryId, newCategoryId));
    }

    /// <summary>
    /// Activates the service, making it available in the catalog.
    /// </summary>
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        MarkAsUpdated();
        AddDomainEvent(new ServiceActivatedDomainEvent(Id));
    }

    /// <summary>
    /// Deactivates the service, removing it from the catalog.
    /// Providers who currently offer this service retain it, but new assignments are prevented.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        MarkAsUpdated();
        AddDomainEvent(new ServiceDeactivatedDomainEvent(Id));
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new CatalogDomainException("Service name is required.");

        if (name.Trim().Length > ValidationConstants.CatalogLimits.ServiceNameMaxLength)
            throw new CatalogDomainException($"Service name cannot exceed {ValidationConstants.CatalogLimits.ServiceNameMaxLength} characters.");
    }

    private static void ValidateDescription(string? description)
    {
        if (description is not null && description.Trim().Length > ValidationConstants.CatalogLimits.ServiceDescriptionMaxLength)
            throw new CatalogDomainException($"Service description cannot exceed {ValidationConstants.CatalogLimits.ServiceDescriptionMaxLength} characters.");
    }

    private static void ValidateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new CatalogDomainException("Display order cannot be negative.");
    }
}
