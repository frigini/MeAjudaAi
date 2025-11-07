using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Shared.Extensions;

/// <summary>
/// Extensões para operações com Enum
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Converte string para enum com validação e retorna Result
    /// </summary>
    /// <typeparam name="TEnum">Tipo do enum</typeparam>
    /// <param name="value">String a ser convertida</param>
    /// <param name="ignoreCase">Se deve ignorar case (padrão: true)</param>
    /// <returns>Result com enum convertido ou erro</returns>
    public static Result<TEnum> ToEnum<TEnum>(this string value, bool ignoreCase = true) 
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<TEnum>.Failure(
                Error.BadRequest($"Value cannot be null or empty for enum {typeof(TEnum).Name}"));
        }

        if (Enum.TryParse<TEnum>(value, ignoreCase, out var result) && Enum.IsDefined(typeof(TEnum), result))
        {
            return Result<TEnum>.Success(result);
        }

        var validValues = string.Join(", ", Enum.GetNames<TEnum>());
        return Result<TEnum>.Failure(
            Error.BadRequest($"Invalid {typeof(TEnum).Name}: '{value}'. Valid values are: {validValues}"));
    }

    /// <summary>
    /// Converte string para enum com valor padrão se conversão falhar
    /// </summary>
    /// <typeparam name="TEnum">Tipo do enum</typeparam>
    /// <param name="value">String a ser convertida</param>
    /// <param name="defaultValue">Valor padrão se conversão falhar</param>
    /// <param name="ignoreCase">Se deve ignorar case (padrão: true)</param>
    /// <returns>Enum convertido ou valor padrão</returns>
    public static TEnum ToEnumOrDefault<TEnum>(this string value, TEnum defaultValue, bool ignoreCase = true) 
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return Enum.TryParse<TEnum>(value, ignoreCase, out var result) && Enum.IsDefined(typeof(TEnum), result) ? result : defaultValue;
    }

    /// <summary>
    /// Verifica se uma string é um valor válido para o enum
    /// </summary>
    /// <typeparam name="TEnum">Tipo do enum</typeparam>
    /// <param name="value">String a ser verificada</param>
    /// <param name="ignoreCase">Se deve ignorar case (padrão: true)</param>
    /// <returns>True se é um valor válido</returns>
    public static bool IsValidEnum<TEnum>(this string value, bool ignoreCase = true) 
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Enum.TryParse<TEnum>(value, ignoreCase, out var result) && Enum.IsDefined(typeof(TEnum), result);
    }

    /// <summary>
    /// Obtém todos os valores válidos de um enum como strings
    /// </summary>
    /// <typeparam name="TEnum">Tipo do enum</typeparam>
    /// <returns>Array com nomes dos valores do enum</returns>
    public static string[] GetValidValues<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetNames<TEnum>();
    }

    /// <summary>
    /// Obtém uma descrição amigável dos valores válidos de um enum
    /// </summary>
    /// <typeparam name="TEnum">Tipo do enum</typeparam>
    /// <returns>String formatada com valores válidos</returns>
    public static string GetValidValuesDescription<TEnum>() where TEnum : struct, Enum
    {
        var values = GetValidValues<TEnum>();
        return $"Valid {typeof(TEnum).Name} values: {string.Join(", ", values)}";
    }
}