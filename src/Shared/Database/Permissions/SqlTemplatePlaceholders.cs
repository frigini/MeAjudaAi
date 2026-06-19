using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Constantes para os placeholders usados nos scripts SQL de permissions (01-permissions.sql).
/// Evita magic strings e garante consistência entre o manager e os templates SQL.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class SqlTemplatePlaceholders
{
    public const string RoleName = "{{ROLE_NAME}}";
    public const string SchemaNameLiteral = "{{SCHEMA_NAME_LITERAL}}";
    public const string RoleOwnerName = "{{ROLE_OWNER_NAME}}";
    public const string AppRoleName = "{{APP_ROLE_NAME}}";
    public const string RolePassword = "{{ROLE_PASSWORD}}";
    public const string AppRolePassword = "{{APP_ROLE_PASSWORD}}";
    public const string SchemaName = "{{SCHEMA_NAME}}";

    public const string RolePwdParam = "role_pwd";
    public const string AppPwdParam = "app_pwd";
}
