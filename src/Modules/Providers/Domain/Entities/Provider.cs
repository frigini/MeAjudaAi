using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Providers.Domain.Entities;

/// <summary>
/// Representa um prestador de serviços como raiz de agregado.
/// Implementa o padrão Domain-Driven Design com eventos de domínio e value objects.
/// </summary>
/// <remarks>
/// Esta classe encapsula todas as regras de negócio relacionadas aos prestadores de serviços,
/// incluindo registro, atualização de perfil, verificação e gestão de documentos.
/// Integra-se com o módulo Users para autenticação através do UserId.
/// </remarks>
public sealed class Provider : AggregateRoot<ProviderId>
{
    /// <summary>
    /// Identificador do usuário no sistema Keycloak.
    /// </summary>
    /// <remarks>
    /// Referência ao usuário autenticado no módulo Users.
    /// </remarks>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Nome do prestador de serviços.
    /// </summary>
    /// <remarks>
    /// Pode ser o nome pessoal (Individual) ou nome fantasia (Company).
    /// </remarks>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Tipo do prestador de serviços (Individual ou Company).
    /// </summary>
    public EProviderType Type { get; private set; }

    /// <summary>
    /// Perfil empresarial do prestador de serviços.
    /// </summary>
    /// <remarks>
    /// Contém informações de identidade empresarial e contato primário.
    /// </remarks>
    public BusinessProfile BusinessProfile { get; private set; } = null!;

    /// <summary>
    /// Status de verificação do prestador de serviços.
    /// </summary>
    public EVerificationStatus VerificationStatus { get; private set; }

    /// <summary>
    /// Coleção de documentos validados do prestador de serviços.
    /// </summary>
    private readonly List<Document> _documents = [];
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    /// <summary>
    /// Coleção de qualificações do prestador de serviços.
    /// </summary>
    private readonly List<Qualification> _qualifications = [];
    public IReadOnlyCollection<Qualification> Qualifications => _qualifications.AsReadOnly();

    /// <summary>
    /// Indica se o prestador de serviços foi excluído logicamente do sistema.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Data e hora da exclusão lógica do prestador de serviços (UTC).
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Construtor privado para uso do Entity Framework.
    /// </summary>
    private Provider() { }

    /// <summary>
    /// Construtor interno para testes que permite especificar o ID.
    /// </summary>
    internal Provider(
        ProviderId id,
        Guid userId,
        string name,
        EProviderType type,
        BusinessProfile businessProfile)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(businessProfile);

        // Validações de regras de negócio específicas para criação
        ValidateProviderCreation(userId, name);

        UserId = userId;
        Name = name.Trim();
        Type = type;
        BusinessProfile = businessProfile;
        VerificationStatus = EVerificationStatus.Pending;

