# Problema Conhecido: Aspire com Pacotes NuGet Locais

## Descrição do Problema

Ao executar `.\scripts\dev.ps1` ou `dotnet run` no AppHost, pode ocorrer o seguinte erro:

```
System.AggregateException: One or more errors occurred. 
  (Property CliPath: The path to the DCP executable used for Aspire orchestration is required.; 
   Property DashboardPath: The path to the Aspire Dashboard binaries is missing.)
```

## Causa Raiz

Este é um bug conhecido no .NET Aspire 13.x quando os pacotes NuGet são armazenados em um diretório customizado (usando `globalPackagesFolder` no nuget.config).

- O MSBuild corretamente define as propriedades `AspireDashboardPath` e `DcpCliPath`
- Mas o código runtime do Aspire espera `DashboardPath` e `CliPath` (sem prefixo "Aspire"/"Dcp")
- Issue rastreada em: https://github.com/dotnet/aspire/issues/6789

## Soluções Alternativas

### ✅ Opção 1: Executar via VS Code (Recomendado)

1. Abra o projeto no VS Code
2. Pressione `F5` ou vá em **Run > Start Debugging**
3. Selecione `.NET Aspire: MeAjudaAi.AppHost` como perfil de execução

O VS Code com C# Dev Kit configura corretamente os caminhos necessários.

### ✅ Opção 2: Executar via Visual Studio 2022

1. Abra `MeAjudaAi.slnx` no Visual Studio
2. Defina `MeAjudaAi.AppHost` como projeto de inicialização
3. Pressione `F5` ou **Debug > Start Debugging**

### ⚠️  Opção 3: Configuração Manual (Avançado)

Defina as variáveis de ambiente antes de executar:

```powershell
$env:DCP_CLI_PATH = "C:\Code\MeAjudaAi\packages\aspire.hosting.orchestration.win-x64\13.1.0\tools\dcp.exe"
$env:ASPIRE_DASHBOARD_PATH = "C:\Code\MeAjudaAi\packages\aspire.dashboard.sdk.win-x64\13.1.0\tools"
$env:POSTGRES_PASSWORD = "postgres"

cd src\Aspire\MeAjudaAi.AppHost
dotnet run
```

**Nota**: Esta opção pode não funcionar devido à forma como o Aspire valida as propriedades.

## Status

- ✅ Workaround documentado
- ⏳ Aguardando correção upstream no .NET Aspire ou migração para pacotes globais
- 🔄 Funciona perfeitamente via VS Code/Visual Studio

## Alternativa: Desabilitar globalPackagesFolder

Se necessário executar via CLI, você pode temporariamente desabilitar o `globalPackagesFolder` no `nuget.config`:

```xml
<config>
  <!-- <add key="globalPackagesFolder" value="packages" /> -->
  <!-- <add key="repositoryPath" value="packages" /> -->
</config>
```

Depois, execute:
```powershell
dotnet restore
.\scripts\dev.ps1
```

**Atenção**: Isso fará o restore baixar os pacotes para `%USERPROFILE%\.nuget\packages` (~5GB).
