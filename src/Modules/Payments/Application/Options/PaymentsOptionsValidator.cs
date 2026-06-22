using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Modules.Payments.Application.Options;

public class PaymentsOptionsValidator : IValidateOptions<PaymentsOptions>
{
    private readonly IConfiguration _configuration;

    public PaymentsOptionsValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ValidateOptionsResult Validate(string? name, PaymentsOptions options)
    {
        if (name != PaymentsOptions.SectionName && name != null)
        {
            return ValidateOptionsResult.Skip;
        }

        var errors = new List<string>();
        var isBypass = EnvironmentHelpers.IsSecurityBypassEnvironment();

        if (string.IsNullOrWhiteSpace(options.SuccessUrl) && !isBypass)
        {
            errors.Add("Payments:SuccessUrl is missing or empty in configuration.");
        }

        if (string.IsNullOrWhiteSpace(options.CancelUrl) && !isBypass)
        {
            errors.Add("Payments:CancelUrl is missing or empty in configuration.");
        }

        if (options.Plans.Count == 0 && !isBypass)
        {
            errors.Add("Payments:Plans is missing or empty in configuration.");
        }

        var clientBaseUrl = _configuration["ClientBaseUrl"];
        if (string.IsNullOrWhiteSpace(clientBaseUrl) && !isBypass)
        {
            errors.Add("ClientBaseUrl is missing or empty in configuration.");
        }

        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}