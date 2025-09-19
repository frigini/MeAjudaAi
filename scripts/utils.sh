#!/bin/bash

# =============================================================================
# MeAjudaAi Shared Utilities - Funções Comuns para Scripts
# =============================================================================
# Biblioteca de funções compartilhadas entre os scripts do projeto.
# Inclui logging, validações, configurações e helpers comuns.
# 
# Uso:
#   source ./scripts/utils.sh
#   ou
#   . ./scripts/utils.sh
#
# Funções disponíveis:
#   - Logging: print_*, log_*
#   - Validações: check_*, validate_*
#   - Sistema: detect_*, get_*
#   - Configuração: load_*, save_*
#   - Docker: docker_*, container_*
#   - .NET: dotnet_*, nuget_*
# =============================================================================

# === Verificar se já foi carregado ===
if [ "${MEAJUDAAI_UTILS_LOADED:-}" = "true" ]; then
    return 0 2>/dev/null || true
    exit 0
fi

# === Configurações Globais ===
MEAJUDAAI_UTILS_LOADED=true
UTILS_SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UTILS_PROJECT_ROOT="$(cd "$UTILS_SCRIPT_DIR/.." && pwd)"

# === Cores para output ===
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# === Níveis de Log ===
LOG_LEVEL_ERROR=1
LOG_LEVEL_WARN=2
LOG_LEVEL_INFO=3
LOG_LEVEL_DEBUG=4
LOG_LEVEL_VERBOSE=5

# Nível padrão (pode ser sobrescrito por variável de ambiente)
CURRENT_LOG_LEVEL=${MEAJUDAAI_LOG_LEVEL:-$LOG_LEVEL_INFO}

# ============================================================================
# FUNÇÕES DE LOGGING
# ============================================================================

# === Logging com timestamp ===
log_with_timestamp() {
    local level="$1"
    local message="$2"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo -e "${timestamp} [${level}] ${message}"
}

# === Print functions (sem timestamp) ===
print_header() {
    echo -e "${BLUE}===================================================================${NC}"
    echo -e "${BLUE} $1${NC}"
    echo -e "${BLUE}===================================================================${NC}"
}

print_subheader() {
    echo -e "${CYAN}--- $1 ---${NC}"
}

print_info() {
    if [ "$CURRENT_LOG_LEVEL" -ge "$LOG_LEVEL_INFO" ]; then
        echo -e "${GREEN}✅ $1${NC}"
    fi
}

print_success() {
    echo -e "${GREEN}🎉 $1${NC}"
}

print_warning() {
    if [ "$CURRENT_LOG_LEVEL" -ge "$LOG_LEVEL_WARN" ]; then
        echo -e "${YELLOW}⚠️  $1${NC}"
    fi
}

print_error() {
    if [ "$CURRENT_LOG_LEVEL" -ge "$LOG_LEVEL_ERROR" ]; then
        echo -e "${RED}❌ $1${NC}" >&2
    fi
}

print_debug() {
    if [ "$CURRENT_LOG_LEVEL" -ge "$LOG_LEVEL_DEBUG" ]; then
        echo -e "${CYAN}🔍 $1${NC}"
    fi
}

print_verbose() {
    if [ "$CURRENT_LOG_LEVEL" -ge "$LOG_LEVEL_VERBOSE" ]; then
        echo -e "${MAGENTA}📝 $1${NC}"
    fi
}

print_step() {
    echo -e "${BLUE}🔧 $1${NC}"
}

print_progress() {
    echo -e "${WHITE}⏳ $1${NC}"
}

# === Log functions (com timestamp) ===
log_info() {
    if [ "$CURRENT_LOG_LEVEL" -ge "$LOG_LEVEL_INFO" ]; then
        log_with_timestamp "INFO" "${GREEN}$1${NC}"
    fi
}

log_warning() {
    if [ "$CURRENT_LOG_LEVEL" -ge "$LOG_LEVEL_WARN" ]; then
        log_with_timestamp "WARN" "${YELLOW}$1${NC}"
    fi
}

