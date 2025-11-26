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
    /// Status do fluxo de registro do prestador de serviços.
    /// </summary>
    /// <remarks>
    /// Controla o progresso do prestador através do processo de registro multi-etapas.
    /// </remarks>
    public EProviderStatus Status { get; private set; }

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
    /// Coleção de serviços que o prestador oferece (many-to-many com ServiceCatalogs).
    /// </summary>
    private readonly List<ProviderService> _services = [];
    public IReadOnlyCollection<ProviderService> Services => _services.AsReadOnly();

    /// <summary>
    /// Indica se o prestador de serviços foi excluído logicamente do sistema.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Data e hora da exclusão lógica do prestador de serviços (UTC).
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Motivo da suspensão do prestador (obrigatório quando Status = Suspended).
    /// </summary>
    public string? SuspensionReason { get; private set; }

    /// <summary>
    /// Motivo da rejeição do prestador (obrigatório quando Status = Rejected).
    /// </summary>
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// Construtor privado para uso do Entity Framework.
    /// </summary>
    private Provider() { }

    /// <summary>
    /// Construtor para testes que permite especificar o ID.
    /// </summary>
    public Provider(
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
        Status = EProviderStatus.PendingBasicInfo;
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
        Status = EProviderStatus.PendingBasicInfo;
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

        // Track which fields are being updated
        var updatedFields = new List<string>();

        var newName = name.Trim();
        if (Name != newName)
            updatedFields.Add("Name");

        if (!BusinessProfile.ContactInfo.Email.Equals(businessProfile.ContactInfo.Email, StringComparison.OrdinalIgnoreCase))
            updatedFields.Add("Email");

        if (BusinessProfile.LegalName != businessProfile.LegalName)
            updatedFields.Add("LegalName");

        if (BusinessProfile.FantasyName != businessProfile.FantasyName)
            updatedFields.Add("FantasyName");

        if (BusinessProfile.Description != businessProfile.Description)
            updatedFields.Add("Description");

        Name = newName;
        BusinessProfile = businessProfile;
        MarkAsUpdated();

        AddDomainEvent(new ProviderProfileUpdatedDomainEvent(
            Id.Value,
            1,
            Name,
            BusinessProfile.ContactInfo.Email,
            updatedBy,
            updatedFields.ToArray()));
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

        // Se o documento é marcado como primário, remove a flag de outros documentos
        if (document.IsPrimary)
        {
            // Recrear todos os documentos como não-primários
            for (int i = 0; i < _documents.Count; i++)
            {
                _documents[i] = _documents[i].WithPrimaryStatus(false);
            }
        }

        _documents.Add(document);
        MarkAsUpdated();

        AddDomainEvent(new ProviderDocumentAddedDomainEvent(
            Id.Value,
            1,
            document.DocumentType,
            document.Number));
    }

    /// <summary>
    /// Define um documento como primário
    /// </summary>
    /// <param name="documentType">Tipo do documento a ser definido como primário</param>
    public void SetPrimaryDocument(EDocumentType documentType)
    {
        if (IsDeleted)
            throw new ProviderDomainException("Cannot set primary document on deleted provider");

        var documentIndex = _documents.FindIndex(d => d.DocumentType == documentType);
        if (documentIndex == -1)
            throw new ProviderDomainException($"Document of type {documentType} not found");

        // Remove a flag primária de todos os documentos
        for (int i = 0; i < _documents.Count; i++)
        {
            _documents[i] = _documents[i].WithPrimaryStatus(i == documentIndex);
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Obtém o documento primário do prestador
    /// </summary>
    /// <returns>O documento primário ou null se não houver</returns>
    public Document? GetPrimaryDocument()
    {
        return _documents.FirstOrDefault(d => d.IsPrimary);
    }

    /// <summary>
    /// Obtém o documento primário ou o primeiro documento se não houver primário definido
    /// </summary>
    /// <returns>O documento primário ou o primeiro documento disponível</returns>
    public Document? GetMainDocument()
    {
        return GetPrimaryDocument() ?? _documents.FirstOrDefault();
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

        // Check for duplicate qualifications by name (case-insensitive)
        if (_qualifications.Any(q => q.Name.Equals(qualification.Name, StringComparison.OrdinalIgnoreCase)))
            throw new ProviderDomainException($"A qualification with the name '{qualification.Name}' already exists");

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
    /// <param name="skipMarkAsUpdated">Se true, não chama MarkAsUpdated (útil quando chamado junto com UpdateStatus)</param>
    public void UpdateVerificationStatus(EVerificationStatus status, string? updatedBy = null, bool skipMarkAsUpdated = false)
    {
        if (IsDeleted)
            throw new ProviderDomainException("Cannot update verification status of deleted provider");

        var previousStatus = VerificationStatus;
        VerificationStatus = status;

        if (!skipMarkAsUpdated)
            MarkAsUpdated();

        AddDomainEvent(new ProviderVerificationStatusUpdatedDomainEvent(
            Id.Value,
            1,
            previousStatus,
            status,
            updatedBy));
    }

    /// <summary>
    /// Atualiza o status do fluxo de registro do prestador de serviços.
    /// </summary>
    /// <param name="newStatus">Novo status de registro</param>
    /// <param name="updatedBy">Quem está fazendo a atualização</param>
    /// <remarks>
    /// Este método gerencia as transições entre diferentes etapas do processo de registro multi-etapas.
    /// Valida que as transições de estado sejam válidas de acordo com as regras de negócio.
    /// </remarks>
    public void UpdateStatus(EProviderStatus newStatus, string? updatedBy = null)
    {
        if (IsDeleted)
            throw new ProviderDomainException("Cannot update status of deleted provider");

        // Valida transições de estado permitidas
        ValidateStatusTransition(Status, newStatus);

        // Valida que os motivos obrigatórios estejam preenchidos
        ValidateRequiredReasons(newStatus);

        var previousStatus = Status;
        Status = newStatus;
        MarkAsUpdated();
        ClearReasonFieldsIfNeeded(newStatus);

        // Dispara eventos de domínio específicos baseado na transição
        if (newStatus == EProviderStatus.PendingDocumentVerification && previousStatus == EProviderStatus.PendingBasicInfo)
        {
            AddDomainEvent(new ProviderAwaitingVerificationDomainEvent(
                Id.Value,
                1,
                UserId,
                Name,
                updatedBy));
        }
        else if (newStatus == EProviderStatus.Active && previousStatus == EProviderStatus.PendingDocumentVerification)
        {
            AddDomainEvent(new ProviderActivatedDomainEvent(
                Id.Value,
                1,
                UserId,
                Name,
                updatedBy));
        }
    }

    /// <summary>
    /// Completa o preenchimento das informações básicas e avança para a etapa de verificação de documentos.
    /// </summary>
    /// <param name="updatedBy">Quem está fazendo a atualização</param>
    public void CompleteBasicInfo(string? updatedBy = null)
    {
        if (Status != EProviderStatus.PendingBasicInfo)
            throw new ProviderDomainException("Cannot complete basic info when not in PendingBasicInfo status");

        UpdateStatus(EProviderStatus.PendingDocumentVerification, updatedBy);
    }

    /// <summary>
    /// Retorna o prestador para correção de informações básicas durante a verificação de documentos.
    /// </summary>
    /// <param name="reason">Motivo da correção necessária (obrigatório)</param>
    /// <param name="updatedBy">Quem está solicitando a correção</param>
    public void RequireBasicInfoCorrection(string reason, string? updatedBy = null)
    {
        if (Status != EProviderStatus.PendingDocumentVerification)
            throw new ProviderDomainException("Can only require basic info correction during document verification");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ProviderDomainException("Correction reason is required");

        UpdateStatus(EProviderStatus.PendingBasicInfo, updatedBy);

        AddDomainEvent(new ProviderBasicInfoCorrectionRequiredDomainEvent(
            Id.Value,
            1,
            UserId,
            Name,
            reason,
            updatedBy));
    }

    /// <summary>
    /// Ativa o prestador após verificação bem-sucedida dos documentos.
    /// </summary>
    /// <param name="updatedBy">Quem está fazendo a ativação</param>
    public void Activate(string? updatedBy = null)
    {
        if (Status != EProviderStatus.PendingDocumentVerification)
            throw new ProviderDomainException("Can only activate providers in PendingDocumentVerification status");

        UpdateStatus(EProviderStatus.Active, updatedBy);
        UpdateVerificationStatus(EVerificationStatus.Verified, updatedBy, skipMarkAsUpdated: true);
    }

    /// <summary>
    /// Reativa um prestador previamente suspenso.
    /// </summary>
    /// <param name="updatedBy">Quem está fazendo a reativação</param>
    public void Reactivate(string? updatedBy = null)
    {
        if (Status != EProviderStatus.Suspended)
            throw new ProviderDomainException("Can only reactivate providers in Suspended status");

        UpdateStatus(EProviderStatus.Active, updatedBy);
        UpdateVerificationStatus(EVerificationStatus.Verified, updatedBy, skipMarkAsUpdated: true);
    }

    /// <summary>
    /// Suspende o prestador de serviços.
    /// </summary>
    /// <param name="reason">Motivo da suspensão (obrigatório)</param>
    /// <param name="updatedBy">Quem está fazendo a suspensão</param>
    public void Suspend(string reason, string? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ProviderDomainException("Suspension reason is required");

        if (Status == EProviderStatus.Suspended)
            return;

        if (IsDeleted)
            throw new ProviderDomainException("Cannot suspend deleted provider");

        SuspensionReason = reason;
        UpdateStatus(EProviderStatus.Suspended, updatedBy);
        UpdateVerificationStatus(EVerificationStatus.Suspended, updatedBy, skipMarkAsUpdated: true);
    }

    /// <summary>
    /// Rejeita o registro do prestador de serviços.
    /// </summary>
    /// <param name="reason">Motivo da rejeição (obrigatório)</param>
    /// <param name="updatedBy">Quem está fazendo a rejeição</param>
    public void Reject(string reason, string? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ProviderDomainException("Rejection reason is required");

        if (Status == EProviderStatus.Rejected)
            return;

        if (IsDeleted)
            throw new ProviderDomainException("Cannot reject deleted provider");

        RejectionReason = reason;
        UpdateStatus(EProviderStatus.Rejected, updatedBy);
        UpdateVerificationStatus(EVerificationStatus.Rejected, updatedBy, skipMarkAsUpdated: true);
    }

    /// <summary>
    /// Adiciona um serviço à lista de serviços oferecidos pelo provider.
    /// </summary>
    /// <param name="serviceId">ID do serviço do catálogo (ServiceCatalogs module)</param>
    /// <exception cref="ProviderDomainException">Lançada se o serviço já estiver na lista</exception>
    public void AddService(Guid serviceId)
    {
        if (serviceId == Guid.Empty)
            throw new ProviderDomainException("ServiceId cannot be empty");

        if (IsDeleted)
            throw new ProviderDomainException("Cannot add services to deleted provider");

        if (_services.Any(s => s.ServiceId == serviceId))
            throw new ProviderDomainException($"Service {serviceId} is already offered by this provider");

        var providerService = new ProviderService(Id, serviceId);
        _services.Add(providerService);
        MarkAsUpdated();

        AddDomainEvent(new ProviderServiceAddedDomainEvent(
            Id.Value,
            1,
            serviceId));
    }

    /// <summary>
    /// Remove um serviço da lista de serviços oferecidos pelo provider.
    /// </summary>
    /// <param name="serviceId">ID do serviço do catálogo</param>
    /// <exception cref="ProviderDomainException">Lançada se o serviço não estiver na lista</exception>
    public void RemoveService(Guid serviceId)
    {
        if (serviceId == Guid.Empty)
            throw new ProviderDomainException("ServiceId cannot be empty");

        if (IsDeleted)
            throw new ProviderDomainException("Cannot remove services from deleted provider");

        var providerService = _services.FirstOrDefault(s => s.ServiceId == serviceId);
        if (providerService == null)
            throw new ProviderDomainException($"Service {serviceId} is not offered by this provider");

        _services.Remove(providerService);
        MarkAsUpdated();

        AddDomainEvent(new ProviderServiceRemovedDomainEvent(
            Id.Value,
            1,
            serviceId));
    }

    /// <summary>
    /// Verifica se o provider oferece um determinado serviço.
    /// </summary>
    public bool OffersService(Guid serviceId)
    {
        return _services.Any(s => s.ServiceId == serviceId);
    }

    /// <summary>
    /// Obtém os IDs de todos os serviços oferecidos pelo provider.
    /// </summary>
    public Guid[] GetServiceIds()
    {
        return _services.Select(s => s.ServiceId).ToArray();
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
    /// Valida se uma transição de status é permitida pelas regras de negócio.
    /// </summary>
    private static void ValidateStatusTransition(EProviderStatus currentStatus, EProviderStatus newStatus)
    {
        // Permite manter o mesmo status
        if (currentStatus == newStatus)
            return;

        var allowedTransitions = new Dictionary<EProviderStatus, EProviderStatus[]>
        {
            [EProviderStatus.PendingBasicInfo] = [EProviderStatus.PendingDocumentVerification, EProviderStatus.Rejected],
            [EProviderStatus.PendingDocumentVerification] = [EProviderStatus.Active, EProviderStatus.Rejected, EProviderStatus.PendingBasicInfo],
            [EProviderStatus.Active] = [EProviderStatus.Suspended],
            [EProviderStatus.Suspended] = [EProviderStatus.Active, EProviderStatus.Rejected],
            [EProviderStatus.Rejected] = [EProviderStatus.PendingBasicInfo]
        };

        if (!allowedTransitions.TryGetValue(currentStatus, out var allowed) || !allowed.Contains(newStatus))
        {
            throw new ProviderDomainException(
                $"Invalid status transition from {currentStatus} to {newStatus}");
        }
    }

    /// <summary>
    /// Limpa os campos de motivo (SuspensionReason e RejectionReason) quando o status não corresponde mais ao motivo armazenado.
    /// </summary>
    /// <param name="newStatus">Novo status do prestador</param>
    /// <remarks>
    /// Este método garante a invariante de que os motivos de suspensão e rejeição
    /// só existem enquanto o prestador está nos estados Suspended ou Rejected, respectivamente.
    /// </remarks>
    private void ClearReasonFieldsIfNeeded(EProviderStatus newStatus)
    {
        // Limpa o motivo de suspensão se não estiver mais no estado Suspended
        if (newStatus != EProviderStatus.Suspended)
            SuspensionReason = null;

        // Limpa o motivo de rejeição se não estiver mais no estado Rejected
        if (newStatus != EProviderStatus.Rejected)
            RejectionReason = null;
    }

    /// <summary>
    /// Valida que os motivos obrigatórios estejam preenchidos ao transicionar para Suspended ou Rejected.
    /// </summary>
    /// <param name="newStatus">Novo status do prestador</param>
    /// <remarks>
    /// Este método garante a invariante de auditoria: transições para Suspended requerem SuspensionReason
    /// e transições para Rejected requerem RejectionReason.
    /// </remarks>
    private void ValidateRequiredReasons(EProviderStatus newStatus)
    {
        if (newStatus == EProviderStatus.Suspended && string.IsNullOrWhiteSpace(SuspensionReason))
            throw new ProviderDomainException("SuspensionReason is required when transitioning to Suspended status");

        if (newStatus == EProviderStatus.Rejected && string.IsNullOrWhiteSpace(RejectionReason))
            throw new ProviderDomainException("RejectionReason is required when transitioning to Rejected status");
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
