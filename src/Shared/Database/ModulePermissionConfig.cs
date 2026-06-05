namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Configuração de permissões por módulo.
/// </summary>
public record ModulePermissionConfig(
    string ModuleName,
    string SchemaName,
    string RoleName,
    string RolePassword,
    string AppRoleName,
    string AppRolePassword);
