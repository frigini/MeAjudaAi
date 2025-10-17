# GitHub Actions Workflow Fixes

## Problemas Resolvidos

### 1. Erro de Sintaxe Bash: `{1..30}` Loop

**Problema:**
```bash
# ❌ ERRO: Sintaxe não-POSIX
for i in {1..30}; do
    # código aqui
done
```csharp
**Solução:**
```bash
# ✅ CORRETO: Sintaxe POSIX-compliant
counter=1
max_attempts=30
while [ $counter -le $max_attempts ]; do
    # código aqui
    counter=$((counter + 1))
done
```text
**Arquivos Corrigidos:**
- `.github/workflows/pr-validation.yml`
- `.github/workflows/aspire-ci-cd.yml`

### 2. Problemas de Interpolação de Secrets

**Problema:**
```bash
# ❌ ERRO: Interpolação direta causa problemas de escaping
export PGPASSWORD="${{ secrets.POSTGRES_PASSWORD }}"
```csharp
**Solução:**
```yaml
# ✅ CORRETO: Usar variáveis de ambiente
env:
  PGPASSWORD: ${{ secrets.POSTGRES_PASSWORD }}
  POSTGRES_USER: ${{ secrets.POSTGRES_USER }}
run: |
  # Usar as variáveis normalmente
  pg_isready -h localhost -p 5432 -U "$POSTGRES_USER"
```yaml
### 3. Configuração PostgreSQL Melhorada

**Adições:**
- Timeout estendido para 180 segundos
- `POSTGRES_HOST_AUTH_METHOD=trust` para CI
- Debug output para troubleshooting
- Logs do Docker em caso de falha

### 4. Consistência de Variáveis de Ambiente

**Problemas Encontrados:**
- Mismatch entre `POSTGRES_*` e `MEAJUDAAI_DB_*`
- Uso inconsistente de variáveis entre jobs

**Soluções:**
- Padronização de nomes de variáveis
- Documentação clara de variáveis requeridas
- Verificação de secrets no início do workflow

## Resumo das Correções

| Arquivo | Problema Principal | Status |
|---------|-------------------|---------|
| `pr-validation.yml` | Bash syntax + env vars | ✅ Corrigido |
| `aspire-ci-cd.yml` | Bash syntax + PostgreSQL config | ✅ Corrigido |
| `ci-cd.yml` | N/A | ✅ Já estava correto |

## Comandos para Testar

### 1. Trigger Manual do Workflow
```bash
# Via GitHub UI: Actions → Pull Request Validation → Run workflow
```csharp
### 2. Verificar Logs
```bash
# Verificar se PostgreSQL está rodando
docker ps | grep postgres

# Verificar logs do PostgreSQL
docker logs $(docker ps -q --filter ancestor=postgres:15)
```powershell
### 3. Testar Conexão Local
```bash
# Definir variáveis
export PGPASSWORD="your-password"
export POSTGRES_USER="postgres"

# Testar conexão
pg_isready -h localhost -p 5432 -U "$POSTGRES_USER"
```text
## Lições Aprendidas

1. **Sempre usar sintaxe POSIX** em scripts de CI/CD
2. **Evitar interpolação direta** de secrets em comandos bash  
3. **Usar variáveis de ambiente** para todos os valores dinâmicos
4. **Incluir debug output** para facilitar troubleshooting
5. **Testar workflows localmente** quando possível

## Próximos Passos

- [ ] Monitorar execução dos workflows corrigidos
- [ ] Adicionar testes de validação de sintaxe bash
- [ ] Documentar padrões de CI/CD para o projeto
- [ ] Considerar usar shell scripts externos para lógica complexa

---
*Documentação criada em: {{ current_date }}*
*Última atualização: {{ current_date }}*