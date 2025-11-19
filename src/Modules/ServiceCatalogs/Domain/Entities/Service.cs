using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;

/// <summary>
/// Representa um serviço específico que provedores podem oferecer (ex: "Limpeza de Apartamento", "Conserto de Torneira").
/// Serviços pertencem a uma categoria e podem ser ativados/desativados por administradores.
/// </summary>
public sealed class Service : AggregateRoot<ServiceId>
{
    /// <summary>
    /// ID da categoria à qual este serviço pertence.
    /// </summary>
    public ServiceCategoryId CategoryId { get; private set; } = null!;

    /// <summary>
    /// Nome do serviço.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Descrição opcional explicando o que este serviço inclui.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indica se este serviço está atualmente ativo e disponível para provedores oferecerem.
    /// Serviços desativados são ocultados do catálogo.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Ordem de exibição opcional dentro da categoria para ordenação na UI.
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation property (loaded explicitly when needed)
    public ServiceCategory? Category { get; private set; }

    // EF Core constructor
    private Service() { }

    /// <summary>
    /// Cria um novo serviço dentro de uma categoria.
    /// </summary>
    /// <param name="categoryId">ID da categoria pai</param>
    /// <param name="name">Nome do serviço (obrigatório, 1-150 caracteres)</param>
    /// <param name="description">Descrição opcional do serviço (máx 1000 caracteres)</param>
    /// <param name="displayOrder">Ordem de exibição para ordenação (padrão: 0)</param>
    /// <exception cref="CatalogDomainException">Lançada quando a validação falha</exception>
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
    /// Atualiza as informações do serviço.
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
    /// Altera a categoria deste serviço.
    /// </summary>
    public void ChangeCategory(ServiceCategoryId newCategoryId)
    {
        if (newCategoryId is null)
            throw new CatalogDomainException("Category ID is required.");

        if (CategoryId.Value == newCategoryId.Value)
            return;

        var oldCategoryId = CategoryId;
        CategoryId = newCategoryId;
        Category = null; // Invalidar navegação para forçar recarga quando necessário
        MarkAsUpdated();

        AddDomainEvent(new ServiceCategoryChangedDomainEvent(Id, oldCategoryId, newCategoryId));
    }

    /// <summary>
    /// Ativa o serviço, tornando-o disponível no catálogo.
    /// </summary>
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        MarkAsUpdated();
        AddDomainEvent(new ServiceActivatedDomainEvent(Id));
    }

    /// <summary>
    /// Desativa o serviço, removendo-o do catálogo.
    /// Provedores que atualmente oferecem este serviço o mantêm, mas novas atribuições são impedidas.
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
