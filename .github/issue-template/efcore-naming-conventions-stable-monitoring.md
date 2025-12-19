---
name: 🔔 Monitor EFCore.NamingConventions Stable Release
about: Track EFCore.NamingConventions 10.x stable release
title: "[MONITOR] EFCore.NamingConventions 10.x - Awaiting stable release"
labels: dependencies, monitoring, ef-core
assignees: ''
---

# [MONITOR] EFCore.NamingConventions 10.x - Aguardando release estável

## 📦 Status Atual

**Pacote**: `EFCore.NamingConventions`  
**Versão atual**: `10.0.0-rc.2` (pre-release)  
**Versão estável mais recente**: `9.0.0` (para EF Core 9.x)  
**Versão esperada**: `10.0.0` (stable)  
**Status**: ⏳ **AGUARDANDO RELEASE STABLE**

## 🔗 Links de Monitoramento

- **NuGet**: [EFCore.NamingConventions no NuGet](https://www.nuget.org/packages/EFCore.NamingConventions)
- **GitHub**: [efcore/EFCore.NamingConventions](https://github.com/efcore/EFCore.NamingConventions)
- **Releases**: [Histórico de releases](https://github.com/efcore/EFCore.NamingConventions/releases)

## ⚠️ Situação Atual

### Por Que Estamos Usando RC?

Atualmente usamos **EF Core 10.0.1 (stable)** com **EFCore.NamingConventions 10.0.0-rc.2 (pre-release)**:

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.1" />
<PackageVersion Include="EFCore.NamingConventions" Version="10.0.0-rc.2" />
```

### Riscos

1. **Version Skew**: Biblioteca RC rodando com EF Core stable pode ter comportamento inesperado
2. **Type Load Issues**: Possíveis `TypeLoadException` ou `MissingMethodException` devido a mudanças internas do EF Core
3. **Unsupported**: Misturar RC + stable não é oficialmente suportado

### Por Que Não Downgrade para 9.0.0?

A versão stable `9.0.0` é compatível apenas com EF Core 9.x, não com EF Core 10.x:

```xml
<!-- EFCore.NamingConventions 9.0.0 dependencies -->
<dependency id="Microsoft.EntityFrameworkCore" version="[9.0.0, 10.0.0)" />
```

## ✅ Critérios para Resolver

### Quando Fechar Esta Issue

- [ ] **EFCore.NamingConventions 10.0.0 stable** lançado no NuGet
- [ ] Dependabot cria PR automático para atualização
- [ ] Atualizar `Directory.Packages.props`:
  ```xml
  <PackageVersion Include="EFCore.NamingConventions" Version="10.0.0" />
  ```
- [ ] Regenerar lockfiles: `dotnet restore --force-evaluate`
- [ ] Build e testes passando
- [ ] Atualizar documentação para remover avisos sobre versão RC

## 📋 Monitoramento Automatizado

### Configuração Dependabot

Dependabot está configurado para monitorar automaticamente:

```yaml
# .github/dependabot.yml
- package-ecosystem: "nuget"
  directory: "/"
  schedule:
    interval: "daily"
  # EFCore.NamingConventions NÃO está bloqueado - Dependabot criará PR automaticamente
```

### Como Verificar Manualmente

```bash
# Check latest version on NuGet
dotnet list package --outdated --include-prerelease | grep EFCore.NamingConventions

# Or use NuGet CLI
nuget list EFCore.NamingConventions -PreRelease
```

## 🧪 Status de Testes

### Testes Atuais (RC)

- ✅ Testes unitários passando
- ✅ Testes de integração passando
- ✅ Migrations funcionando
- ⚠️ **Sem testes específicos** para validar compatibilidade RC + stable

### Quando Stable for Lançado

1. Atualizar pacote via Dependabot PR
2. Executar suite completa de testes
3. Validar migrations existentes
4. Testar localmente antes de production

## 📝 Notas Adicionais

### Alternativas Consideradas

1. **Opção 1**: Continuar com RC (atual)
   - ✅ Permite usar EF Core 10.x
   - ❌ Risco de incompatibilidade
   - ✅ Testes passando até agora

2. **Opção 2**: Downgrade para EF Core 9.x
   - ✅ Usa versão stable (9.0.0)
   - ❌ Perde recursos do .NET 10
   - ❌ Adia migração

3. **Opção 3**: Remover EFCore.NamingConventions
   - ❌ Perde snake_case naming conventions
   - ❌ Requer refatoração de todas migrations
   - ❌ Não recomendado

**Decisão**: Manter RC até stable release (Opção 1)

### Histórico de Releases

- **9.0.0** (Nov 2024): Stable para EF Core 9.x
- **10.0.0-rc.2** (Set 2024): RC para EF Core 10.x RC
- **10.0.0** (TBD): Aguardando...

### Related Issues

- #42: Npgsql 10.x stable monitoring (CLOSED - já lançado)
- See: `.github/ISSUE_TEMPLATE/npgsql-10-stable-monitoring.md`

---

**Última verificação**: YYYY-MM-DD (atualizar ao revisar)  
**Próxima verificação**: Automática via Dependabot (diária)
