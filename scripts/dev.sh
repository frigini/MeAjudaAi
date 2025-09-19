#!/bin/bash

# =============================================================================
# MeAjudaAi Development Script - Ambiente de Desenvolvimento Local
# =============================================================================
# Script consolidado para desenvolvimento local da aplicação MeAjudaAi.
# Inclui configuração de infraestrutura, testes e execução local.
# 
# Uso:
#   ./scripts/dev.sh [opções]
#
# Opções:
#   -h, --help           Mostra esta ajuda
#   -v, --verbose        Modo verboso
#   -s, --simple         Execução simples sem Azure
#   -t, --test-only      Apenas executa testes
#   -b, --build-only     Apenas compila
#   --skip-deps          Pula verificação de dependências
#   --skip-tests         Pula execução de testes
#
# Exemplos:
#   ./scripts/dev.sh                    # Menu interativo
#   ./scripts/dev.sh --simple           # Execução local simples
#   ./scripts/dev.sh --test-only        # Apenas testes
#   ./scripts/dev.sh --build-only       # Apenas build
#
# Dependências:
#   - .NET 8 SDK
#   - Docker Desktop
#   - Azure CLI (para modo completo)
#   - PowerShell (Windows)
# =============================================================================

set -e  # Para em caso de erro

# === Configurações ===
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
RESOURCE_GROUP="meajudaai-dev"
ENVIRONMENT_NAME="dev"
BICEP_FILE="infrastructure/main.bicep"
LOCATION="brazilsouth"
PROJECT_DIR="src/Bootstrapper/MeAjudaAi.ApiService"
APPHOST_DIR="src/Aspire/MeAjudaAi.AppHost"

# === Variáveis de Controle ===
VERBOSE=false
SIMPLE_MODE=false
TEST_ONLY=false
BUILD_ONLY=false
SKIP_DEPS=false
SKIP_TESTS=false

# === Cores para output ===
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# === Função de ajuda ===
show_help() {
    sed -n '/^# =/,/^# =/p' "$0" | sed 's/^# //g' | sed 's/^=.*//g'
}

# === Funções de Logging ===
print_header() {
    echo -e "${BLUE}===================================================================${NC}"
    echo -e "${BLUE} $1${NC}"
    echo -e "${BLUE}===================================================================${NC}"
}

print_info() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_verbose() {
    if [ "$VERBOSE" = true ]; then
        echo -e "${CYAN}🔍 $1${NC}"
    fi
}

# === Parsing de argumentos ===
while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            show_help
            exit 0
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -s|--simple)
            SIMPLE_MODE=true
            shift
            ;;
        -t|--test-only)
            TEST_ONLY=true
            shift
            ;;
        -b|--build-only)
            BUILD_ONLY=true
            shift
            ;;
        --skip-deps)
            SKIP_DEPS=true
            shift
            ;;
        --skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        *)
            echo "Opção desconhecida: $1"
            show_help
            exit 1
            ;;
    esac
done

# === Navegar para raiz do projeto ===
cd "$PROJECT_ROOT"

# === Verificação de Dependências ===
check_dependencies() {
    if [ "$SKIP_DEPS" = true ]; then
        print_info "Pulando verificação de dependências..."
        return 0
    fi

    print_header "Verificando Dependências"
    
    # Verificar .NET
    print_verbose "Verificando .NET SDK..."
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK não encontrado. Instale o .NET 8 SDK."
        exit 1
    fi
    
    # Verificar versão do .NET
    DOTNET_VERSION=$(dotnet --version)
    print_info ".NET SDK encontrado: $DOTNET_VERSION"
    
    # Verificar Docker
    print_verbose "Verificando Docker..."
    if ! command -v docker &> /dev/null; then
        print_error "Docker não encontrado. Instale o Docker Desktop."
        exit 1
    fi
    
    # Verificar se Docker está rodando
    if ! docker info &> /dev/null; then
        print_error "Docker não está rodando. Inicie o Docker Desktop."
        exit 1
    fi
    
    print_info "Docker encontrado e rodando."
    
    # Verificar Azure CLI (apenas se não for modo simples)
    if [ "$SIMPLE_MODE" = false ]; then
        print_verbose "Verificando Azure CLI..."
        if ! command -v az &> /dev/null; then
            print_warning "Azure CLI não encontrado. Modo simples será usado."
            SIMPLE_MODE=true
        else
            print_info "Azure CLI encontrado."
        fi
    fi
    
    print_info "Todas as dependências verificadas!"
}

# === Build da Solução ===
build_solution() {
    print_header "Compilando Solução"
    
    print_info "Restaurando dependências..."
    dotnet restore
    
    print_info "Compilando solução..."
    if [ "$VERBOSE" = true ]; then
        dotnet build --no-restore --verbosity normal
    else
        dotnet build --no-restore --verbosity minimal
    fi
    
    if [ $? -eq 0 ]; then
        print_info "Build concluído com sucesso!"
    else
        print_error "Falha no build. Verifique os erros acima."
        exit 1
    fi
}

# === Execução de Testes ===
run_tests() {
    if [ "$SKIP_TESTS" = true ]; then
        print_info "Pulando execução de testes..."
        return 0
    fi

    print_header "Executando Testes"
    
    print_info "Executando testes unitários..."
    if [ "$VERBOSE" = true ]; then
        dotnet test --no-build --verbosity normal
    else
        dotnet test --no-build --verbosity minimal
    fi
    
    if [ $? -eq 0 ]; then
        print_info "Todos os testes passaram!"
    else
        print_error "Alguns testes falharam. Verifique os resultados acima."
        exit 1
    fi
}

