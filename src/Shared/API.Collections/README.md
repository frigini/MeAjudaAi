# MeAjudaAi - Shared API Collections

Esta pasta contém resources compartilhados entre todos os módulos da aplicação MeAjudaAi.

## 📁 Estrutura

```text
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

### 1. **Setup Inicial (RECOMENDADO)**

No Bruno, cada módulo é uma **Collection** independente. Para compartilhar o token entre elas:

1. **Abra a Collection Shared**: `src/Shared/API.Collections`
2. **Selecione um Ambiente**: Escolha `Local` (ou crie um novo).
3. **Execute o Setup**:
   ```
   📁 Setup/SetupGetKeycloakToken.bru
   ```
4. **No seu Módulo**: Utilize o **mesmo arquivo de ambiente** ou copie o valor da variável `accessToken` gerada.

> **Dica**: No Bruno, você pode importar o arquivo `src/Shared/API.Collections/environments/Local.bru` em qualquer coleção para manter as URLs e credenciais sincronizadas.

### 2. **Verificação de Saúde**
```
📁 Setup/HealthCheckAll.bru
```

### 3. **Informações do Sistema**
```
📁 Setup/AspireDashboard.bru
```

## 🔧 Integração com Módulos

### **Para Desenvolvedores de Módulos:**

1. **Importe o ambiente compartilhado**:
   Aponte seu Bruno para `src/Shared/API.Collections/environments/Local.bru`.

2. **No README do seu módulo**, documente:
   ```markdown
   ## 🔧 Setup Inicial
   
   ### 1. Autenticação (COMPARTILHADO)
   1. Abra a coleção `src/Shared/API.Collections`.
   2. Execute `Setup/SetupGetKeycloakToken.bru` usando o ambiente `Local`.
   3. O token será salvo na variável `accessToken` do ambiente.
   
   ### 2. Testes do Módulo
   Certifique-se de que sua coleção está usando o mesmo ambiente `Local`.
   ```

3. **Na sua collection.bru**, herde as variáveis:
   ```javascript
   vars {
     # O token e baseUrl virão do Ambiente Local compartilhado
     # accessToken: {{accessToken}}
     # baseUrl: {{baseUrl}}
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

- **Aspire Dashboard**: https://localhost:17063
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

**📝 Última atualização**: June 4, 2026  
**🔧 Compatível com**: Bruno v1.x+  
**🏗️ Versão da API**: v1