namespace MeAjudaAi.Modules.Providers.Domain.Enums;

/// <summary>
/// Status do fluxo de registro do prestador de serviços.
/// </summary>
/// <remarks>
/// Este enum representa os diferentes estágios do processo de registro multi-etapas,
/// permitindo que os prestadores de serviços salvem seu progresso e completem o registro
/// de forma incremental.
/// </remarks>
public enum EProviderStatus
{
    /// <summary>
    /// Status não definido
    /// </summary>
    None = 0,

    /// <summary>
    /// Aguardando preenchimento das informações básicas.
    /// O prestador iniciou o registro mas ainda não completou as informações essenciais.
    /// </summary>
    PendingBasicInfo = 1,

    /// <summary>
    /// Aguardando envio e verificação de documentos.
    /// As informações básicas foram preenchidas, mas documentos ainda precisam ser enviados.
    /// </summary>
    PendingDocumentVerification = 2,

    /// <summary>
    /// Prestador ativo e verificado.
    /// Todas as etapas foram completadas e o prestador está apto a oferecer serviços.
    /// </summary>
    Active = 3,

    /// <summary>
    /// Prestador suspenso.
    /// A conta foi temporariamente desativada por violação de políticas ou outros motivos.
    /// </summary>
    Suspended = 4,

    /// <summary>
    /// Prestador rejeitado.
    /// O processo de verificação foi concluído, mas o prestador não atendeu aos requisitos.
    /// </summary>
    Rejected = 5
}
