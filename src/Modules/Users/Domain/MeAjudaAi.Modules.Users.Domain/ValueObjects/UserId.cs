using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Value object que representa o identificador único de um usuário.
/// </summary>
/// <remarks>
/// Implementa o padrão Value Object para garantir imutabilidade e validação
/// do identificador do usuário. Encapsula um Guid e fornece validações básicas.
/// </remarks>
public class UserId : ValueObject
{
    /// <summary>
    /// O valor do identificador como Guid.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Cria um novo identificador de usuário.
    /// </summary>
    /// <param name="value">O valor Guid para o identificador</param>
    /// <exception cref="ArgumentException">Lançada quando o Guid fornecido é vazio</exception>
    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty");

        Value = value;
    }

    /// <summary>
    /// Cria um novo identificador de usuário com um Guid aleatório.
    /// </summary>
    /// <returns>Nova instância de UserId com um Guid único</returns>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>
    /// Fornece os componentes para comparação de igualdade.
    /// </summary>
    /// <returns>Componentes usados para determinar igualdade entre instâncias</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Conversão implícita de UserId para Guid.
    /// </summary>
    /// <param name="userId">O UserId a ser convertido</param>
    /// <returns>O valor Guid do UserId</returns>
    public static implicit operator Guid(UserId userId) => userId.Value;
    
    /// <summary>
    /// Conversão implícita de Guid para UserId.
    /// </summary>
    /// <param name="guid">O Guid a ser convertido</param>
    /// <returns>Nova instância de UserId</returns>
    public static implicit operator UserId(Guid guid) => new(guid);
}