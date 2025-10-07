#!/bin/bash

# =============================================================================
# MeAjudaAi Azure Infrastructure Deployment Script
# =============================================================================
# Script para deploy automatizado da infraestrutura Azure usando Bicep.
# Suporta múltiplos ambientes (dev, prod) com configurações específicas.
# 
# Uso:
#   ./scripts/deploy.sh <ambiente> <localização> [opções]
#
# Argumentos:
#   ambiente        Ambiente de destino (dev, prod)
#   localização     Região Azure (ex: brazilsouth, eastus)
#
# Opções:
#   -h, --help           Mostra esta ajuda
#   -v, --verbose        Modo verboso
#   -g, --resource-group Nome personalizado do resource group
#   -d, --dry-run        Simula o deploy sem executar
#   -f, --force          Força o deploy mesmo com warnings
#   --skip-validation    Pula validação do template
#   --what-if           Mostra o que seria alterado sem executar
#
# Exemplos:
#   ./scripts/deploy.sh dev brazilsouth           # Deploy desenvolvimento
#   ./scripts/deploy.sh prod brazilsouth -v       # Deploy produção verboso
#   ./scripts/deploy.sh prod eastus --what-if  # Simular produção
#
# Dependências:
#   - Azure CLI autenticado
#   - Permissões Contributor no resource group
#   - Templates Bicep na pasta infrastructure/
# =============================================================================

set -e  # Para em caso de erro

# === Configurações ===
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
INFRASTRUCTURE_DIR="$PROJECT_ROOT/infrastructure"
BICEP_FILE="$INFRASTRUCTURE_DIR/main.bicep"

# === Variáveis de Controle ===
VERBOSE=false
DRY_RUN=false
FORCE=false
SKIP_VALIDATION=false
WHAT_IF=false
CUSTOM_RESOURCE_GROUP=""

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
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_verbose() {
    if [ "$VERBOSE" = true ]; then
        echo -e "${CYAN}[VERBOSE]${NC} $1"
    fi
}

# === Parsing de argumentos ===
ENVIRONMENT=""
LOCATION=""

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
        -g|--resource-group)
            CUSTOM_RESOURCE_GROUP="$2"
            shift 2
            ;;
        -d|--dry-run)
            DRY_RUN=true
            shift
            ;;
        -f|--force)
            FORCE=true
            shift
            ;;
        --skip-validation)
            SKIP_VALIDATION=true
            shift
            ;;
        --what-if)
            WHAT_IF=true
            shift
            ;;
        -*)
            print_error "Opção desconhecida: $1"
            show_help
            exit 1
            ;;
        *)
            if [ -z "$ENVIRONMENT" ]; then
                ENVIRONMENT="$1"
            elif [ -z "$LOCATION" ]; then
                LOCATION="$1"
            else
                print_error "Muitos argumentos fornecidos"
                show_help
                exit 1
            fi
            shift
            ;;
    esac
done

# === Validação de Argumentos ===
if [ -z "$ENVIRONMENT" ] || [ -z "$LOCATION" ]; then
    print_error "Ambiente e localização são obrigatórios"
    show_help
    exit 1
fi

# === Configuração de Variáveis ===
case $ENVIRONMENT in
    dev|prod)
        print_verbose "Ambiente válido: $ENVIRONMENT"
        ;;
    *)
        print_error "Ambiente inválido: $ENVIRONMENT. Deve ser: dev ou prod"
        exit 1
        ;;
esac

RESOURCE_GROUP=${CUSTOM_RESOURCE_GROUP:-"meajudaai-${ENVIRONMENT}"}
DEPLOYMENT_NAME="deploy-$(date +%Y%m%d-%H%M%S)"

print_verbose "Configurações:"
print_verbose "  Ambiente: $ENVIRONMENT"
print_verbose "  Localização: $LOCATION"
print_verbose "  Resource Group: $RESOURCE_GROUP"
print_verbose "  Deployment Name: $DEPLOYMENT_NAME"

# === Navegar para raiz do projeto ===
cd "$PROJECT_ROOT"

# === Verificação de Pré-requisitos ===
check_prerequisites() {
    print_header "Verificando Pré-requisitos"
    
    # Verificar Azure CLI
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI não encontrado. Instale o Azure CLI."
        exit 1
    fi
    
    print_verbose "Azure CLI encontrado"
    
    # Verificar autenticação
    print_verbose "Verificando autenticação Azure..."
    if ! az account show &> /dev/null; then
        print_error "Não autenticado no Azure. Execute: az login"
        exit 1
    fi
    
    local subscription=$(az account show --query name -o tsv)
    print_info "Autenticado na subscription: $subscription"
    
    # Verificar arquivo Bicep
    if [ ! -f "$BICEP_FILE" ]; then
        print_error "Template Bicep não encontrado: $BICEP_FILE"
        exit 1
    fi
    
    print_verbose "Template Bicep encontrado: $BICEP_FILE"
    
    print_success "Todos os pré-requisitos verificados!"
}

