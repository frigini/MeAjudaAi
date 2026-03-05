namespace MeAjudaAi.Contracts.Identity.Enums;

/// <summary>
/// Especifica os diferentes provedores de identidade disponíveis via Keycloak.
/// Utilizado para tipar as indicações de provedor de identidade entre o frontend e o backend.
/// </summary>
public enum EAuthProvider
{
    /// <summary>
    /// Login social do Google
    /// </summary>
    Google,
    
    /// <summary>
    /// Login social da Microsoft
    /// </summary>
    Microsoft,

    /// <summary>
    /// Login social do Facebook
    /// </summary>
    Facebook,

    /// <summary>
    /// Login social da Apple
    /// </summary>
    Apple
}