        // Não adiciona eventos de domínio para testes
    }

    /// <summary>
    /// Cria um novo prestador de serviços no sistema.
    /// </summary>
    /// <param name="userId">ID do usuário no Keycloak</param>
    /// <param name="name">Nome do prestador de serviços</param>
    /// <param name="type">Tipo do prestador de serviços</param>
    /// <param name="businessProfile">Perfil empresarial</param>
    /// <remarks>
    /// Este construtor dispara automaticamente o evento ProviderRegisteredDomainEvent.
    /// </remarks>
    /// <exception cref="ProviderDomainException">Thrown when business rules are violated</exception>
    public Provider(
        Guid userId,
        string name,
        EProviderType type,
        BusinessProfile businessProfile)
        : base(ProviderId.New())
    {
        ArgumentNullException.ThrowIfNull(businessProfile);

        // Validações de regras de negócio específicas para criação
        ValidateProviderCreation(userId, name);

        UserId = userId;
        Name = name.Trim();
        Type = type;
        BusinessProfile = businessProfile;
        VerificationStatus = EVerificationStatus.Pending;

        AddDomainEvent(new ProviderRegisteredDomainEvent(
            Id.Value,
            1,
            UserId,
            Name,
            Type,
            BusinessProfile.ContactInfo.Email));
    }

    /// <summary>
    /// Atualiza as informações básicas do prestador de serviços.
    /// </summary>
    /// <param name="name">Novo nome</param>
    /// <param name="businessProfile">Novo perfil empresarial</param>
    /// <param name="updatedBy">Quem está fazendo a atualização</param>
    public void UpdateProfile(string name, BusinessProfile businessProfile, string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(businessProfile);

        if (string.IsNullOrWhiteSpace(name))
            throw new ProviderDomainException("Name cannot be empty");

        if (IsDeleted)
            throw new ProviderDomainException("Cannot update deleted provider");

        Name = name.Trim();
        BusinessProfile = businessProfile;
        MarkAsUpdated();

        AddDomainEvent(new ProviderProfileUpdatedDomainEvent(
            Id.Value,
            1,
            Name,
            BusinessProfile.ContactInfo.Email,
            updatedBy));
    }

    /// <summary>
    /// Adiciona um documento ao prestador de serviços.
    /// </summary>
    /// <param name="document">Documento a ser adicionado</param>
    public void AddDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (IsDeleted)
            throw new ProviderDomainException("Cannot add document to deleted provider");

        // Verifica se já existe um documento do mesmo tipo
        if (_documents.Any(d => d.DocumentType == document.DocumentType))
            throw new ProviderDomainException($"Document of type {document.DocumentType} already exists");

        _documents.Add(document);
        MarkAsUpdated();

        AddDomainEvent(new ProviderDocumentAddedDomainEvent(
            Id.Value,
            1,
            document.DocumentType,
            document.Number));
    }

    /// <summary>
    /// Remove um documento do prestador de serviços.
    /// </summary>
    /// <param name="documentType">Tipo do documento a ser removido</param>
    public void RemoveDocument(EDocumentType documentType)
    {
        if (IsDeleted)
            throw new ProviderDomainException("Cannot remove document from deleted provider");

        var document = _documents.FirstOrDefault(d => d.DocumentType == documentType) ?? throw new ProviderDomainException($"Document of type {documentType} not found");
        _documents.Remove(document);
        MarkAsUpdated();

        AddDomainEvent(new ProviderDocumentRemovedDomainEvent(
            Id.Value,
            1,
            documentType,
            document.Number));
    }

    /// <summary>
    /// Adiciona uma qualificação ao prestador de serviços.
    /// </summary>
    /// <param name="qualification">Qualificação a ser adicionada</param>
    public void AddQualification(Qualification qualification)
    {
        ArgumentNullException.ThrowIfNull(qualification);

        if (IsDeleted)
            throw new ProviderDomainException("Cannot add qualification to deleted provider");

        _qualifications.Add(qualification);
        MarkAsUpdated();

        AddDomainEvent(new ProviderQualificationAddedDomainEvent(
            Id.Value,
            1,
            qualification.Name,
            qualification.IssuingOrganization));
    }

    /// <summary>
    /// Remove uma qualificação do prestador de serviços.
    /// </summary>
    /// <param name="qualificationName">Nome da qualificação a ser removida</param>
    public void RemoveQualification(string qualificationName)
    {
        if (string.IsNullOrWhiteSpace(qualificationName))
            throw new ArgumentException("Qualification name cannot be empty", nameof(qualificationName));

        if (IsDeleted)
            throw new ProviderDomainException("Cannot remove qualification from deleted provider");

        var qualification = _qualifications.FirstOrDefault(q => q.Name.Equals(qualificationName, StringComparison.OrdinalIgnoreCase)) ?? throw new ProviderDomainException($"Qualification '{qualificationName}' not found");
        _qualifications.Remove(qualification);
        MarkAsUpdated();

        AddDomainEvent(new ProviderQualificationRemovedDomainEvent(
            Id.Value,
            1,
            qualificationName,
            qualification.IssuingOrganization));
    }

    /// <summary>
    /// Atualiza o status de verificação do prestador de serviços.
    /// </summary>
    /// <param name="status">Novo status de verificação</param>
    /// <param name="updatedBy">Quem está fazendo a atualização</param>
    public void UpdateVerificationStatus(EVerificationStatus status, string? updatedBy = null)
    {
        if (IsDeleted)
            throw new ProviderDomainException("Cannot update verification status of deleted provider");

        var previousStatus = VerificationStatus;
        VerificationStatus = status;
        MarkAsUpdated();

        AddDomainEvent(new ProviderVerificationStatusUpdatedDomainEvent(
            Id.Value,
            1,
            previousStatus,
            status,
            updatedBy));
    }

    /// <summary>
    /// Exclui logicamente o prestador de serviços do sistema.
    /// </summary>
    /// <param name="dateTimeProvider">Provedor de data/hora para auditoria</param>
    /// <param name="deletedBy">Quem está fazendo a exclusão</param>
    /// <remarks>
    /// Implementa exclusão lógica (soft delete) em vez de remoção física dos dados.
    /// Dispara o evento ProviderDeletedDomainEvent quando a exclusão é realizada.
    /// Se o prestador já estiver excluído, o método retorna sem fazer alterações.
    /// </remarks>
    public void Delete(IDateTimeProvider dateTimeProvider, string? deletedBy = null)
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = dateTimeProvider.CurrentDate();
        MarkAsUpdated();

        AddDomainEvent(new ProviderDeletedDomainEvent(
            Id.Value,
            1,
            Name,
            deletedBy));
    }

    /// <summary>
    /// Valida as regras de negócio para criação de prestador de serviços.
    /// </summary>
    private static void ValidateProviderCreation(Guid userId, string name)
    {
        if (userId == Guid.Empty)
            throw new ProviderDomainException("UserId cannot be empty");

        if (string.IsNullOrWhiteSpace(name))
            throw new ProviderDomainException("Name cannot be empty");

        if (name.Length < 2)
            throw new ProviderDomainException("Name must be at least 2 characters long");

        if (name.Length > 100)
            throw new ProviderDomainException("Name cannot exceed 100 characters");
    }
}