# === Validação do Template ===
validate_template() {
    if [ "$SKIP_VALIDATION" = true ]; then
        print_warning "Pulando validação do template..."
        return 0
    fi

    print_header "Validando Template Bicep"
    
    print_info "Executando validação do template..."
    
    local validation_output
    if validation_output=$(az deployment group validate \
        --resource-group "$RESOURCE_GROUP" \
        --template-file "$BICEP_FILE" \
        --parameters environmentName="$ENVIRONMENT" location="$LOCATION" \
        2>&1); then
        print_success "Template válido!"
        if [ "$VERBOSE" = true ]; then
            echo "$validation_output"
        fi
    else
        print_error "Falha na validação do template:"
        echo "$validation_output"
        
        if [ "$FORCE" = false ]; then
            exit 1
        else
            print_warning "Continuando devido ao flag --force"
        fi
    fi
}

# === What-If Analysis ===
run_what_if() {
    print_header "Análise What-If"
    
    print_info "Analisando mudanças que seriam aplicadas..."
    
    az deployment group what-if \
        --resource-group "$RESOURCE_GROUP" \
        --template-file "$BICEP_FILE" \
        --parameters environmentName="$ENVIRONMENT" location="$LOCATION"
    
    print_info "Análise what-if concluída."
}

# === Criação do Resource Group ===
ensure_resource_group() {
    print_header "Verificando Resource Group"
    
    if az group show --name "$RESOURCE_GROUP" &> /dev/null; then
        print_info "Resource group '$RESOURCE_GROUP' já existe"
    else
        print_info "Criando resource group '$RESOURCE_GROUP'..."
        
        if [ "$DRY_RUN" = false ]; then
            az group create \
                --name "$RESOURCE_GROUP" \
                --location "$LOCATION" \
                --tags environment="$ENVIRONMENT" project="meajudaai"
            
            print_success "Resource group criado com sucesso!"
        else
            print_info "[DRY-RUN] Resource group seria criado"
        fi
    fi
}

# === Deploy da Infraestrutura ===
deploy_infrastructure() {
    print_header "Deploying Infraestrutura Azure"
    
    if [ "$DRY_RUN" = true ]; then
        print_info "[DRY-RUN] Deploy seria executado com os seguintes parâmetros:"
        print_info "  Resource Group: $RESOURCE_GROUP"
        print_info "  Template: $BICEP_FILE"
        print_info "  Environment: $ENVIRONMENT"
        print_info "  Location: $LOCATION"
        return 0
    fi
    
    print_info "Iniciando deploy da infraestrutura..."
    print_info "  Deployment Name: $DEPLOYMENT_NAME"
    
    local deploy_args=""
    if [ "$VERBOSE" = true ]; then
        deploy_args="--verbose"
    fi
    
    # Executar deploy
    az deployment group create \
        --name "$DEPLOYMENT_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --template-file "$BICEP_FILE" \
        --parameters environmentName="$ENVIRONMENT" location="$LOCATION" \
        $deploy_args
    
    if [ $? -eq 0 ]; then
        print_success "Deploy concluído com sucesso!"
    else
        print_error "Falha no deploy"
        exit 1
    fi
}

# === Obter Outputs do Deploy ===
get_deployment_outputs() {
    print_header "Obtendo Outputs do Deploy"
    
    if [ "$DRY_RUN" = true ]; then
        print_info "[DRY-RUN] Outputs seriam obtidos"
        return 0
    fi
    
    print_info "Obtendo outputs do deployment..."
    
    # Listar todos os outputs
    local outputs=$(az deployment group show \
        --name "$DEPLOYMENT_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --query "properties.outputs" \
        --output table)
    
    if [ -n "$outputs" ]; then
        print_success "Outputs do deployment:"
        echo "$outputs"
        
        # Salvar outputs em arquivo
        local outputs_file="$PROJECT_ROOT/deployment-outputs-$ENVIRONMENT.json"
        az deployment group show \
            --name "$DEPLOYMENT_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --query "properties.outputs" \
            --output json > "$outputs_file"
        
        print_info "Outputs salvos em: $outputs_file"
    else
        print_warning "Nenhum output encontrado no deployment"
    fi
}

# === Relatório Final ===
show_summary() {
    print_header "Resumo do Deploy"
    
    print_info "Deployment executado com sucesso!"
    print_info "  Ambiente: $ENVIRONMENT"
    print_info "  Resource Group: $RESOURCE_GROUP"
    print_info "  Localização: $LOCATION"
    
    if [ "$DRY_RUN" = false ]; then
        print_info "  Deployment Name: $DEPLOYMENT_NAME"
        
        # Link para o portal
        local portal_url="https://portal.azure.com/#@/resource/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$RESOURCE_GROUP"
        print_info "  Portal Azure: $portal_url"
    fi
    
    print_success "Deploy concluído! 🚀"
}

# === Execução Principal ===
main() {
    local start_time=$(date +%s)
    
    # Banner
    print_header "MeAjudaAi Infrastructure Deployment"
    
    check_prerequisites
    ensure_resource_group
    validate_template
    
    if [ "$WHAT_IF" = true ]; then
        run_what_if
        print_info "Análise what-if concluída. Use sem --what-if para executar o deploy."
        exit 0
    fi
    
    deploy_infrastructure
    get_deployment_outputs
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    show_summary
    print_info "Tempo total: ${duration}s"
}

# === Execução ===
main "$@"