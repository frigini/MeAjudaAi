# Adicionando Novos Módulos ao CI/CD

## Como adicionar um novo módulo ao pipeline de testes

Quando criar um novo módulo (ex: Orders, Payments, etc.), siga estes passos para incluí-lo no pipeline de CI/CD:

### 1. Estrutura do Módulo

Certifique-se de que o novo módulo siga a estrutura padrão:

```yaml
src/Modules/{ModuleName}/
├── MeAjudaAi.Modules.{ModuleName}.API/
├── MeAjudaAi.Modules.{ModuleName}.Application/
├── MeAjudaAi.Modules.{ModuleName}.Domain/
├── MeAjudaAi.Modules.{ModuleName}.Infrastructure/
└── MeAjudaAi.Modules.{ModuleName}.Tests/      # ← Testes unitários
```bash
### 2. Atualizar o Workflow de PR

No arquivo `.github/workflows/pr-validation.yml`, adicione o novo módulo na seção `MODULES`:

```bash
MODULES=(
  "Users:src/Modules/Users/MeAjudaAi.Modules.Users.Tests/"
  "Providers:src/Modules/Providers/MeAjudaAi.Modules.Providers.Tests/"
  "Services:src/Modules/Services/MeAjudaAi.Modules.Services.Tests/"  # ← Nova linha
)
```text
### 3. Atualizar o Workflow Aspire (se necessário)

No arquivo `.github/workflows/aspire-ci-cd.yml`, se o módulo tiver testes específicos que precisam ser executados no pipeline de deploy, adicione-os na seção de testes:

```bash
dotnet test src/Modules/{ModuleName}/MeAjudaAi.Modules.{ModuleName}.Tests/ --no-build --configuration Release
```text
### 4. Cobertura de Código

O sistema automaticamente:
- ✅ Coleta cobertura APENAS dos testes unitários do módulo
- ✅ Inclui apenas as classes do módulo no relatório (`[MeAjudaAi.Modules.{ModuleName}.*]*`)
- ✅ Exclui classes de teste e assemblies de teste
- ✅ Gera relatórios separados por módulo

### 5. Testes que NÃO geram cobertura

Estes tipos de teste são executados mas NÃO contribuem para o relatório de cobertura:
- `tests/MeAjudaAi.Architecture.Tests/` - Testes de arquitetura
- `tests/MeAjudaAi.Integration.Tests/` - Testes de integração
- `tests/MeAjudaAi.Shared.Tests/` - Testes do shared
- `tests/MeAjudaAi.E2E.Tests/` - Testes end-to-end

### 6. Validação

Após adicionar um novo módulo:
1. Verifique se o pipeline executa sem erros
2. Confirme que o relatório de cobertura inclui o novo módulo
3. Verifique se não há DLLs duplicadas no relatório

## Exemplo Completo

Para adicionar o módulo "Orders":

1. **Estrutura criada:**
   ```
   src/Modules/Orders/
   ├── MeAjudaAi.Modules.Orders.API/
   ├── MeAjudaAi.Modules.Orders.Application/
   ├── MeAjudaAi.Modules.Orders.Domain/
   ├── MeAjudaAi.Modules.Orders.Infrastructure/
   └── MeAjudaAi.Modules.Orders.Tests/
   ```

2. **Atualização no workflow:**
   ```bash
   MODULES=(
     "Users:src/Modules/Users/MeAjudaAi.Modules.Users.Tests/"
     "Orders:src/Modules/Orders/MeAjudaAi.Modules.Orders.Tests/"  # ← Nova linha
   )
   ```

3. **Resultado esperado:**
   - Testes unitários do Orders executados ✅
   - Cobertura coletada apenas para classes Orders ✅
   - Relatório separado gerado ✅
   - Sem DLLs duplicadas ✅