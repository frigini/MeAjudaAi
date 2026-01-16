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
            var fieldIdentifier = ResolveFieldIdentifier(CurrentEditContext.Model, error.PropertyName);
            _validationMessageStore?.Add(fieldIdentifier, error.ErrorMessage);
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        if (_validator == null || CurrentEditContext?.Model is not TModel model)
            return;

        _validationMessageStore?.Clear(e.FieldIdentifier);

        var validationResult = _validator.Validate(model);
        
        // Match errors for this field, handling nested properties
        var errors = validationResult.Errors.Where(error =>
        {
            var errorFieldIdentifier = ResolveFieldIdentifier(CurrentEditContext.Model, error.PropertyName);
            return errorFieldIdentifier.Equals(e.FieldIdentifier);
        });

        foreach (var error in errors)
        {
            _validationMessageStore?.Add(e.FieldIdentifier, error.ErrorMessage);
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Resolve dotted property paths (e.g., "BusinessProfile.LegalName") to proper FieldIdentifier.
    /// </summary>
    private FieldIdentifier ResolveFieldIdentifier(object model, string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath) || !propertyPath.Contains('.'))
        {
            return new FieldIdentifier(model, propertyPath);
        }

        var segments = propertyPath.Split('.');
        var current = model;

        // Walk the property path to get the leaf owner object
        for (var i = 0; i < segments.Length - 1; i++)
        {
            var propertyInfo = current.GetType().GetProperty(segments[i]);
            if (propertyInfo == null)
            {
                // Fallback to root model if property not found
                return new FieldIdentifier(model, propertyPath);
            }

            var value = propertyInfo.GetValue(current);
            if (value == null)
            {
                // Fallback to root model if intermediate value is null
                return new FieldIdentifier(model, propertyPath);
            }

            current = value;
        }

        // Return FieldIdentifier with the leaf owner and simple property name
        var leafPropertyName = segments[^1];
        return new FieldIdentifier(current, leafPropertyName);
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
        return EventCallback.Factory.Create<string>(receiver ?? new object(), (value) =>
        {
            setter(Sanitize(value));
        });
    }
}
