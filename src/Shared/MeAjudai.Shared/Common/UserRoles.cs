namespace MeAjudaAi.Shared.Common;

/// <summary>
/// System roles for authorization and access control
/// </summary>
public static class UserRoles
{
    /// <summary>
    /// Regular user with basic permissions
    /// </summary>
    public const string User = "user";
    
    /// <summary>
    /// Administrator with elevated permissions
    /// </summary>
    public const string Admin = "admin";
    
    /// <summary>
    /// Super administrator with full system access
    /// </summary>
    public const string SuperAdmin = "super-admin";
    
    /// <summary>
    /// Service provider role for business accounts
    /// </summary>
    public const string ServiceProvider = "service-provider";
    
    /// <summary>
    /// Customer role for client accounts
    /// </summary>
    public const string Customer = "customer";
    
    /// <summary>
    /// Moderator role for content management (future use)
    /// </summary>
    public const string Moderator = "moderator";

    /// <summary>
    /// Gets all available roles in the system
    /// </summary>
    public static readonly string[] AllRoles = 
    {
        User,
        Admin,
        SuperAdmin,
        ServiceProvider,
        Customer,
        Moderator
    };

    /// <summary>
    /// Gets roles that have administrative privileges
    /// </summary>
    public static readonly string[] AdminRoles = 
    {
        Admin,
        SuperAdmin
    };

    /// <summary>
    /// Gets roles available for regular user creation
    /// </summary>
    public static readonly string[] BasicRoles = 
    {
        User,
        Customer,
        ServiceProvider
    };

    /// <summary>
    /// Validates if a role is valid in the system
    /// </summary>
    /// <param name="role">Role to validate</param>
    /// <returns>True if role is valid, false otherwise</returns>
    public static bool IsValidRole(string role)
    {
        return AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates if a role has administrative privileges
    /// </summary>
    /// <param name="role">Role to check</param>
    /// <returns>True if role is admin-level, false otherwise</returns>
    public static bool IsAdminRole(string role)
    {
        return AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}