# === Azure Infrastructure Setup ===
setup_azure_infrastructure() {
    print_header "Configurando Infraestrutura Azure"
    
    print_info "Fazendo login no Azure (se necessário)..."
    az account show > /dev/null 2>&1 || az login

    print_info "Deploying Bicep template..."
    DEPLOYMENT_NAME="sb-deployment-$(date +%s)"

    # Deploy the Bicep template first
    az deployment group create \
      --name "$DEPLOYMENT_NAME" \
      --resource-group "$RESOURCE_GROUP" \
      --template-file "$BICEP_FILE" \
      --parameters environmentName="$ENVIRONMENT_NAME" location="$LOCATION"

    # Get the Service Bus namespace and policy names from outputs
    NAMESPACE_NAME=$(az deployment group show \
      --name "$DEPLOYMENT_NAME" \
      --resource-group "$RESOURCE_GROUP" \
      --query "properties.outputs.serviceBusNamespace.value" \
      --output tsv)

    MANAGEMENT_POLICY_NAME=$(az deployment group show \
      --name "$DEPLOYMENT_NAME" \
      --resource-group "$RESOURCE_GROUP" \
      --query "properties.outputs.managementPolicyName.value" \
      --output tsv)

    # Get the connection string securely using Azure CLI
    OUTPUT_JSON=$(az servicebus namespace authorization-rule keys list \
      --resource-group "$RESOURCE_GROUP" \
      --namespace-name "$NAMESPACE_NAME" \
      --name "$MANAGEMENT_POLICY_NAME" \
      --query "primaryConnectionString" \
      --output tsv)

    if [ -z "$OUTPUT_JSON" ]; then
      print_error "Não foi possível extrair a ConnectionString do output do Bicep."
      exit 1
    fi

    print_info "ConnectionString obtida com sucesso."
    export Messaging__ServiceBus__ConnectionString="$OUTPUT_JSON"
}

# === Execução Local ===
run_local() {
    print_header "Executando Aplicação Local"
    
    if [ "$SIMPLE_MODE" = false ]; then
        setup_azure_infrastructure
    else
        print_info "Modo simples - usando configurações locais..."
        export ASPNETCORE_ENVIRONMENT=Development
    fi
    
    print_info "Iniciando aplicação Aspire..."
    cd "$APPHOST_DIR"
    dotnet run
}

# === Configurar User Secrets ===
setup_user_secrets() {
    print_header "Configurando User Secrets"
    
    print_info "Configurando secrets para o projeto API..."
    cd "$PROJECT_DIR"
    
    # Lista os secrets atuais
    print_info "Secrets atuais:"
    dotnet user-secrets list || print_warning "Nenhum secret configurado."
    
    echo ""
    print_info "Para adicionar um novo secret, use:"
    echo "  dotnet user-secrets set \"chave\" \"valor\""
    echo ""
    print_info "Exemplo:"
    echo "  dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"sua-connection-string\""
}

# === Menu Principal ===
show_menu() {
    print_header "MeAjudaAi Development Script"
    echo "Escolha uma opção:"
    echo ""
    echo "  1) ✅ Verificar dependências"
    echo "  2) 🔨 Build completo (compile + teste)"
    echo "  3) 🔧 Apenas compilar"
    echo "  4) 🧪 Apenas testar"
    echo "  5) 🚀 Executar local (com Azure)"
    echo "  6) ⚡ Executar local (simples - sem Azure)"
    echo "  7) 🔐 Configurar user-secrets"
    echo "  8) 🎯 Setup completo (build + test + run)"
    echo "  0) ❌ Sair"
    echo ""
    read -p "Digite sua escolha [0-8]: " choice
}

# === Processamento de Menu ===
process_menu_choice() {
    case $choice in
        1)
            check_dependencies
            ;;
        2)
            check_dependencies
            build_solution
            run_tests
            ;;
        3)
            check_dependencies
            build_solution
            ;;
        4)
            run_tests
            ;;
        5)
            check_dependencies
            build_solution
            run_tests
            run_local
            ;;
        6)
            SIMPLE_MODE=true
            check_dependencies
            build_solution
            run_tests
            run_local
            ;;
        7)
            setup_user_secrets
            ;;
        8)
            check_dependencies
            build_solution
            run_tests
            run_local
            ;;
        0)
            print_info "Saindo..."
            exit 0
            ;;
        *)
            print_error "Opção inválida!"
            ;;
    esac
}

# === Execução Principal ===
main() {
    # Execução baseada em argumentos
    if [ "$TEST_ONLY" = true ]; then
        run_tests
        exit 0
    fi
    
    if [ "$BUILD_ONLY" = true ]; then
        check_dependencies
        build_solution
        exit 0
    fi
    
    # Se há argumentos específicos, executa diretamente
    if [ "$SIMPLE_MODE" = true ] && [ $# -gt 0 ]; then
        check_dependencies
        build_solution
        run_tests
        run_local
        exit 0
    fi
    
    # Menu interativo
    while true; do
        show_menu
        process_menu_choice
        echo ""
        read -p "Pressione Enter para continuar..."
    done
}

# === Execução ===
main "$@"