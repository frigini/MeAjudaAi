# Authorization System Refactoring

## 🎯 Objetivo

Eliminar redundância no sistema de autorização e consolidar em uma estratégia única, type-safe e consistente.

## 📊 Problema Identificado

### **Redundância Estratégica:**
- **Sistema Legado:** Políticas baseadas em strings (`"AdminOnly"`, `"SelfOrAdmin"`, etc.)
- **Sistema Novo:** Permissões type-safe `EPermissions` + extension methods
- **Enum EPolicies:** Tentativa de type-safety para políticas legadas (não usada)
- **Duplicação:** Ambos os sistemas ativos simultaneamente

### **Problemas:**
- ❌ **Inconsistência:** Alguns endpoints usavam políticas legadas, outros permissões
- ❌ **Manutenção:** Dois sistemas para manter
- ❌ **Confusão:** Desenvolvedores não sabiam qual usar
- ❌ **Performance:** Registrava políticas desnecessárias

## ✅ Solução Implementada

### **1. Sistema Unificado**
- **Primário:** `EPermissions` com extension methods `RequirePermission()`
- **Secundário:** Políticas essenciais (`SelfOrAdmin`, `AdminOnly`, `SuperAdminOnly`) para casos específicos

### **2. Extension Methods Type-Safe**
```csharp
// Permissões específicas (preferido)
.RequirePermission(EPermissions.UserRead)
.RequirePermissions(EPermissions.UserRead, EPermissions.UserWrite)

// Políticas convenientes para casos comuns
.RequireSelfOrAdmin()   // Para endpoints de usuário
.RequireAdmin()         // Para operações administrativas
.RequireSuperAdmin()    // Para operações críticas
```

### **3. Arquitetura Limpa**
```csharp
// SecurityExtensions.cs - Políticas especiais apenas
services.AddAuthorizationBuilder()
    .AddPolicy("SelfOrAdmin", policy => policy.AddRequirements(new SelfOrAdminRequirement()))
    .AddPolicy("AdminOnly", policy => policy.RequireRole("admin", "super-admin"))
    .AddPolicy("SuperAdminOnly", policy => policy.RequireRole("super-admin"));

// AuthorizationExtensions.cs - Sistema principal de permissões
foreach (EPermissions permission in Enum.GetValues<EPermissions>())
{
    var policyName = $"RequirePermission:{permission.GetValue()}";
    options.AddPolicy(policyName, policy => policy.Requirements.Add(new PermissionRequirement(permission)));
}
```

## 🗂️ Arquivos Modificados

### **Atualizados:**
- `SecurityExtensions.cs` - Removidas políticas legadas redundantes
- `AuthorizationExtensions.cs` - Adicionados extension methods convenientes
- **Endpoints atualizados:**
  - `GetUserByIdEndpoint.cs` - `RequireAuthorization("SelfOrAdmin")` → `RequireSelfOrAdmin()`
  - `GetUserByEmailEndpoint.cs` - `RequireAuthorization("AdminOnly")` → `RequireAdmin()`
  - `DeleteUserEndpoint.cs` - `RequireAuthorization("AdminOnly")` → `RequireAdmin()`
  - `CreateUserEndpoint.cs` - `RequireAuthorization("AdminOnly")` → `RequireAdmin()`
  - `UpdateUserProfileEndpoint.cs` - `RequireAuthorization("SelfOrAdmin")` → `RequireSelfOrAdmin()`

### **Removidos:**
- `EPolicies.cs` - Enum não utilizada
- `PoliciesExtensions.cs` - Extensions não utilizadas

## 📈 Benefícios Alcançados

### **✅ Consistência**
- Todos os endpoints usam a mesma abordagem
- API uniforme para autorização

### **✅ Type-Safety**
- IntelliSense para permissões
- Erros em tempo de compilação

### **✅ Manutenibilidade**
- Sistema único para manter
- Mais fácil adicionar novas permissões

### **✅ Performance**
- Menos políticas registradas
- Resolução mais eficiente

### **✅ Developer Experience**
- API intuitiva e descobrível
- Documentação clara através do código

## 🔄 Estratégia de Migração

### **Fase 1: ✅ Concluída**
- Consolidação do sistema de autorização
- Remoção de redundâncias
- Criação de extension methods

### **Fase 2: 🔄 Futura**
- Migração completa para permissões granulares
- Possível depreciação de políticas baseadas em roles
- Integração com sistema de permissions do Keycloak

## 💡 Diretrizes para Novos Endpoints

### **Preferência de Uso:**
1. **Primeiro:** `RequirePermission(EPermissions.SpecificPermission)`
2. **Segundo:** Extension methods convenientes (`RequireSelfOrAdmin()`, `RequireAdmin()`)
3. **Último:** `RequireAuthorization("PolicyName")` - apenas para casos especiais

### **Exemplos:**
```csharp
// ✅ Preferido - Permissão específica
.RequirePermission(EPermissions.UserRead)

// ✅ Aceitável - Casos comuns
.RequireSelfOrAdmin()

// ❌ Evitar - Strings mágicas
.RequireAuthorization("AdminOnly")
```

## 🧪 Validação

- ✅ 0 erros de compilação
- ✅ 0 warnings
- ✅ Todos os endpoints funcionais
- ✅ Type-safety mantida
- ✅ Backwards compatibility preservada

## 📝 Notas Técnicas

- **SelfOrAdminHandler** mantido para lógica específica de autorização de usuário
- Políticas essenciais mantidas em `SecurityExtensions` por necessitarem de handlers específicos
- Sistema de permissões principal em `AuthorizationExtensions` para reutilização
- Extension methods facilitam descoberta via IntelliSense