namespace MeAjudaAi.Shared.Security;

/// <summary>
/// Pap�is do sistema para autoriza��o e controle de acesso
/// </summary>
public static class UserRoles
{
    /// <summary>
    /// Usu�rio comum com permiss�es b�sicas
    /// </summary>
    public const string User = "user";
    
    /// <summary>
    /// Administrador com permiss�es elevadas
    /// </summary>
    public const string Admin = "admin";
    
    /// <summary>
    /// Super administrador com acesso total ao sistema
    /// </summary>
    public const string SuperAdmin = "super-admin";
    
    /// <summary>
    /// Papel de prestador de servi�o para contas empresariais
    /// </summary>
    public const string ServiceProvider = "service-provider";
    
    /// <summary>
    /// Papel de cliente para contas de usu�rio final
    /// </summary>
    public const string Customer = "customer";
    
    /// <summary>
    /// Papel de moderador para gest�o de conte�do (uso futuro)
    /// </summary>
    public const string Moderator = "moderator";

    /// <summary>
    /// Obt�m todos os pap�is dispon�veis no sistema
    /// </summary>
    public static readonly string[] AllRoles = 
    [
        User,
        Admin,
        SuperAdmin,
        ServiceProvider,
        Customer,
        Moderator
    ];

    /// <summary>
    /// Obt�m pap�is que possuem privil�gios administrativos
    /// </summary>
    public static readonly string[] AdminRoles = 
    [
        Admin,
        SuperAdmin
    ];

    /// <summary>
    /// Obt�m pap�is dispon�veis para cria��o de usu�rio comum
    /// </summary>
    public static readonly string[] BasicRoles = 
    [
        User,
        Customer,
        ServiceProvider
    ];

    /// <summary>
    /// Valida se um papel � v�lido no sistema
    /// </summary>
    /// <param name="role">Papel a ser validado</param>
    /// <returns>True se o papel for v�lido, false caso contr�rio</returns>
    public static bool IsValidRole(string role)
    {
        return AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Valida se um papel possui privil�gios administrativos
    /// </summary>
    /// <param name="role">Papel a ser verificado</param>
    /// <returns>True se o papel for de n�vel admin, false caso contr�rio</returns>
    public static bool IsAdminRole(string role)
    {
        return AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}