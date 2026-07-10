# Architecture Tests

## Overview

Testes de arquitetura (fitness functions) que verificam guardrails e convenções do projeto.

## Project Structure

```text
tests/MeAjudaAi.Architecture.Tests/
├── Authorization/                  # Regras de autorização
├── Contracts/                      # Regras de contratos
├── Conventions/                    # Convenções de nomenclatura
├── CrossCutting/                   # Regras transversais
├── Database/                       # Regras de banco de dados
├── Dependencies/                   # Regras de dependências
├── Helpers/                        # Helpers para testes de arquitetura
├── MeAjudaAi.Architecture.Tests.csproj
└── README.md
```

## Responsibilities

**O que testar:**
- Guardrails de arquitetura (dependências entre camadas)
- Regras de nomenclatura
- Convenções de projeto
- Limites de dependências

**O que NÃO testar:**
- Lógica de negócio
- Integração com infraestrutura

## Writing Tests

### Libraries

- **NetArchTest.Rules**: Framework para verificação de regras de arquitetura

### Example

```csharp
public class DomainLayerTests
{
    [Fact]
    public void Domain_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(typeof(User).Assembly)
            .ShouldNot()
            .HaveDependencyOn("MeAjudaAi.Modules.Users.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
```

## Running Tests

```bash
dotnet test tests/MeAjudaAi.Architecture.Tests
```

## Dependencies

- `MeAjudaAi.Shared` (para tipos base)
- Todos os módulos (para verificar dependências)
