namespace MeAjudaAi.Shared.Tests.Builders;

/// <summary>
/// Padrão builder base para criar objetos de teste com Bogus
/// </summary>
public abstract class BuilderBase<T> where T : class
{
    protected Faker<T> Faker;
    private readonly List<Action<T>> _customActions = [];

    protected BuilderBase()
    {
        Faker = new Faker<T>();
    }

    /// <summary>
    /// Constrói uma única instância
    /// </summary>
    public virtual T Build()
    {
        var instance = Faker.Generate();

        // Aplica ações customizadas
        foreach (var action in _customActions)
        {
            action(instance);
        }

        return instance;
    }

    /// <summary>
    /// Constrói múltiplas instâncias
    /// </summary>
    public virtual IEnumerable<T> BuildMany(int count = 3)
    {
        for (int i = 0; i < count; i++)
        {
            yield return Build();
        }
    }

    /// <summary>
    /// Constrói uma lista de instâncias
    /// </summary>
    public virtual List<T> BuildList(int count = 3) => [.. BuildMany(count)];

    /// <summary>
    /// Adiciona uma ação customizada para ser aplicada após a criação do objeto
    /// </summary>
    protected BuilderBase<T> WithCustomAction(Action<T> action)
    {
        _customActions.Add(action);
        return this;
    }

    /// <summary>
    /// Conversão implícita para T por conveniência
    /// </summary>
    public static implicit operator T(BuilderBase<T> builder) => builder.Build();
}
