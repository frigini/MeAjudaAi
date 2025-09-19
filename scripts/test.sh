#!/bin/bash

# =============================================================================
# MeAjudaAi Test Runner Script - Execução Abrangente de Testes
# =============================================================================
# Script consolidado para execução de diferentes tipos de testes da aplicação.
# Inclui testes unitários, de integração, E2E e otimizações de performance.
# 
# Uso:
#   ./scripts/test.sh [opções]
#
# Opções:
#   -h, --help           Mostra esta ajuda
#   -v, --verbose        Modo verboso
#   -u, --unit           Apenas testes unitários
#   -i, --integration    Apenas testes de integração
#   -e, --e2e           Apenas testes E2E
#   -f, --fast          Modo rápido (com otimizações)
#   -c, --coverage      Gera relatório de cobertura
#   --skip-build        Pula o build
#   --parallel          Executa testes em paralelo
#
# Exemplos:
#   ./scripts/test.sh                   # Todos os testes
#   ./scripts/test.sh --unit            # Apenas unitários
#   ./scripts/test.sh --fast            # Modo otimizado
#   ./scripts/test.sh --coverage        # Com cobertura
#
# Dependências:
#   - .NET 8 SDK
#   - Docker Desktop (para testes de integração)
#   - reportgenerator (para cobertura)
# =============================================================================

set -e  # Para em caso de erro

# === Configurações ===
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
TEST_RESULTS_DIR="$PROJECT_ROOT/TestResults"
COVERAGE_DIR="$PROJECT_ROOT/TestResults/Coverage"

# === Variáveis de Controle ===
VERBOSE=false
UNIT_ONLY=false
INTEGRATION_ONLY=false
E2E_ONLY=false
FAST_MODE=false
COVERAGE=false
SKIP_BUILD=false
PARALLEL=false

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
        -u|--unit)
            UNIT_ONLY=true
            shift
            ;;
        -i|--integration)
            INTEGRATION_ONLY=true
            shift
            ;;
        -e|--e2e)
            E2E_ONLY=true
            shift
            ;;
        -f|--fast)
            FAST_MODE=true
            shift
            ;;
        -c|--coverage)
            COVERAGE=true
            shift
            ;;
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        --parallel)
            PARALLEL=true
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

