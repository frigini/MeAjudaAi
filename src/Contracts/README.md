# MeAjudaAi.Shared.Contracts

Biblioteca portável de contratos compartilhados entre backend (ASP.NET Core) e frontend (Blazor WebAssembly).

## 📦 Propósito

Este projeto contém **apenas tipos portáteis** que podem ser usados tanto no servidor quanto no navegador:

- **DTOs** (Data Transfer Objects) - Modelos de dados para comunicação entre módulos
- **Result Pattern** - Tipo funcional para tratamento de erros
- **Interfaces de Módulos** - Contratos públicos (`IModuleApi`)
- **Enums e Value Objects** - Tipos de domínio sem dependências

## 🚫 O que NÃO incluir

- ❌ `Microsoft.AspNetCore.App` framework reference
- ❌ Entity Framework Core
- ❌ Dapper ou ADO.NET
- ❌ Azure SDKs com dependências nativas
- ❌ Qualquer código que dependa de servidor HTTP

**Documentação:** Ver [docs/architecture.md](../../../../docs/architecture.md) para detalhes sobre modular monolith e result pattern.

## ✅ O que pode incluir

- ✅ FluentValidation (validadores compartilhados)
- ✅ System.Text.Json (serialização)
- ✅ DataAnnotations básicas
- ✅ Records e classes POCO

## 🎯 Uso

### Backend (MeAjudaAi.Shared)
```csharp
using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Shared.Contracts.Functional;

// Usar DTOs para comunicação entre módulos
public async Task<Result<ModuleProviderDto>> GetProviderAsync(Guid id)
{
    // ...
}
```

### Frontend (MeAjudaAi.Client.Contracts → Blazor WASM)
```csharp
using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Client.Contracts.Api;

// Refit interface usa os mesmos DTOs
[Get("/api/v1/providers/{id}")]
Task<Result<ModuleProviderDto?>> GetProviderByIdAsync(Guid id);
```

## 📂 Estrutura

```text
MeAjudaAi.Shared.Contracts/
├── Functional/              # Result pattern, Unit, Error
├── Modules/                 # Contratos públicos dos módulos
│   ├── IModuleApi.cs       # Interface base
│   ├── Providers/
│   │   ├── DTOs/           # ModuleProviderDto, ModuleProviderBasicDto, etc.
│   │   └── IProvidersModuleApi.cs
│   ├── Documents/
│   ├── Locations/
│   ├── SearchProviders/
│   ├── ServiceCatalogs/
│   └── Users/
```

## 🔗 Dependências

- **MeAjudaAi.Shared** (backend) → referencia este projeto
- **MeAjudaAi.Client.Contracts** (frontend) → referencia este projeto
- **MeAjudaAi.Web.Admin** (Blazor WASM) → referencia Client.Contracts

## 📝 Convenções

1. **Namespace**: `MeAjudaAi.Shared.Contracts.*`
2. **DTOs**: Sufixo `Dto`, sealed records, XML comments obrigatórios
3. **Module APIs**: Prefixo `I`, sufixo `ModuleApi`, herdam de `IModuleApi`
4. **Result**: Sempre retornar `Result<T>` ou `Result` (Unit)

## 🧪 Testes

Este projeto é testado indiretamente por:
- Testes de integração no backend
- Testes de componentes no frontend (bUnit)
- Testes E2E (Playwright)

## 📚 Referências

- [Result Pattern no C#](../../../docs/architecture.md#result-pattern)
- [Modular Monolith Architecture](../../../docs/architecture.md#modular-monolith)
- [Blazor WASM Setup](../../../docs/modules/admin-portal.md)
