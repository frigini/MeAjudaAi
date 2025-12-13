# 🏗️ Infrastructure Scripts - MeAjudaAi

Scripts para configuração e gerenciamento da infraestrutura local e remota (PostgreSQL, Keycloak, Docker, Azure).

---

## 📋 Índice

- [Database Scripts](#-database-scripts)
- [Keycloak Scripts](#-keycloak-scripts)
- [Docker Compose Scripts](#-docker-compose-scripts)
- [Testing Scripts](#-testing-scripts)
- [Deployment](#-deployment)

---

## 🗄️ Database Scripts

### **`database/01-init-meajudaai.sh`**
**Propósito**: Inicialização de schemas PostgreSQL para todos os módulos  
**Quando Usar**: Executado automaticamente pelo Docker Compose no primeiro start  
**Módulos Criados**:
- `users` - Gerenciamento de usuários
- `providers` - Prestadores de serviços
- `service_catalogs` - Catálogo de serviços
- `documents` - Documentos e verificações
- `locations` - Cidades e geolocalização
- `search_providers` - Índice de busca (RediSearch)

**Execução Manual**:
```bash
# Conectar ao container PostgreSQL
docker exec -it postgres psql -U postgres -d meajudaai

# Executar script
\i /docker-entrypoint-initdb.d/01-init-meajudaai.sh
```

---

### **`database/create-module.ps1`**
**Propósito**: Template/helper para criar schema de novo módulo  
**Quando Usar**: Ao adicionar novo módulo ao projeto

**Uso**:
```powershell
# Criar schema para novo módulo "Orders"
.\infrastructure\database\create-module.ps1 -ModuleName "Orders"

# Output: Cria script SQL em database/modules/
```

**O que gera**:
- Schema SQL com permissões
- Tabelas exemplo
- Extensões necessárias (uuid-ossp)

---

## 🔐 Keycloak Scripts

### **`keycloak/scripts/keycloak-init-dev.sh`**
**Propósito**: Configuração Keycloak para ambiente Development  
**Quando Usar**: Setup inicial local ou reset de auth

**O que configura**:
- Realm `meajudaai-dev`
- Clients: `api-service`, `admin-portal`, `customer-app`
- Roles padrão: `admin`, `user`, `provider`
- Usuários de teste

**Execução**:
```bash
# Pré-requisito: Keycloak rodando
docker-compose up -d keycloak

# Executar init
./infrastructure/keycloak/scripts/keycloak-init-dev.sh
```

**Variáveis de Ambiente**:
```bash
KEYCLOAK_URL=http://localhost:8080
KEYCLOAK_ADMIN_USER=admin
KEYCLOAK_ADMIN_PASSWORD=admin
```

---

### **`keycloak/scripts/keycloak-init-prod.sh`**
**Propósito**: Configuração Keycloak para ambiente Production  
**Quando Usar**: Deployment em Azure/produção

**Diferenças vs Dev**:
- ❌ Sem usuários de teste
- ✅ HTTPS obrigatório
- ✅ Password policies fortes
- ✅ Rate limiting configurado

**⚠️ ATENÇÃO**: Este script **NÃO** deve ser executado em produção manualmente. É usado apenas via pipeline CI/CD.

---

## 🐳 Docker Compose Scripts

### **`compose/environments/setup-secrets.sh`**
**Propósito**: Configurar Docker secrets para ambientes locais  
**Quando Usar**: Primeira vez usando docker-compose ou ao regenerar secrets

**Uso**:
```bash
# Setup ambiente development
./infrastructure/compose/environments/setup-secrets.sh development

# Setup ambiente staging
./infrastructure/compose/environments/setup-secrets.sh staging
```

**Secrets Criados**:
- `postgres_password`
- `keycloak_admin_password`
- `redis_password`
- `rabbitmq_password`
- `app_connection_string`

**Localização**: `.secrets/{environment}/`

---

### **`compose/environments/verify-resources.sh`**
**Propósito**: Health check de todos os recursos Docker  
**Quando Usar**: Troubleshooting ou validação pós-deploy

**Uso**:
```bash
./infrastructure/compose/environments/verify-resources.sh
```

**Verifica**:
- ✅ PostgreSQL (port 5432)
- ✅ Keycloak (port 8080)
- ✅ Redis (port 6379)
- ✅ RabbitMQ (port 5672, 15672)
- ✅ Seq (port 5341)

**Output Exemplo**:
```
🔍 Verificando recursos Docker...
✅ PostgreSQL: Healthy (port 5432)
✅ Keycloak: Healthy (port 8080)
✅ Redis: Healthy (port 6379)
⚠️  RabbitMQ: Not responding (port 5672)
```

---

### **`compose/standalone/postgres/init/02-custom-setup.sh`**
**Propósito**: Customizações adicionais PostgreSQL (extensões, configurações)  
**Quando Usar**: Executado automaticamente após `01-init-meajudaai.sh`

**Configurações**:
- Extensões: PostGIS, pg_trgm, btree_gin
- Performance tuning para desenvolvimento
- Logging configurado

---

## 🧪 Testing Scripts

### **`test-database-init.sh`** / **`test-database-init.ps1`**
**Propósito**: Validar que todos os scripts de init executam sem erros  
**Quando Usar**: Após modificar scripts de database ou adicionar novo módulo

**Bash (Linux/macOS)**:
```bash
./infrastructure/test-database-init.sh
```

**PowerShell (Windows)**:
```powershell
.\infrastructure\test-database-init.ps1
```

**O que testa**:
1. Docker está rodando?
2. Containers iniciam corretamente?
3. Scripts SQL executam sem erros?
4. Schemas foram criados?
5. Permissões estão corretas?

**Output Exemplo**:
```
🧪 Testing Database Initialization Scripts

✅ Docker is running
✅ Starting containers...
✅ Executing init scripts...
✅ Schema 'users' created
✅ Schema 'providers' created
...
✅ All tests passed!
```

---

## 🚀 Deployment

### **Azure Deployment**
Para deploy em Azure, use:
```bash
# Deploy completo (Bicep)
./scripts/deploy.sh production brazilsouth
```

Ver [../scripts/README.md](../scripts/README.md#-deployrsh---deploy-azure) para detalhes.

---

## 📁 Estrutura de Diretórios

```
infrastructure/
├── README.md (este arquivo)
├── main.bicep (template Bicep principal)
├── servicebus.bicep (Azure Service Bus)
├── database/
│   ├── 01-init-meajudaai.sh (init PostgreSQL)
│   └── create-module.ps1 (template novo módulo)
├── keycloak/
│   └── scripts/
│       ├── keycloak-init-dev.sh
│       └── keycloak-init-prod.sh
├── compose/
│   ├── base/ (docker-compose base)
│   ├── environments/
│   │   ├── setup-secrets.sh
│   │   └── verify-resources.sh
│   └── standalone/ (compose standalone)
├── rabbitmq/ (configs RabbitMQ)
├── test-database-init.sh
└── test-database-init.ps1
```

---

## 🔧 Troubleshooting

### **Problema**: "Schema already exists"
```bash
# Solução: Drop e recria
docker exec -it postgres psql -U postgres -d meajudaai -c "DROP SCHEMA users CASCADE; DROP SCHEMA providers CASCADE;"
docker-compose restart postgres
```

### **Problema**: "Permission denied"
```bash
# Solução: Dar permissões de execução
chmod +x infrastructure/**/*.sh
```

### **Problema**: Keycloak não aceita configuração
```bash
# Solução: Reset completo
docker-compose down -v
docker-compose up -d keycloak
# Aguardar 30s para Keycloak inicializar
./infrastructure/keycloak/scripts/keycloak-init-dev.sh
```

---

## 📚 Recursos Adicionais

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Init Scripts](https://hub.docker.com/_/postgres) - ver "Initialization scripts"
- [Keycloak Admin CLI](https://www.keycloak.org/docs/latest/server_admin/#admin-cli)
- [Azure Bicep Templates](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/)

---

**Última Atualização**: 12 Dez 2025  
**Manutenção**: Atualizar ao adicionar novos scripts ou módulos
