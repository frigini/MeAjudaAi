# FluentValidation Integration Guide

## Overview
FluentValidation has been successfully integrated into the MeAjudaAi.Web.Admin project with Brazilian-specific validation rules and XSS protection.

## Components Added

### 1. Validators
- **CreateProviderRequestDtoValidator**: Validates provider creation with CPF/CNPJ, email, phone
- **UpdateProviderRequestDtoValidator**: Validates provider updates (all fields optional)
- **UploadDocumentDtoValidator**: Validates file uploads (type, size, security)

### 2. ValidationExtensions.cs
Reusable validation helpers:
- `ValidCpf()` - CPF validation with checksum
- `ValidCnpj()` - CNPJ validation with checksum
- `ValidCpfOrCnpj()` - Combined document validation
- `ValidBrazilianPhone()` - Phone format validation (10-11 digits)
- `ValidEmail()` - Email validation
- `ValidCep()` - ZIP code validation
- `NoXss()` - XSS prevention
- `SanitizeInput()` - Client-side text sanitization
- `ValidFileType()` - File extension validation
- `MaxFileSize()` - File size validation

### 3. FluentValidator Component
Razor component for integrating FluentValidation with Blazor EditForm.

## Usage Examples

### Example 1: Using with MudForm (Current Pattern)

For MudForm, you can use `Validation` parameter with Func<T, IEnumerable<string>>:

```razor
@using FluentValidation
@inject IValidator<CreateProviderRequestDto> Validator

<MudForm Model="@model" Validation="@(ValidateField)">
    <MudTextField @bind-Value="model.Name" 
                  For="@(() => model.Name)"
                  Label="Nome" 
                  Required="true" />
    
    <MudTextField @bind-Value="model.Document" 
                  For="@(() => model.Document)"
                  Label="CPF/CNPJ" />
</MudForm>

@code {
    private CreateProviderRequestDto model = new(...);
    
    private IEnumerable<string> ValidateField(object fieldValue)
    {
        var result = Validator.Validate(model);
        return result.Errors.Select(e => e.ErrorMessage);
    }
}
```

### Example 2: Using with EditForm + FluentValidator

```razor
@using Microsoft.AspNetCore.Components.Forms

<EditForm Model="@model" OnValidSubmit="HandleSubmit">
    <FluentValidator TModel="CreateProviderRequestDto" />
    <DataAnnotationsValidator />
    
    <InputText @bind-Value="model.Name" class="form-control" />
    <ValidationMessage For="@(() => model.Name)" />
    
    <button type="submit">Submit</button>
</EditForm>
```

### Example 3: Manual Validation in Code

```csharp
@inject IValidator<CreateProviderRequestDto> Validator

private async Task Submit()
{
    var validationResult = await Validator.ValidateAsync(model);
    
    if (!validationResult.IsValid)
    {
        foreach (var error in validationResult.Errors)
        {
            Snackbar.Add(error.ErrorMessage, Severity.Error);
        }
        return;
    }
    
    // Proceed with API call
}
```

### Example 4: Field-Level Validation

```csharp
@code {
    private string cpf = "";
    
    private async Task ValidateCpf()
    {
        if (ValidationExtensions.IsValidCpf(cpf))
        {
            Snackbar.Add("CPF válido!", Severity.Success);
        }
        else
        {
            Snackbar.Add("CPF inválido", Severity.Error);
        }
    }
}
```

### Example 5: XSS Sanitization

```razor
@using MeAjudaAi.Web.Admin.Extensions

<MudTextField @bind-Value="name" 
              Label="Nome"
              OnBlur="SanitizeName" />

@code {
    private string name = "";
    
    private void SanitizeName()
    {
        name = ValidationExtensions.SanitizeInput(name);
    }
}
```

## Integration with Existing Forms

### CreateProviderDialog.razor

To add validation to CreateProviderDialog, replace the model class with the DTO and add validator:

