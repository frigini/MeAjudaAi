using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Shared.Authorization;

/// <summary>
/// Value object compartilhado para identificadores de usuário.
/// Garante type safety e validação de identificadores em toda a aplicação.
/// Usado principalmente em interfaces de permissões e APIs entre módulos.
/// </summary>
/// <remarks>
/// Este é um value object compartilhado que pode ser usado por qualquer módulo.
/// Módulos específicos podem ter seus próprios value objects UserId mais especializados
/// que herdam deste ou implementam conversões implícitas.
/// </remarks>
public sealed class UserId : ValueObject
{
    /// <summary>
    /// Valor do identificador único do usuário
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Inicializa uma nova instância de UserId
    /// </summary>
    /// <param name="value">Valor do GUID do usuário</param>
    /// <exception cref="ArgumentException">Quando o valor é Guid.Empty</exception>
    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(value));
        
        Value = value;
    }

    /// <summary>
    /// Cria um novo UserId com um GUID único
    /// </summary>
    /// <returns>Nova instância de UserId com GUID único</returns>
    public static UserId New() => new(UuidGenerator.NewId());

    /// <summary>
    /// Cria um UserId a partir de uma string GUID
    /// </summary>
    /// <param name="guidString">String representando um GUID</param>
    /// <returns>Nova instância de UserId</returns>
    /// <exception cref="ArgumentException">Quando a string não é um GUID válido</exception>
    /// <exception cref="ArgumentNullException">Quando a string é null ou vazia</exception>
    public static UserId FromString(string guidString)
    {
        if (string.IsNullOrWhiteSpace(guidString))
            throw new ArgumentNullException(nameof(guidString), "GUID string cannot be null or empty");

        if (!Guid.TryParse(guidString, out var guid))
            throw new ArgumentException($"Invalid GUID format: {guidString}", nameof(guidString));

        return new UserId(guid);
    }

    /// <summary>
    /// Componentes para comparação de igualdade
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Conversão implícita de UserId para Guid
    /// </summary>
    public static implicit operator Guid(UserId userId) => userId.Value;

    /// <summary>
    /// Conversão implícita de Guid para UserId
    /// </summary>
    public static implicit operator UserId(Guid guid) => new(guid);

    /// <summary>
    /// Conversão implícita de string para UserId
    /// </summary>
    public static implicit operator UserId(string guidString) => FromString(guidString);

    /// <summary>
    /// Representação em string do UserId
    /// </summary>
    public override string ToString() => Value.ToString();
}