using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MeAjudaAi.Web.Admin.Extensions;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Validator de FluentValidation para integração com EditForm do Blazor.
/// Permite usar validadores FluentValidation em componentes Blazor.
/// </summary>
/// <typeparam name="TModel">Tipo do modelo a ser validado</typeparam>
public class FluentValidationValidator<TModel> : ComponentBase, IDisposable
{
    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;
    
    [CascadingParameter] private EditContext? CurrentEditContext { get; set; }

    private ValidationMessageStore? _validationMessageStore;
    private IValidator<TModel>? _validator;

    protected override void OnInitialized()
    {
        if (CurrentEditContext == null)
        {
            throw new InvalidOperationException(
                $"{nameof(FluentValidationValidator<TModel>)} requires a cascading parameter " +
                $"of type {nameof(EditContext)}. For example, you can use {nameof(FluentValidationValidator<TModel>)} " +
                $"inside an {nameof(EditForm)}.");
        }

        _validationMessageStore = new ValidationMessageStore(CurrentEditContext);
        _validator = ServiceProvider.GetService<IValidator<TModel>>();

        CurrentEditContext.OnValidationRequested += OnValidationRequested;
        CurrentEditContext.OnFieldChanged += OnFieldChanged;
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        _validationMessageStore?.Clear();

        if (_validator == null || CurrentEditContext?.Model is not TModel model)
            return;

        var validationResult = _validator.Validate(model);

        foreach (var error in validationResult.Errors)
        {
            var fieldIdentifier = new FieldIdentifier(CurrentEditContext.Model, error.PropertyName);
            _validationMessageStore?.Add(fieldIdentifier, error.ErrorMessage);
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        if (_validator == null || CurrentEditContext?.Model is not TModel model)
            return;

        var propertyName = e.FieldIdentifier.FieldName;
        _validationMessageStore?.Clear(e.FieldIdentifier);

        var validationResult = _validator.Validate(model);
        var errors = validationResult.Errors.Where(x => x.PropertyName == propertyName);

        foreach (var error in errors)
        {
            _validationMessageStore?.Add(e.FieldIdentifier, error.ErrorMessage);
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    public void Dispose()
    {
        if (CurrentEditContext != null)
        {
            CurrentEditContext.OnValidationRequested -= OnValidationRequested;
            CurrentEditContext.OnFieldChanged -= OnFieldChanged;
        }
    }
}

/// <summary>
/// Helper para sanitizar inputs de formulários.
/// </summary>
public static class InputSanitizer
{
    /// <summary>
    /// Sanitiza string para prevenir XSS.
    /// </summary>
    public static string Sanitize(string? input)
    {
        return ValidationExtensions.SanitizeInput(input);
    }

    /// <summary>
    /// Cria um callback para sanitizar automaticamente ao digitar.
    /// </summary>
    public static EventCallback<string> CreateSanitizedCallback(object? receiver, Action<string> setter)
    {
        return EventCallback.Factory.Create<string>(receiver, (value) =>
        {
            setter(Sanitize(value));
        });
    }
}
