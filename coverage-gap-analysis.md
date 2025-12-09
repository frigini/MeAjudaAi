# Coverage Analysis - Gaps to Address

## Priority 1: Configuration Issues (já corrigido)
- ✅ Program.cs ainda aparecendo no relatório → Adicionado `**/*Program.cs` ao coverage.runsettings
- ✅ DbContextFactory classes → Adicionado `**/Persistence/**/*DbContextFactory.cs` ao coverage.runsettings

## Priority 2: Module DTOs com 0% Coverage

### SearchProviders Module
- `ModulePagedSearchResultDto` - 0% (3 lines, 15 branches)
  - Usado em: SearchProvidersModuleApi
  - Referência: LocationsModuleApiTests (já testado)

### ServiceCatalogs Module  
- `ModuleServiceCategoryDto` - 0% (7 lines, 13 branches)
- `ModuleServiceDto` - 0% (8 lines, 25 branches)
- `ModuleServiceListDto` - 0% (6 lines, 35 branches)
- ✅ `ModuleServiceValidationResultDto` - 100% (5 lines, 44 branches) - JÁ COBERTO
  - Usado em: ServiceCatalogsModuleApi
  - Referência: LocationsModuleApiTests

### Users Module
- `ModuleUserBasicDto` - 0% (6 lines, 12 branches)
- `ModuleUserDto` - 0% (aparece truncado na imagem)
  - Usado em: UsersModuleApi
  - Referência: LocationsModuleApiTests

**Ação**: Verificar se os testes de ModuleApi estão realmente exercitando os DTOs. Se não, adicionar testes específicos.

## Priority 3: Gaps nos Módulos (de acordo com imagem)

### Documents Module - 82.8% coverage
- 17 linhas não cobertas de 99 linhas
- 267 branches (necessita análise detalhada)
- DocumentsDbContextFactory - 0% (já será excluído)

### Próximos passos:
1. ✅ Rodar cobertura local após fix do runsettings
2. Verificar se ModuleApi tests estão exercitando os DTOs
3. Se não, adicionar testes específicos para cada DTO
4. Analisar gaps restantes nos módulos após exclusões

## Nota sobre Records
Os DTOs são `record` types que auto-geram:
- Constructor
- Deconstruct
- ToString
- Equals/GetHashCode
- Property getters

Para 100% de cobertura, os testes precisam:
- Criar instâncias (testa constructor)
- Usar em asserções (testa getters e Equals)
- Pattern matching/deconstruction (se aplicável)