log_error() {
    if [ "$CURRENT_LOG_LEVEL" -ge "$LOG_LEVEL_ERROR" ]; then
        log_with_timestamp "ERROR" "${RED}$1${NC}" >&2
    fi
}

log_debug() {
    if [ "$CURRENT_LOG_LEVEL" -ge "$LOG_LEVEL_DEBUG" ]; then
        log_with_timestamp "DEBUG" "${CYAN}$1${NC}"
    fi
}

# ============================================================================
# FUNÇÕES DE VALIDAÇÃO E VERIFICAÇÃO
# ============================================================================

# === Verificar se comando existe ===
command_exists() {
    command -v "$1" &> /dev/null
}

# === Verificar se arquivo existe ===
file_exists() {
    [ -f "$1" ]
}

# === Verificar se diretório existe ===
dir_exists() {
    [ -d "$1" ]
}

# === Verificar dependências essenciais ===
check_essential_dependencies() {
    local missing_deps=()
    
    if ! command_exists dotnet; then
        missing_deps+=(".NET SDK")
    fi
    
    if ! command_exists git; then
        missing_deps+=("Git")
    fi
    
    if [ ${#missing_deps[@]} -gt 0 ]; then
        print_error "Dependências essenciais não encontradas:"
        for dep in "${missing_deps[@]}"; do
            print_error "  - $dep"
        done
        return 1
    fi
    
    return 0
}

# === Verificar dependências opcionais ===
check_optional_dependencies() {
    local warnings=()
    
    if ! command_exists docker; then
        warnings+=("Docker não encontrado - testes de integração não funcionarão")
    fi
    
    if ! command_exists az; then
        warnings+=("Azure CLI não encontrado - deploy não funcionará")
    fi
    
    if ! command_exists code; then
        warnings+=("VS Code não encontrado")
    fi
    
    if [ ${#warnings[@]} -gt 0 ]; then
        print_warning "Dependências opcionais não encontradas:"
        for warning in "${warnings[@]}"; do
            print_warning "  - $warning"
        done
    fi
}

# === Validar argumentos ===
validate_argument() {
    local arg_name="$1"
    local arg_value="$2"
    local required="${3:-true}"
    
    if [ "$required" = "true" ] && [ -z "$arg_value" ]; then
        print_error "Argumento obrigatório não fornecido: $arg_name"
        return 1
    fi
    
    return 0
}

# === Validar ambiente ===
validate_environment() {
    local env="$1"
    case $env in
        dev|development|prod|production)
            return 0
            ;;
        *)
            print_error "Ambiente inválido: $env"
            print_error "Ambientes válidos: dev, development, prod, production"
            return 1
            ;;
    esac
}

# ============================================================================
# FUNÇÕES DE SISTEMA
# ============================================================================

# === Detectar sistema operacional ===
detect_os() {
    local os_type=""
    
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        os_type="linux"
        if command_exists lsb_release; then
            DISTRO=$(lsb_release -si 2>/dev/null)
        else
            DISTRO="Unknown"
        fi
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        os_type="macos"
        DISTRO="macOS"
    elif [[ "$OSTYPE" == "cygwin" ]] || [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "win32" ]]; then
        os_type="windows"
        DISTRO="Windows"
    else
        os_type="unknown"
        DISTRO="Unknown"
    fi
    
    export OS_TYPE="$os_type"
    export OS_DISTRO="$DISTRO"
    
    print_debug "Sistema detectado: $os_type ($DISTRO)"
    return 0
}

# === Obter informações do sistema ===
get_system_info() {
    detect_os
    
    # CPU info
    if command_exists nproc; then
        CPU_CORES=$(nproc)
    elif command_exists sysctl; then
        CPU_CORES=$(sysctl -n hw.ncpu)
    else
        CPU_CORES=1
    fi
    
    # Memory info (em GB)
    if [ "$OS_TYPE" = "linux" ]; then
        MEMORY_GB=$(free -g | awk 'NR==2{print $2}')
    elif [ "$OS_TYPE" = "macos" ]; then
        MEMORY_BYTES=$(sysctl -n hw.memsize)
        MEMORY_GB=$((MEMORY_BYTES / 1024 / 1024 / 1024))
    else
        MEMORY_GB=8  # Default fallback
    fi
    
    export CPU_CORES
    export MEMORY_GB
    
    print_debug "CPU Cores: $CPU_CORES, Memory: ${MEMORY_GB}GB"
}

# === Obter caminho absoluto ===
get_absolute_path() {
    local path="$1"
    
    if [ -d "$path" ]; then
        (cd "$path" && pwd)
    elif [ -f "$path" ]; then
        echo "$(cd "$(dirname "$path")" && pwd)/$(basename "$path")"
    else
        echo "$path"
    fi
}

# ============================================================================
# FUNÇÕES DE CONFIGURAÇÃO
# ============================================================================

# === Carregar configuração do projeto ===
load_project_config() {
    local config_file="$UTILS_PROJECT_ROOT/.meajudaai.config"
    
    if [ -f "$config_file" ]; then
        source "$config_file"
        print_debug "Configuração carregada de: $config_file"
    else
        print_debug "Arquivo de configuração não encontrado: $config_file"
    fi
}

# === Salvar configuração ===
save_project_config() {
    local config_file="$UTILS_PROJECT_ROOT/.meajudaai.config"
    local key="$1"
    local value="$2"
    
    # Criar arquivo se não existir
    touch "$config_file"
    
    # Remover linha existente e adicionar nova
    grep -v "^$key=" "$config_file" > "${config_file}.tmp" 2>/dev/null || true
    echo "$key=$value" >> "${config_file}.tmp"
    mv "${config_file}.tmp" "$config_file"
    
    print_debug "Configuração salva: $key=$value"
}

# === Obter configuração ===
get_config() {
    local key="$1"
    local default="$2"
    local config_file="$UTILS_PROJECT_ROOT/.meajudaai.config"
    
    if [ -f "$config_file" ]; then
        local value=$(grep "^$key=" "$config_file" | cut -d'=' -f2-)
        echo "${value:-$default}"
    else
        echo "$default"
    fi
}

# ============================================================================
# FUNÇÕES DOCKER
# ============================================================================

# === Verificar se Docker está rodando ===
docker_is_running() {
    docker info &> /dev/null
}

# === Obter containers rodando ===
docker_list_containers() {
    local filter="$1"
    
    if [ -n "$filter" ]; then
        docker ps --filter "$filter" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    else
        docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    fi
}

# === Parar containers por pattern ===
docker_stop_containers() {
    local pattern="$1"
    
    if [ -z "$pattern" ]; then
        print_error "Pattern é obrigatório para docker_stop_containers"
        return 1
    fi
    
    local containers=$(docker ps --filter "name=$pattern" --format "{{.Names}}")
    
    if [ -n "$containers" ]; then
        print_info "Parando containers: $containers"
        echo "$containers" | xargs docker stop
    else
        print_info "Nenhum container encontrado com pattern: $pattern"
    fi
}

# === Limpar containers e volumes ===
docker_cleanup() {
    print_step "Limpando containers e volumes Docker..."
    
    # Parar containers
    docker stop $(docker ps -q) 2>/dev/null || true
    
    # Remover containers
    docker rm $(docker ps -aq) 2>/dev/null || true
    
    # Remover volumes não utilizados
    docker volume prune -f 2>/dev/null || true
    
    print_info "Limpeza Docker concluída"
}

# ============================================================================
# FUNÇÕES .NET
# ============================================================================

# === Verificar versão do .NET ===
dotnet_get_version() {
    if command_exists dotnet; then
        dotnet --version
    else
        echo "not-installed"
    fi
}

# === Verificar se projeto é válido ===
dotnet_is_valid_project() {
    local project_path="$1"
    
    if [ -f "$project_path" ] && [[ "$project_path" == *.csproj ]]; then
        return 0
    elif [ -d "$project_path" ] && find "$project_path" -name "*.csproj" -maxdepth 1 | grep -q .; then
        return 0
    else
        return 1
    fi
}

# === Build projeto com configuração ===
dotnet_build_project() {
    local project="$1"
    local configuration="${2:-Release}"
    local verbosity="${3:-minimal}"
    
    print_step "Building projeto: $project"
    
    dotnet build "$project" \
        --configuration "$configuration" \
        --verbosity "$verbosity" \
        --no-restore
}

# === Executar testes com filtros ===
dotnet_run_tests() {
    local project="$1"
    local filter="$2"
    local configuration="${3:-Release}"
    
    local test_args="--no-build --configuration $configuration"
    
    if [ -n "$filter" ]; then
        test_args="$test_args --filter \"$filter\""
    fi
    
    print_step "Executando testes: $project"
    eval "dotnet test \"$project\" $test_args"
}

# ============================================================================
# FUNÇÕES DE REDE E PORTAS
# ============================================================================

# === Verificar se porta está em uso ===
port_is_in_use() {
    local port="$1"
    
    if command_exists netstat; then
        netstat -an | grep ":$port " > /dev/null
    elif command_exists ss; then
        ss -an | grep ":$port " > /dev/null
    else
        return 1
    fi
}

# === Encontrar porta livre ===
find_free_port() {
    local start_port="${1:-3000}"
    local max_port="${2:-65535}"
    
    for port in $(seq $start_port $max_port); do
        if ! port_is_in_use "$port"; then
            echo "$port"
            return 0
        fi
    done
    
    return 1
}

# ============================================================================
# FUNÇÕES DE TEMPO E PERFORMANCE
# ============================================================================

# === Medir tempo de execução ===
time_start() {
    TIMER_START=$(date +%s)
}

time_end() {
    local start_time="${TIMER_START:-$(date +%s)}"
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    echo "$duration"
}

# === Formatar duração ===
format_duration() {
    local seconds="$1"
    
    if [ "$seconds" -lt 60 ]; then
        echo "${seconds}s"
    elif [ "$seconds" -lt 3600 ]; then
        local minutes=$((seconds / 60))
        local remaining_seconds=$((seconds % 60))
        echo "${minutes}m ${remaining_seconds}s"
    else
        local hours=$((seconds / 3600))
        local remaining_minutes=$(((seconds % 3600) / 60))
        echo "${hours}h ${remaining_minutes}m"
    fi
}

# ============================================================================
# FUNÇÕES DE CLEANUP E FINALIZAÇÃO
# ============================================================================

# === Cleanup automático ===
cleanup_on_exit() {
    local cleanup_function="$1"
    
    trap "$cleanup_function" EXIT INT TERM
}

# === Remover arquivos temporários ===
cleanup_temp_files() {
    local temp_pattern="${1:-/tmp/meajudaai_*}"
    
    print_debug "Removendo arquivos temporários: $temp_pattern"
    rm -f $temp_pattern 2>/dev/null || true
}

# ============================================================================
# INICIALIZAÇÃO
# ============================================================================

# === Inicializar utils ===
utils_init() {
    # Detectar sistema
    detect_os
    
    # Carregar configuração do projeto
    load_project_config
    
    print_debug "MeAjudaAi Utils inicializado"
}

# === Auto-inicialização (se não foi explicitamente desabilitada) ===
if [ "${MEAJUDAAI_UTILS_AUTO_INIT:-true}" = "true" ]; then
    utils_init
fi

# === Exportar funções principais ===
export -f print_header print_info print_success print_warning print_error print_debug print_verbose print_step
export -f command_exists file_exists dir_exists check_essential_dependencies
export -f detect_os get_system_info
export -f docker_is_running docker_cleanup
export -f dotnet_get_version dotnet_build_project
export -f time_start time_end format_duration

# === Marcar como carregado ===
print_debug "MeAjudaAi Utilities carregado com sucesso! 🛠️"