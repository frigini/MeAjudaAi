# Documentação de Autenticação e Autorização

## 📋 Visão Geral

Esta pasta contém documentação completa sobre os sistemas de autenticação e autorização do MeAjudaAi, incluindo o sistema type-safe baseado em `EPermissions`.

## 📚 Conteúdo

### Documentação Principal
- **[Sistema de Autenticação](../authentication.md)** - Documentação principal do sistema de autenticação e autorização
- **[Guia de Implementação](./authorization_system_implementation.md)** - Guia completo para implementar autorização type-safe
- **[Sistema de Permissões Type-Safe](./type_safe_permissions_system.md)** - Detalhes do sistema baseado em EPermissions
- **[Resolução Server-Side](../server_side_permissions.md)** - Guia para resolução de permissões no servidor

### Testes e Desenvolvimento
- **[Test Authentication Handler](../testing/test_authentication_handler.md)** - Handler configurável para testes

## 🏗️ Arquitetura do Sistema

### Sistema de Autenticação
- ✅ **Configurável** - Suporte a múltiplos provedores
- ✅ **Testável** - Handler específico para testes
- ✅ **Middleware** - Processamento e validação de requests

### Sistema de Autorização Type-Safe
- ✅ **EPermissions Enum** - Sistema unificado type-safe
- ✅ **Modular** - Cada módulo implementa `IModulePermissionResolver`
- ✅ **Performance** - Cache distribuído com HybridCache
- ✅ **Extensível** - Suporte para múltiplos provedores
- ✅ **Monitoramento** - Métricas integradas para observabilidade

### Componentes Principais

1. **IPermissionService** - Interface principal para resolução de permissões
2. **IModulePermissionResolver** - Resolução modular de permissões
3. **EPermissions** - Enum type-safe com todas as permissões do sistema
4. **Permission Cache** - Sistema de cache distribuído para performance
5. **Middleware de Autorização** - Middleware para validação automática

## 🚀 Configuração Rápida

### 1. Configuração Básica
```csharp
// Program.cs
builder.Services.AddPermissionBasedAuthorization(builder.Configuration);
builder.Services.AddModulePermissionResolver<UsersPermissionResolver>();

app.UsePermissionBasedAuthorization();
```
### 2. Uso em Endpoints
```csharp
group.MapGet("/", GetUsers)
     .RequirePermission(EPermission.UsersRead);

group.MapPost("/", CreateUser)
     .RequirePermission(EPermission.UsersCreate);
```
### 3. Verificação Programática
```csharp
var hasPermission = await permissionService
    .HasPermissionAsync(userId, EPermission.UsersRead);
```
## 🔧 Ambientes

### Desenvolvimento
- Autenticação simplificada para desenvolvimento local
- Cache em memória para rapidez
- Logs detalhados para debugging

### Testes
- Handler de autenticação configurável
- Permissões mocadas para cenários específicos
- Integração com test containers

### Produção
- Autenticação completa com provedores externos
- Cache distribuído (Redis/SQL Server)
- Métricas e monitoramento completos

## 📖 Guias de Uso

### Para Desenvolvedores
1. Leia a [documentação principal](../authentication.md)
2. Siga o [guia de implementação](./authorization_system_implementation.md)
3. Implemente seu `IModulePermissionResolver`
4. Use `.RequirePermission()` nos endpoints

### Para Testes
1. Configure o [Test Authentication Handler](../testing/test_authentication_handler.md)
2. Use permissões mocadas nos testes
3. Valide cenários com e sem permissão

### Para DevOps
1. Configure cache distribuído
2. Monitore métricas em `/metrics`
3. Configure alertas para falhas de autorização

## 📊 Métricas e Monitoramento

O sistema expõe automaticamente:
- ⏱️ Tempo de resolução de permissões
- 📊 Taxa de acerto do cache
- ❌ Falhas de autorização
- 📈 Performance por módulo

## 🔗 Documentação Relacionada

- [Guias de Desenvolvimento](../development.md)
- [Arquitetura do Sistema](../architecture.md)
- [Guia de Testes](../testing/)
- [Configuração CI/CD](../ci_cd.md)