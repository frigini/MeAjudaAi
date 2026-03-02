namespace MeAjudaAi.Contracts.Identity.Enums;

/// <summary>
/// Specifies the different identity providers available via Keycloak.
/// Used to type the identity provider hints across frontend and backend.
/// </summary>
public enum EAuthProvider
{
    /// <summary>
    /// Google Social Login
    /// </summary>
    Google,
    
    /// <summary>
    /// Microsoft Social Login
    /// </summary>
    Microsoft,

    /// <summary>
    /// Facebook Social Login
    /// </summary>
    Facebook,

    /// <summary>
    /// Apple Social Login
    /// </summary>
    Apple
}
