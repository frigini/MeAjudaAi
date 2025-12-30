namespace MeAjudaAi.Shared.Contracts.Functional;

/// <summary>
/// Representa um tipo que não retorna valor útil.
/// Usado para padronizar interfaces que podem ou não retornar valores.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Instância padrão do Unit.
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// Verifica igualdade entre instâncias Unit.
    /// </summary>
    /// <param name="other">Outra instância Unit</param>
    /// <returns>Sempre true, pois todas as instâncias Unit são iguais</returns>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Verifica igualdade com outro objeto.
    /// </summary>
    /// <param name="obj">Objeto a ser comparado</param>
    /// <returns>True se o objeto for Unit</returns>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Retorna o hash code para Unit.
    /// </summary>
    /// <returns>Sempre 0, pois todas as instâncias são iguais</returns>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Operador de igualdade.
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Operador de desigualdade.
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;

    /// <summary>
    /// Representação em string do Unit.
    /// </summary>
    public override string ToString() => "()";
}

