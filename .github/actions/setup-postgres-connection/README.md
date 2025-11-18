# Setup PostgreSQL Connection Action

Ação composta reutilizável para construir a string de conexão do PostgreSQL a partir de secrets e valores padrão.

## Uso

```yaml
- name: Setup PostgreSQL connection
  id: db
  uses: ./.github/actions/setup-postgres-connection
  with:
    postgres-host: localhost
    postgres-port: 5432
    postgres-db: ${{ secrets.POSTGRES_DB || 'meajudaai_test' }}
    postgres-user: ${{ secrets.POSTGRES_USER || 'postgres' }}
    postgres-password: ${{ secrets.POSTGRES_PASSWORD || 'test123' }}

- name: Use connection string
  env:
    ConnectionStrings__DefaultConnection: ${{ steps.db.outputs.connection-string }}
  run: dotnet test
```

## Inputs

| Nome | Descrição | Obrigatório | Padrão |
|------|-----------|-------------|--------|
| `postgres-host` | Host do PostgreSQL | Não | `localhost` |
| `postgres-port` | Porta do PostgreSQL | Não | `5432` |
| `postgres-db` | Nome do banco de dados | Não | `meajudaai_test` |
| `postgres-user` | Usuário do PostgreSQL | Não | `postgres` |
| `postgres-password` | Senha do PostgreSQL | Não | `test123` |

## Outputs

| Nome | Descrição |
|------|-----------|
| `connection-string` | String de conexão completa no formato Npgsql |

## Benefícios

- **Centralização**: Lógica de construção da connection string em um único lugar
- **Manutenibilidade**: Facilita updates na estrutura da connection string
- **Legibilidade**: Linhas mais curtas nos workflows (< 120 caracteres)
- **Reutilização**: Usada em `aspire-ci-cd.yml`, `ci-cd.yml` e `pr-validation.yml`

## Formato da Connection String

```text
Host={host};Port={port};Database={database};Username={username};Password={password}
```
