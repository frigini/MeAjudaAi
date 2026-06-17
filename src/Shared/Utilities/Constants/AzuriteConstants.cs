using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes para o Azurite (emulador local do Azure Storage).
/// Credenciais padrão documentadas pela Microsoft:
/// https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite
/// </summary>
[ExcludeFromCodeCoverage]
public static class AzuriteConstants
{
    public const string AccountName = "devstoreaccount1";

    /// <summary>
    /// Chave de conta padrão do Azurite (não é secreta - é um emulador local).
    /// </summary>
    public const string AccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
}
