using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;

/// <summary>
/// Identificador único para SearchableProvider usando o padrão Strongly-Typed ID.
/// 
/// NOTA: Este é um Strongly-Typed ID (record struct wrapper sobre Guid), não um Value Object
/// tradicional no sentido DDD. Não herda de ValueObject porque:
/// 1. Records já fornecem igualdade por valor automaticamente
/// 2. É um ID simples sem lógica de domínio complexa
/// 3. Segue o padrão moderno de Strongly-Typed IDs do C# 10+
/// 
/// Permanece na pasta ValueObjects por convenção (IDs são conceitualmente value objects),
/// mas usa estrutura de record ao invés de herança de classe base.
/// </summary>
public sealed record SearchableProviderId(Guid Value)
{
    public static SearchableProviderId New() => new(UuidGenerator.NewId());

    public static SearchableProviderId From(Guid value) => new(value);

    public static implicit operator Guid(SearchableProviderId id) => id.Value;
}
