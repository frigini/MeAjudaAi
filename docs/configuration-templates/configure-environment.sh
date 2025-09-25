#!/bin/bash

# Script de configuração automatizada para diferentes ambientes
# Uso: ./configure-environment.sh [development|production]

set -e

ENVIRONMENT=${1:-development}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
CONFIG_DIR="$PROJECT_ROOT/docs/configuration-templates"
TARGET_DIR="$PROJECT_ROOT/src/Bootstrapper/MeAjudaAi.ApiService"

echo "🔧 Configurando ambiente: $ENVIRONMENT"

# Validar ambiente
case $ENVIRONMENT in
  development|production)
    ;;
  *)
    echo "❌ Ambiente inválido: $ENVIRONMENT"
    echo "Ambientes suportados: development, production"
    exit 1
    ;;
esac

# Função para copiar e configurar arquivo
configure_appsettings() {
    local env=$1
    local template_file="$CONFIG_DIR/appsettings.$env.template.json"
    local target_file="$TARGET_DIR/appsettings.$env.json"
    
    if [ ! -f "$template_file" ]; then
        echo "❌ Template não encontrado: $template_file"
        exit 1
    fi
    
    echo "📄 Copiando template para: $target_file"
    cp "$template_file" "$target_file"
    
    # Substituir variáveis de ambiente se estiverem definidas
    if [[ "${env,,}" != "development" ]]; then
        echo "🔄 Substituindo variáveis de ambiente..."
        
        # Lista de variáveis esperadas
        declare -a vars=(
            "DATABASE_CONNECTION_STRING"
            "REDIS_CONNECTION_STRING" 
            "KEYCLOAK_BASE_URL"
            "KEYCLOAK_CLIENT_ID"
            "KEYCLOAK_CLIENT_SECRET"
            "SERVICEBUS_CONNECTION_STRING"
            "RABBITMQ_HOSTNAME"
            "RABBITMQ_USERNAME"
            "RABBITMQ_PASSWORD"
        )
        
        for var in "${vars[@]}"; do
            if [ ! -z "${!var}" ]; then
                echo "  ✅ Substituindo \${$var}"
                sed -i "s|\${$var}|${!var}|g" "$target_file"
            else
                echo "  ⚠️  Variável não definida: $var"
            fi
        done
    fi
    
    echo "✅ Configuração criada: $target_file"
}

# Função para validar configuração
validate_config() {
    local env=$1
    local config_file="$TARGET_DIR/appsettings.$env.json"
    
    echo "🔍 Validando configuração..."
    
    if ! command -v jq &> /dev/null; then
        echo "⚠️  jq não encontrado - validação JSON ignorada"
        return 0
    fi
    
    if ! jq empty "$config_file" 2>/dev/null; then
        echo "❌ JSON inválido em: $config_file"
        exit 1
    fi
    
    # Validações específicas por ambiente
    case "${env,,}" in
        production)
            # Verificar se ainda há variáveis não substituídas
            if grep -q '\${' "$config_file"; then
                echo "❌ Variáveis não substituídas encontradas em produção:"
                grep '\${' "$config_file"
                exit 1
            fi
            
            # Verificar configurações de segurança
            if ! jq -e '.Security.EnforceHttps == true' "$config_file" >/dev/null; then
                echo "❌ HTTPS deve estar habilitado em produção"
                exit 1
            fi
            ;;
    esac
    
    echo "✅ Configuração válida"
}

# Função para criar arquivo de ambiente
create_env_file() {
    local env=$1
    local env_file="$PROJECT_ROOT/.env.$env"
    
    if [[ "${env,,}" = "development" ]]; then
        echo "⏭️  Arquivo .env não necessário para development"
        return 0
    fi
    
    echo "📝 Criando arquivo de exemplo: $env_file.example"
    
    cat > "$env_file.example" << EOF
# Variáveis de ambiente para $env
# Copie este arquivo para .env.$env e configure os valores reais

# Database
DATABASE_CONNECTION_STRING="Host=your-db-host;Database=meajudaai_$env;Username=your-user;Password=your-password;Port=5432;SslMode=Require;"

# Redis
REDIS_CONNECTION_STRING="your-redis-host:6379"

# Keycloak
KEYCLOAK_BASE_URL="https://your-keycloak-host"
KEYCLOAK_CLIENT_ID="meajudaai-$env"
KEYCLOAK_CLIENT_SECRET="your-keycloak-secret"

# Messaging
SERVICEBUS_CONNECTION_STRING="your-servicebus-connection"
RABBITMQ_HOSTNAME="your-rabbitmq-host"
RABBITMQ_USERNAME="your-rabbitmq-user"
RABBITMQ_PASSWORD="your-rabbitmq-password"
EOF
    
    echo "✅ Arquivo de exemplo criado: $env_file.example"
}

# Função principal
main() {
    echo "🚀 Iniciando configuração do ambiente $ENVIRONMENT"
    
    # Criar diretório de destino se não existir
    mkdir -p "$TARGET_DIR"
    
    # Configurar appsettings
    case $ENVIRONMENT in
        development)
            configure_appsettings "Development"
            ;;
        production)
            configure_appsettings "Production"
            ;;
    esac
    
    # Validar configuração
    case $ENVIRONMENT in
        development)
            validate_config "Development"
            ;;
        production)
            validate_config "Production"
            ;;
    esac
    
    # Criar arquivo de ambiente
    create_env_file "$ENVIRONMENT"
    
    echo ""
    echo "🎉 Configuração do ambiente $ENVIRONMENT concluída!"
    echo ""
    echo "📋 Próximos passos:"
    
    case $ENVIRONMENT in
        development)
            echo "  1. Execute: dotnet run --project $TARGET_DIR"
            echo "  2. Acesse: http://localhost:5000/swagger"
            ;;
        production)
            echo "  1. Configure as variáveis de ambiente em .env.$ENVIRONMENT"
            echo "  2. Configure o serviço de secrets (Azure Key Vault, etc.)"
            echo "  3. Execute o deploy para $ENVIRONMENT"
            echo "  4. Verifique os health checks"
            ;;
    esac
    
    echo ""
    echo "📚 Documentação: $CONFIG_DIR/README.md"
}

# Verificar se está sendo executado como script principal
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi