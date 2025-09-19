using System.Text.RegularExpressions;

namespace MeAjudaAi.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Value object que representa um nome de usuário válido.
/// </summary>
/// <remarks>
/// Implementa validações específicas para nomes de usuário incluindo:
/// - Tamanho mínimo de 3 caracteres e máximo de 30 caracteres
/// - Caracteres permitidos: letras, números, pontos, underscores e hífens
/// - Conversão automática para minúsculas para padronização
/// - Garantia de unicidade no sistema através de validações de negócio
/// </remarks>
public sealed partial record Username
{
    /// <summary>
    /// Regex compilada para validação de formato de nome de usuário.
    /// </summary>
    private static readonly Regex UsernameRegex = UsernameGeneratedRegex();

    /// <summary>
    /// O valor do nome de usuário em formato padronizado (minúsculas).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Cria um novo nome de usuário com validação.
    /// </summary>
    /// <param name="value">O nome de usuário a ser validado</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando:
    /// - O nome de usuário é nulo, vazio ou apenas espaços em branco
    /// - O nome de usuário tem menos de 3 caracteres
    /// - O nome de usuário excede 30 caracteres
    /// - O nome de usuário contém caracteres inválidos
    /// </exception>
    public Username(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Username cannot be empty", nameof(value));

        if (value.Length < 3)
            throw new ArgumentException("Username must be at least 3 characters", nameof(value));

        if (value.Length > 30)
            throw new ArgumentException("Username cannot exceed 30 characters", nameof(value));

        if (!UsernameRegex.IsMatch(value))
            throw new ArgumentException("Username contains invalid characters", nameof(value));

        Value = value.ToLowerInvariant();
    }

    /// <summary>
    /// Conversão implícita de Username para string.
    /// </summary>
    /// <param name="username">O Username a ser convertido</param>
    /// <returns>O valor string do nome de usuário</returns>
    public static implicit operator string(Username username) => username.Value;
    
    /// <summary>
    /// Conversão implícita de string para Username.
    /// </summary>
    /// <param name="username">A string a ser convertida em Username</param>
    /// <returns>Nova instância de Username validada</returns>
    public static implicit operator Username(string username) => new(username);

    /// <summary>
    /// Regex gerada em tempo de compilação para validação de nome de usuário.
    /// Permite letras, números, pontos, underscores e hífens.
    /// </summary>
    /// <returns>Instância de Regex compilada para validação de nome de usuário</returns>
    [GeneratedRegex(@"^[a-zA-Z0-9._-]{3,30}$", RegexOptions.Compiled)]
    private static partial Regex UsernameGeneratedRegex();
}