# === Preparação do Ambiente ===
setup_test_environment() {
    print_header "Preparando Ambiente de Testes"
    
    # Criar diretórios de resultados
    print_verbose "Criando diretórios de resultados..."
    mkdir -p "$TEST_RESULTS_DIR"
    mkdir -p "$COVERAGE_DIR"
    
    # Limpar resultados antigos
    print_verbose "Limpando resultados antigos..."
    rm -rf "$TEST_RESULTS_DIR"/*.trx 2>/dev/null || true
    rm -rf "$COVERAGE_DIR"/* 2>/dev/null || true
    
    # Verificar Docker se necessário
    if [ "$INTEGRATION_ONLY" = true ] || [ "$E2E_ONLY" = true ] || ([ "$UNIT_ONLY" = false ] && [ "$INTEGRATION_ONLY" = false ] && [ "$E2E_ONLY" = false ]); then
        print_verbose "Verificando Docker para testes de integração..."
        if ! docker info &> /dev/null; then
            print_warning "Docker não está rodando. Testes de integração serão pulados."
        else
            print_info "Docker disponível para testes de integração."
        fi
    fi
    
    print_info "Ambiente de testes preparado!"
}

# === Aplicar Otimizações ===
apply_optimizations() {
    if [ "$FAST_MODE" = true ]; then
        print_header "Aplicando Otimizações de Performance"
        
        print_info "Configurando variáveis de ambiente para otimização..."
        
        # Configurações Docker/TestContainers
        export DOCKER_HOST="npipe://./pipe/docker_engine"
        export TESTCONTAINERS_RYUK_DISABLED=true
        export TESTCONTAINERS_CHECKS_DISABLE=true
        export TESTCONTAINERS_WAIT_STRATEGY_RETRIES=1
        
        # Configurações .NET para testes
        export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
        export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
        export DOTNET_CLI_TELEMETRY_OPTOUT=1
        export DOTNET_RUNNING_IN_CONTAINER=1
        export ASPNETCORE_ENVIRONMENT=Testing
        export COMPlus_EnableDiagnostics=0
        export COMPlus_TieredCompilation=0
        export DOTNET_TieredCompilation=0
        export DOTNET_ReadyToRun=0
        export DOTNET_TC_QuickJitForLoops=1
        
        # Configurações PostgreSQL para testes
        export POSTGRES_SHARED_PRELOAD_LIBRARIES=""
        export POSTGRES_LOGGING_COLLECTOR=off
        export POSTGRES_LOG_STATEMENT=none
        export POSTGRES_LOG_DURATION=off
        export POSTGRES_LOG_CHECKPOINTS=off
        export POSTGRES_CHECKPOINT_COMPLETION_TARGET=0.9
        export POSTGRES_WAL_BUFFERS=16MB
        export POSTGRES_SHARED_BUFFERS=256MB
        export POSTGRES_EFFECTIVE_CACHE_SIZE=1GB
        export POSTGRES_MAINTENANCE_WORK_MEM=64MB
        export POSTGRES_WORK_MEM=4MB
        export POSTGRES_FSYNC=off
        export POSTGRES_SYNCHRONOUS_COMMIT=off
        export POSTGRES_FULL_PAGE_WRITES=off
        
        print_info "Otimizações aplicadas! Esperado 70%+ de melhoria na performance."
    fi
}

# === Build da Solução ===
build_solution() {
    if [ "$SKIP_BUILD" = true ]; then
        print_info "Pulando build..."
        return 0
    fi

    print_header "Compilando Solução para Testes"
    
    print_info "Restaurando dependências..."
    dotnet restore
    
    print_info "Compilando em modo Release..."
    if [ "$VERBOSE" = true ]; then
        dotnet build --no-restore --configuration Release --verbosity normal
    else
        dotnet build --no-restore --configuration Release --verbosity minimal
    fi
    
    if [ $? -eq 0 ]; then
        print_info "Build concluído com sucesso!"
    else
        print_error "Falha no build. Verifique os erros acima."
        exit 1
    fi
}

# === Testes Unitários ===
run_unit_tests() {
    print_header "Executando Testes Unitários"
    
    local test_args="--no-build --configuration Release"
    test_args="$test_args --filter \"Category!=Integration&Category!=E2E\""
    test_args="$test_args --logger \"trx;LogFileName=unit-tests.trx\""
    test_args="$test_args --results-directory \"$TEST_RESULTS_DIR\""
    
    if [ "$VERBOSE" = true ]; then
        test_args="$test_args --logger \"console;verbosity=normal\""
    else
        test_args="$test_args --logger \"console;verbosity=minimal\""
    fi
    
    if [ "$COVERAGE" = true ]; then
        test_args="$test_args --collect:\"XPlat Code Coverage\""
    fi
    
    if [ "$PARALLEL" = true ]; then
        test_args="$test_args --parallel"
    fi
    
    print_info "Executando testes unitários..."
    eval "dotnet test $test_args"
    
    if [ $? -eq 0 ]; then
        print_info "Testes unitários concluídos com sucesso!"
    else
        print_error "Alguns testes unitários falharam."
        return 1
    fi
}

# === Testes de Integração ===
run_integration_tests() {
    print_header "Executando Testes de Integração"
    
    # Verificar se Docker está disponível
    if ! docker info &> /dev/null; then
        print_warning "Docker não disponível. Pulando testes de integração."
        return 0
    fi
    
    local test_args="--no-build --configuration Release"
    test_args="$test_args --filter \"Category=Integration\""
    test_args="$test_args --logger \"trx;LogFileName=integration-tests.trx\""
    test_args="$test_args --results-directory \"$TEST_RESULTS_DIR\""
    
    if [ "$VERBOSE" = true ]; then
        test_args="$test_args --logger \"console;verbosity=normal\""
    else
        test_args="$test_args --logger \"console;verbosity=minimal\""
    fi
    
    if [ "$COVERAGE" = true ]; then
        test_args="$test_args --collect:\"XPlat Code Coverage\""
    fi
    
    print_info "Executando testes de integração..."
    eval "dotnet test $test_args"
    
    if [ $? -eq 0 ]; then
        print_info "Testes de integração concluídos com sucesso!"
    else
        print_error "Alguns testes de integração falharam."
        return 1
    fi
}

# === Testes E2E ===
run_e2e_tests() {
    print_header "Executando Testes End-to-End"
    
    local test_args="--no-build --configuration Release"
    test_args="$test_args --filter \"Category=E2E\""
    test_args="$test_args --logger \"trx;LogFileName=e2e-tests.trx\""
    test_args="$test_args --results-directory \"$TEST_RESULTS_DIR\""
    
    if [ "$VERBOSE" = true ]; then
        test_args="$test_args --logger \"console;verbosity=normal\""
    else
        test_args="$test_args --logger \"console;verbosity=minimal\""
    fi
    
    print_info "Executando testes E2E..."
    eval "dotnet test $test_args"
    
    if [ $? -eq 0 ]; then
        print_info "Testes E2E concluídos com sucesso!"
    else
        print_error "Alguns testes E2E falharam."
        return 1
    fi
}

# === Gerar Relatório de Cobertura ===
generate_coverage_report() {
    if [ "$COVERAGE" = false ]; then
        return 0
    fi

    print_header "Gerando Relatório de Cobertura"
    
    # Verificar se reportgenerator está instalado
    if ! command -v reportgenerator &> /dev/null; then
        print_warning "reportgenerator não encontrado. Instalando..."
        dotnet tool install --global dotnet-reportgenerator-globaltool
    fi
    
    print_info "Processando arquivos de cobertura..."
    reportgenerator \
        -reports:"$TEST_RESULTS_DIR/**/coverage.cobertura.xml" \
        -targetdir:"$COVERAGE_DIR" \
        -reporttypes:"Html;Cobertura;TextSummary" \
        -verbosity:Warning
    
    if [ $? -eq 0 ]; then
        print_info "Relatório de cobertura gerado em: $COVERAGE_DIR"
        print_info "Abra o arquivo index.html no navegador para visualizar."
    else
        print_warning "Erro ao gerar relatório de cobertura."
    fi
}

