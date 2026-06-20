using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.Domain.ValueObjects;

/// <summary>
/// Resposta de uma operação de gateway de pagamento para criação de assinatura.
/// </summary>
/// <remarks>
/// Encapsula o resultado de uma chamada ao provedor de pagamento (ex: Stripe),
/// contendo os dados de sucesso ou informação de falha.
/// </remarks>
/// <param name="Success">Indica se a operação foi bem-sucedida.</param>
/// <param name="ExternalSubscriptionId">ID da assinatura no provedor externo. Preenchido quando <see cref="Success"/> é true.</param>
/// <param name="CheckoutUrl">URL de checkout para o cliente. Preenchido quando <see cref="Success"/> é true.</param>
/// <param name="ErrorMessage">Mensagem de erro quando a operação falhou. Preenchido quando <see cref="Success"/> é false.</param>
[ExcludeFromCodeCoverage]
public record SubscriptionGatewayResponse(
    bool Success,
    string? ExternalSubscriptionId,
    string? CheckoutUrl,
    string? ErrorMessage)
{
    /// <summary>
    /// Cria uma resposta de sucesso com os dados da assinatura criada.
    /// </summary>
    /// <param name="externalSubscriptionId">ID da assinatura no provedor externo.</param>
    /// <param name="checkoutUrl">URL de checkout para o cliente.</param>
    /// <returns>Resposta indicando sucesso.</returns>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="checkoutUrl"/> é nulo ou vazio.</exception>
    public static SubscriptionGatewayResponse Succeeded(string? externalSubscriptionId, string checkoutUrl)
    {
        if (string.IsNullOrWhiteSpace(checkoutUrl))
            throw new ArgumentException("CheckoutUrl is required for successful result.", nameof(checkoutUrl));

        return new SubscriptionGatewayResponse(true, externalSubscriptionId, checkoutUrl, null);
    }

    /// <summary>
    /// Cria uma resposta de falha com a mensagem de erro.
    /// </summary>
    /// <param name="errorMessage">Mensagem descrevendo o motivo da falha.</param>
    /// <returns>Resposta indicando falha.</returns>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="errorMessage"/> é nulo ou vazio.</exception>
    public static SubscriptionGatewayResponse Failed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("ErrorMessage is required for failed result.", nameof(errorMessage));

        return new SubscriptionGatewayResponse(false, null, null, errorMessage);
    }
}
