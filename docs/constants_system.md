# Constantes do MeAjudaAi

Este documento descreve o sistema de constantes centralizadas implementado no projeto MeAjudaAi para melhor organização e manutenção.

## 📁 Estrutura das Constantes

Todas as constantes estão localizadas em `src/Shared/MeAjudai.Shared/Constants/`:

```csharp
Constants/
├── ApiEndpoints.cs          # Endpoints da API por módulo
├── AuthConstants.cs         # Constantes de autorização
├── ValidationConstants.cs   # Limites de validação
└── ValidationMessages.cs    # Mensagens de erro padronizadas
```text
## 🚀 Como Usar

### 1. ApiEndpoints - Endpoints da API

**Para Endpoints Internos (dentro dos módulos):**

```csharp
using MeAjudaAi.Shared.Constants;

public static void Map(IEndpointRouteBuilder app)
    => app.MapGet(ApiEndpoints.Users.GetById, GetUserAsync)
        .WithName("GetUser")
        .RequireAdmin();
```csharp
**Para Clientes HTTP Externos (com Refit):**

```csharp
[Headers("Authorization: Bearer")]
public interface IUsersApi
{
    [Get("/api/v1/users/{id}")]  // Hardcoded para clientes externos
    Task<UserResponse> GetUserAsync(string id);
    
    // OU usando interpolação para melhor manutenção:
    [Get($"/api/v1{ApiEndpoints.Users.GetById}")]
    Task<UserResponse> GetUserAsync(string id);
}
```yaml
### 2. AuthConstants - Autorização

**Policies:**
```csharp
using MeAjudaAi.Shared.Constants;

// Em vez de:
.RequireAuthorization("AdminOnly")

// Use:
.RequireAuthorization(AuthConstants.Policies.AdminOnly)
```csharp
**Claims:**
```csharp
// Em vez de:
var userId = context.User.FindFirst("user_id")?.Value;

// Use:
var userId = context.User.FindFirst(AuthConstants.Claims.UserId)?.Value;
```yaml
**Roles:**
```csharp
// Em vez de:
if (user.IsInRole("admin"))

// Use:
if (user.IsInRole(AuthConstants.Roles.Admin))
```csharp
### 3. ValidationConstants - Limites de Validação

**Em DTOs e Entidades:**
```csharp
using MeAjudaAi.Shared.Constants;

public class User
{
    [StringLength(ValidationConstants.UserLimits.MaxFirstNameLength,
                  MinimumLength = ValidationConstants.UserLimits.MinFirstNameLength)]
    public string FirstName { get; set; }
    
    [StringLength(ValidationConstants.UserLimits.MaxEmailLength)]
    [EmailAddress]
    public string Email { get; set; }
}
```yaml
**Em Validadores FluentValidation:**
```csharp
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.FirstName)
            .Length(ValidationConstants.UserLimits.MinFirstNameLength,
                   ValidationConstants.UserLimits.MaxFirstNameLength)
            .WithMessage(ValidationMessages.Length.FirstNameTooShort);
            
        RuleFor(x => x.Email)
            .MaximumLength(ValidationConstants.UserLimits.MaxEmailLength)
            .Matches(ValidationConstants.Patterns.Email);
    }
}
```csharp
### 4. ValidationMessages - Mensagens de Erro

**Em Validadores:**
```csharp
RuleFor(x => x.Email)
    .NotEmpty()
    .WithMessage(ValidationMessages.Required.Email)
    .EmailAddress()
    .WithMessage(ValidationMessages.InvalidFormat.Email);
```yaml
**Em Handlers de Comando:**
```csharp
if (await userRepository.EmailExistsAsync(command.Email))
{
    return Result.Failure<UserDto>(ValidationMessages.Conflict.EmailAlreadyExists);
}
```sql
## 🎯 Benefícios

### ✅ **Antes (Problemas):**
```csharp
// Endpoints hardcoded espalhados
app.MapGet("/users/{id:guid}", ...)
app.MapPost("/users", ...)

// Autorização inconsistente
.RequireAuthorization("AdminOnly")
.RequireAuthorization("Admin")  // Inconsistente!

// Validações duplicadas
[StringLength(50, MinimumLength = 2)]  // Números mágicos
[StringLength(40, MinimumLength = 3)]  // Inconsistente!

// Mensagens hardcoded
"O email é obrigatório"
"Email é obrigatório"     // Inconsistente!
```csharp
### ✅ **Depois (Soluções):**
```csharp
// Endpoints centralizados
app.MapGet(ApiEndpoints.Users.GetById, ...)
app.MapPost(ApiEndpoints.Users.Create, ...)

// Autorização consistente
.RequireAuthorization(AuthConstants.Policies.AdminOnly)

// Validações consistentes
[StringLength(ValidationConstants.UserLimits.MaxFirstNameLength,
              MinimumLength = ValidationConstants.UserLimits.MinFirstNameLength)]

// Mensagens padronizadas
ValidationMessages.Required.Email
```text
## 📝 Diretrizes de Uso

### ✅ **DO - Faça:**
- Use sempre as constantes em vez de strings/números hardcoded
- Mantenha as constantes organizadas por contexto (Users, Auth, etc.)
- Documente novas constantes com XML comments
- Use nomes descritivos e consistentes

### ❌ **DON'T - Não faça:**
- Não hardcode endpoints, roles, ou limites
- Não misture constantes de diferentes contextos
- Não duplique valores em locais diferentes
- Não esqueça de atualizar tanto a constante quanto o uso

## 🔄 Migração Gradual

Para projetos existentes, migre gradualmente:

1. **Primeiro:** Implemente as constantes
2. **Segundo:** Substitua hardcoded strings nos novos códigos
3. **Terceiro:** Refatore código existente aos poucos
4. **Quarto:** Adicione linting rules para prevenir regressões

## 🚀 Próximos Passos

- [ ] Adicionar constantes para outros módulos conforme necessário
- [ ] Implementar analyzer/linting rules para detectar hardcoded values
- [ ] Criar extensões para facilitar uso das constantes
- [ ] Documentar padrões específicos para cada tipo de constante

## 📚 Exemplos Completos

Veja exemplos completos de uso nas seguintes classes:
- `CreateUserEndpoint.cs` - Uso de ApiEndpoints
- `AuthorizationExtensions.cs` - Uso de AuthConstants
- Validators em `Application/Validators/` - Uso de ValidationConstants
- Exception handlers - Uso de ValidationMessages