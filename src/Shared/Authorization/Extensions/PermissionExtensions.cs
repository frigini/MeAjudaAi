using MeAjudaAi.Shared.Authorization.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MeAjudaAi.Shared.Authorization.Extensions;

/// <summary>
/// Extensions para facilitar o trabalho com permissões de forma type-safe.
/// </summary>
public static class PermissionExtensions
{
    /// <summary>
    /// Obtém o valor string da permissão definido no atributo Display.
    /// </summary>
    /// <param name="permission">A permissão</param>
    /// <returns>O valor string da permissão</returns>
    public static string GetValue(this EPermission permission)
    {
        var field = permission.GetType().GetField(permission.ToString());
        var attribute = field?.GetCustomAttribute<DisplayAttribute>();
        return attribute?.Name ?? permission.ToString();
    }

    /// <summary>
    /// Obtém o módulo da permissão baseado no prefixo do valor.
    /// </summary>
    /// <param name="permission">A permissão</param>
    /// <returns>O nome do módulo</returns>
    public static string GetModule(this EPermission permission)
    {
        var value = permission.GetValue();
        var colonIndex = value.IndexOf(':', StringComparison.Ordinal);
        return colonIndex > 0 ? value[..colonIndex] : "unknown";
    }

    /// <summary>
    /// Converte uma string de permissão para o enum correspondente.
    /// </summary>
    /// <param name="permissionValue">O valor string da permissão</param>
    /// <returns>A permissão enum ou null se não encontrada</returns>
    public static EPermission? FromValue(string permissionValue)
    {
        if (string.IsNullOrWhiteSpace(permissionValue))
            return null;

        var permissions = Enum.GetValues<EPermission>();

        foreach (var permission in permissions)
        {
            if (permission.GetValue().Equals(permissionValue, StringComparison.OrdinalIgnoreCase))
                return permission;
        }

        return null;
    }

    /// <summary>
    /// Obtém todas as permissões de um módulo específico.
    /// </summary>
    /// <param name="module">O nome do módulo</param>
    /// <returns>Lista de permissões do módulo</returns>
    public static IEnumerable<EPermission> GetPermissionsByModule(string module)
    {
        if (string.IsNullOrWhiteSpace(module))
            return [];

        var permissions = Enum.GetValues<EPermission>();

        return permissions.Where(p => p.GetModule().Equals(module, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Obtém todos os módulos disponíveis no sistema.
    /// </summary>
    /// <returns>Lista de nomes de módulos</returns>
    public static IEnumerable<string> GetAllModules()
    {
        var permissions = Enum.GetValues<EPermission>();
        return permissions.Select(p => p.GetModule()).Distinct().OrderBy(m => m);
    }

    /// <summary>
    /// Verifica se uma permissão é de administração do sistema.
    /// </summary>
    /// <param name="permission">A permissão</param>
    /// <returns>True se for permissão de admin</returns>
    public static bool IsAdminPermission(this EPermission permission)
    {
        return permission.GetModule().Equals("admin", StringComparison.OrdinalIgnoreCase);
    }
}
