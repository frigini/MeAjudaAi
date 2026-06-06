using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Authorization.Core.Models;

/// <summary>
/// Configuração de permissões por módulo.
/// </summary>
[ExcludeFromCodeCoverage]
public record ModulePermissionConfig(
    string ModuleName,
    string SchemaName,
    string RoleName,
    string RolePassword,
    string AppRoleName,
    string AppRolePassword);
