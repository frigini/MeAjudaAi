using System.Text.RegularExpressions;

namespace MeAjudaAi.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Value object que representa um endereço de email válido.
/// </summary>
/// <remarks>
/// Implementa validações rigorosas de formato de email usando regex compilada.
/// Garante que o email seja único, válido e esteja dentro dos limites de tamanho.
/// O valor é automaticamente convertido para minúsculas para padronização.
/// </remarks>
public sealed partial record Email
{
    /// <summary>
    /// Regex compilada para validação de formato de email.
    /// </summary>
    private static readonly Regex EmailRegex = EmailGeneratedRegex();

    /// <summary>
    /// O valor do endereço de email em formato padronizado (minúsculas).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Cria um novo endereço de email com validação.
    /// </summary>
    /// <param name="value">O endereço de email a ser validado</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando:
    /// - O email é nulo, vazio ou apenas espaços em branco
    /// - O email excede 254 caracteres (limite padrão RFC)
    /// - O formato do email é inválido
    /// </exception>
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        if (value.Length > 254)
            throw new ArgumentException("Email cannot exceed 254 characters", nameof(value));

        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException("Invalid email format", nameof(value));

        Value = value.ToLowerInvariant();
    }

    /// <summary>
    /// Conversão implícita de Email para string.
    /// </summary>
    /// <param name="email">O Email a ser convertido</param>
    /// <returns>O valor string do email</returns>
    public static implicit operator string(Email email) => email.Value;
    
    /// <summary>
    /// Conversão implícita de string para Email.
    /// </summary>
    /// <param name="email">A string a ser convertida em Email</param>
    /// <returns>Nova instância de Email validada</returns>
    public static implicit operator Email(string email) => new(email);

    /// <summary>
    /// Regex gerada em tempo de compilação para validação de email.
    /// </summary>
    /// <returns>Instância de Regex compilada para validação de email</returns>
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex EmailGeneratedRegex();
}