# === Relatório de Resultados ===
show_results() {
    print_header "Resultados dos Testes"
    
    # Contar arquivos de resultado
    local trx_files=$(find "$TEST_RESULTS_DIR" -name "*.trx" 2>/dev/null | wc -l)
    
    if [ "$trx_files" -gt 0 ]; then
        print_info "Arquivos de resultado gerados: $trx_files"
        print_info "Localização: $TEST_RESULTS_DIR"
    fi
    
    if [ "$COVERAGE" = true ] && [ -f "$COVERAGE_DIR/index.html" ]; then
        print_info "Relatório de cobertura disponível em: $COVERAGE_DIR/index.html"
    fi
    
    if [ "$FAST_MODE" = true ]; then
        print_info "Modo otimizado usado - performance melhorada!"
    fi
}

# === Execução Principal ===
main() {
    local start_time=$(date +%s)
    local failed_tests=0
    
    setup_test_environment
    apply_optimizations
    build_solution
    
    # Executar testes baseado nas opções
    if [ "$UNIT_ONLY" = true ]; then
        run_unit_tests || failed_tests=$((failed_tests + 1))
    elif [ "$INTEGRATION_ONLY" = true ]; then
        run_integration_tests || failed_tests=$((failed_tests + 1))
    elif [ "$E2E_ONLY" = true ]; then
        run_e2e_tests || failed_tests=$((failed_tests + 1))
    else
        # Executar todos os tipos de teste
        run_unit_tests || failed_tests=$((failed_tests + 1))
        run_integration_tests || failed_tests=$((failed_tests + 1))
        run_e2e_tests || failed_tests=$((failed_tests + 1))
    fi
    
    generate_coverage_report
    show_results
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    print_header "Resumo da Execução"
    print_info "Tempo total: ${duration}s"
    
    if [ "$failed_tests" -eq 0 ]; then
        print_info "Todos os testes foram executados com sucesso! 🎉"
        exit 0
    else
        print_error "Alguns conjuntos de testes falharam. Total: $failed_tests"
        exit 1
    fi
}

# === Execução ===
main "$@"