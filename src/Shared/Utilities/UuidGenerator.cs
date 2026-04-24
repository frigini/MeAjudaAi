using System.Runtime.CompilerServices;

namespace MeAjudaAi.Shared.Utilities;

/// <summary>
/// Gerador centralizado de identificadores únicos
/// </summary>
public static class UuidGenerator
{
    /// <summary>
    /// Gera um novo identificador único
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid NewId() => Guid.CreateVersion7();

    /// <summary>
    /// Gera um novo identificador único como string
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NewIdString() => Guid.CreateVersion7().ToString();

    /// <summary>
    /// Gera um novo identificador único como string sem hífens
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NewIdStringCompact() => Guid.CreateVersion7().ToString("N");

    /// <summary>
    /// Verifica se um Guid é válido (não vazio)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid(Guid guid) => guid != Guid.Empty;
}
