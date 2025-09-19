# MeAjudaAi - Shared API Collections

Esta pasta contém resources compartilhados entre todos os módulos da aplicação MeAjudaAi.

## 📁 Estrutura

```
src/Shared/API.Collections/
├── README.md                    # Esta documentação
├── Setup/
│   ├── SetupGetKeycloakToken.bru  # 🔑 Autenticação Keycloak (OBRIGATÓRIO)
│   ├── HealthCheckAll.bru         # 🏥 Verificação de saúde de todos os serviços
│   └── AspireDashboard.bru        # 📊 Informações do Aspire Dashboard
└── Common/
    ├── GlobalVariables.bru        # 🌍 Variáveis globais compartilhadas
    └── StandardHeaders.bru        # 📋 Headers padrão da API
```

## 🚀 Como Usar

### 1. **Setup Inicial (OBRIGATÓRIO)**

Antes de usar qualquer collection de módulo, execute:

```
📁 Setup/SetupGetKeycloakToken.bru
```

Este endpoint:
- ✅ Obtém token de acesso do Keycloak
- ✅ Define automaticamente a variável `accessToken` 
- ✅ Funciona para todos os módulos (Users, Providers, Services, etc.)

### 2. **Verificação de Saúde**

Para verificar se todos os serviços estão funcionando:

```
📁 Setup/HealthCheckAll.bru
```

### 3. **Informações do Sistema**

Para ver estado do Aspire e serviços:

```
📁 Setup/AspireDashboard.bru
```

## 🔧 Integração com Módulos

### **Para Desenvolvedores de Módulos:**

1. **No README do seu módulo**, documente:
   ```markdown
   ## 🔧 Setup Inicial
   
   ### 1. Autenticação (COMPARTILHADO)
   Execute primeiro: `src/Shared/API.Collections/Setup/SetupGetKeycloakToken.bru`
   
   ### 2. Testes do Módulo
   Agora execute os endpoints específicos do módulo...
   ```

2. **Na sua collection.bru**, referencie as variáveis compartilhadas:
   ```javascript
   vars {
     # Módulo-specific variables
     userId: 
     testEmail: test@example.com
     
     # Global variables (set by shared Setup)
     # Execute src/Shared/API.Collections/Setup/SetupGetKeycloakToken.bru first
     # accessToken: [AUTO-SET by shared setup]
     # baseUrl: [AUTO-SET by shared setup]
   }
   ```

## ⚙️ Variáveis Compartilhadas

### **Definidas pelo Setup:**
- `accessToken`: Token JWT do Keycloak
- `refreshToken`: Refresh token para renovação
- `baseUrl`: URL base da API (http://localhost:5000)
- `keycloakUrl`: URL do Keycloak (http://localhost:8080)
- `realm`: Realm do Keycloak (meajudaai-realm)

### **Usadas por todos os módulos:**
- Headers de autenticação automáticos
- Timeouts padrão
- Configurações de retry

## 🎯 Workflow Recomendado

### **Para desenvolvimento:**
1. 🔑 Execute `Setup/SetupGetKeycloakToken.bru` (uma vez)
2. 🏥 Execute `Setup/HealthCheckAll.bru` (verificar serviços)
3. 🚀 Execute endpoints do módulo específico
4. 🔄 Re-execute setup se token expirar

### **Para CI/CD:**
1. Automatize execução do setup antes dos testes
2. Use variables de ambiente para diferentes ambientes
3. Configure timeouts apropriados para cada ambiente

## 🚨 Troubleshooting

### **Token expirado:**
- Re-execute `Setup/SetupGetKeycloakToken.bru`
- Verifique se Keycloak está rodando

### **Serviços indisponíveis:**
- Execute `Setup/HealthCheckAll.bru`
- Verifique Aspire Dashboard
- Confirme se `dotnet run --project src/Aspire/MeAjudaAi.AppHost` está ativo

### **Variables não definidas:**
- Confirme execução do setup compartilhado
- Verifique logs no console do Bruno
- Valide se está usando Bruno versão recente

## 📚 Documentação Adicional

- **Aspire Dashboard**: https://localhost:15888
- **Keycloak Admin**: http://localhost:8080/admin
- **API Base**: http://localhost:5000

## 🔄 Manutenção

### **Para atualizar autenticação:**
- Modifique apenas `Setup/SetupGetKeycloakToken.bru`
- Mudanças automaticamente aplicadas a todos os módulos

### **Para adicionar novos headers globais:**
- Adicione em `Common/StandardHeaders.bru`
- Documente no README dos módulos

### **Para novas variáveis globais:**
- Adicione em `Common/GlobalVariables.bru`
- Comunique mudanças para todos os módulos

---

**📝 Última atualização**: September 2025  
**🔧 Compatível com**: Bruno v1.x+  
**🏗️ Versão da API**: v1