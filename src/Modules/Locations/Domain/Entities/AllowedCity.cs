namespace MeAjudaAi.Modules.Locations.Domain.Entities;

/// <summary>
/// Entidade que representa uma cidade permitida para operação de prestadores.
/// Usado para validação geográfica centralizada via banco de dados.
/// </summary>
public sealed class AllowedCity
{
    /// <summary>
    /// Identificador único da cidade permitida
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Nome da cidade (ex: "Muriaé", "Itaperuna")
    /// </summary>
    public string CityName { get; private set; } = string.Empty;

    /// <summary>
    /// Sigla do estado (ex: "MG", "RJ", "ES")
    /// </summary>
    public string StateSigla { get; private set; } = string.Empty;

    /// <summary>
    /// Código IBGE do município (opcional - preenchido via integração IBGE)
    /// </summary>
    public int? IbgeCode { get; private set; }

    /// <summary>
    /// Indica se a cidade está ativa para operação
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Data de criação do registro
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Usuário que criou o registro (Admin)
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Usuário que fez a última atualização
    /// </summary>
    public string? UpdatedBy { get; private set; }

    // EF Core constructor
    private AllowedCity() { }

    public AllowedCity(
        string cityName,
        string stateSigla,
        string createdBy,
        int? ibgeCode = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            throw new ArgumentException("Nome da cidade não pode ser vazio", nameof(cityName));

        if (string.IsNullOrWhiteSpace(stateSigla))
            throw new ArgumentException("Sigla do estado não pode ser vazia", nameof(stateSigla));

        if (stateSigla.Length != 2)
            throw new ArgumentException("Sigla do estado deve ter 2 caracteres", nameof(stateSigla));

        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy não pode ser vazio", nameof(createdBy));

        Id = Guid.NewGuid();
        CityName = cityName.Trim();
        StateSigla = stateSigla.Trim().ToUpperInvariant();
        IbgeCode = ibgeCode;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    public void Update(string cityName, string stateSigla, int? ibgeCode, bool isActive, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            throw new ArgumentException("Nome da cidade não pode ser vazio", nameof(cityName));

        if (string.IsNullOrWhiteSpace(stateSigla))
            throw new ArgumentException("Sigla do estado não pode ser vazia", nameof(stateSigla));

        if (stateSigla.Length != 2)
            throw new ArgumentException("Sigla do estado deve ter 2 caracteres", nameof(stateSigla));

        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy não pode ser vazio", nameof(updatedBy));

        CityName = cityName.Trim();
        StateSigla = stateSigla.Trim().ToUpperInvariant();
        IbgeCode = ibgeCode;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Activate(string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy não pode ser vazio", nameof(updatedBy));

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Deactivate(string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy não pode ser vazio", nameof(updatedBy));

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}