```razor
@using FluentValidation
@inject IValidator<CreateProviderRequestDto> Validator

<MudForm @ref="form" Model="@request" Validation="@(new Func<object, IEnumerable<string>>(ValidateValue))">
    <!-- form fields -->
</MudForm>

@code {
    private CreateProviderRequestDto request = new(...);
    
    private IEnumerable<string> ValidateValue(object value)
    {
        var result = Validator.Validate(request);
        
        if (value is string fieldName)
        {
            return result.Errors
                .Where(e => e.PropertyName == fieldName)
                .Select(e => e.ErrorMessage);
        }
        
        return result.Errors.Select(e => e.ErrorMessage);
    }
}
```

### UploadDocumentDialog.razor

```razor
@using FluentValidation
@inject IValidator<UploadDocumentDto> Validator

@code {
    private async Task Submit()
    {
        var uploadDto = new UploadDocumentDto
        {
            ProviderId = ProviderId,
            File = selectedFile,
            DocumentType = documentType
        };
        
        var validationResult = await Validator.ValidateAsync(uploadDto);
        
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                Snackbar.Add(error.ErrorMessage, Severity.Error);
            }
            return;
        }
        
        // Proceed with upload
    }
}
```

## Real-time Validation with MudBlazor

MudBlazor supports real-time validation through the `For` parameter:

```razor
<MudTextField @bind-Value="model.Email" 
              For="@(() => model.Email)"
              Label="Email"
              Validation="@(new Func<string, IEnumerable<string>>(ValidateEmail))" />

@code {
    private IEnumerable<string> ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return new[] { "Email é obrigatório" };
            
        if (!ValidationExtensions.ValidEmail(email))
            return new[] { "Email inválido" };
            
        return Array.Empty<string>();
    }
}
```

## Best Practices

1. **Always sanitize user input** on blur or submit:
   ```csharp
   model.Name = ValidationExtensions.SanitizeInput(model.Name);
   ```

2. **Validate before API calls**:
   ```csharp
   var result = await Validator.ValidateAsync(model);
   if (!result.IsValid) return;
   ```

3. **Show specific error messages**:
   ```csharp
   foreach (var error in validationResult.Errors)
   {
       Snackbar.Add($"{error.PropertyName}: {error.ErrorMessage}", Severity.Error);
   }
   ```

4. **Use typed validators** for better IntelliSense:
   ```csharp
   @inject IValidator<CreateProviderRequestDto> CreateValidator
   @inject IValidator<UpdateProviderRequestDto> UpdateValidator
   ```

## Validation Rules Summary

### CPF/CNPJ
- Format: 000.000.000-00 or 00.000.000/0000-00
- Validates checksum digits
- Rejects sequential numbers (111.111.111-11)

### Phone
- Format: (00) 00000-0000 or (00) 0000-0000
- 10 digits (landline) or 11 digits (mobile)
- Mobile must start with 9 after area code

### Email
- RFC 5322 compliant
- Max 100 characters

### CEP
- Format: 00000-000
- 8 digits required

### File Upload
- Max size: 10 MB
- Allowed types: PDF, JPG, JPEG, PNG
- Content type validation

### XSS Protection
- Removes HTML tags
- Blocks javascript: and data: URIs
- Removes event handlers (onclick, etc.)
- Blocks <script>, <iframe>, <embed>, <object>

## Testing Validators

```csharp
[Fact]
public void ValidCpf_ShouldReturnTrue_ForValidCpf()
{
    // Arrange
    var cpf = "123.456.789-09";
    
    // Act
    var result = ValidationExtensions.IsValidCpf(cpf);
    
    // Assert
    Assert.True(result);
}
```

## Next Steps

1. Update all existing forms to use FluentValidation
2. Add unit tests for validators
3. Document validation rules in user-facing help text
4. Consider adding async validators for database checks (e.g., unique email)
5. Add localization for error messages (PT-BR)
