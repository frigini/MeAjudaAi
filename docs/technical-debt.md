# Débito Técnico e Rastreamento de Melhorias

Este documento rastreia itens de débito técnico e melhorias planejadas identificadas durante o desenvolvimento que devem ser convertidas em issues do GitHub.

## Melhorias nos Testes de Integração

### Melhoria do Teste de Status de Verificação de Prestador
**Arquivo**: `tests/MeAjudaAi.Integration.Tests/Providers/ProvidersIntegrationTests.cs`  
**Linha**: ~180  
**Situação**: Aguardando Criação de Issue  

**Descrição**: 
O teste `GetProvidersByVerificationStatus_ShouldReturnOnlyPendingProviders` atualmente apenas valida a estrutura da resposta devido à falta de endpoints de gerenciamento de status de verificação.

**Melhoria Necessária**:
- Implementar endpoints de gerenciamento de status de verificação de prestadores (aprovar/rejeitar/atualizar verificação)
- Criar prestadores de teste com diferentes status de verificação
- Melhorar o teste para verificar o comportamento real de filtragem (apenas prestadores com status Pending retornados)
- Adicionar testes similares para outros status de verificação (Approved, Rejected, etc.)

**Prioridade**: Média  
**Dependências**: Endpoints de API para gerenciamento de status de verificação de prestadores  

**Critérios de Aceitação**:
- [ ] Endpoints de gerenciamento de status de verificação de prestadores disponíveis
- [ ] Teste pode criar prestadores com diferentes status de verificação
- [ ] Teste verifica que a filtragem retorna apenas prestadores com o status especificado
- [ ] Teste inclui limpeza dos dados de teste criados
- [ ] Testes similares adicionados para todos os valores de status de verificação

---

## Instruções para Mantenedores

1. **Conversão para Issues do GitHub**: 
   - Copiar a descrição da melhoria para um novo issue do GitHub
   - Adicionar labels apropriadas (`technical-debt`, `testing`, `enhancement`)
   - Vincular ao arquivo específico e número da linha
   - Adicionar ao backlog do projeto com prioridade apropriada

2. **Atualizando este Documento**:
   - Marcar itens como "Issue Criado" com número do issue quando convertido
   - Remover itens completos ou mover para seção "Concluído"
   - Adicionar novos itens de débito técnico conforme identificados

3. **Referências de Código**:
   - Usar tag `[ISSUE]` em comentários TODO para indicar itens rastreados aqui
   - Incluir caminho do arquivo e números de linha para navegação fácil
   - Manter descrições específicas e acionáveis