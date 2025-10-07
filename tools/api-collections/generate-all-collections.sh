#!/bin/bash

# Script para gerar todas as collections da API MeAjudaAi
# Uso: ./generate-all-collections.sh [ambiente]

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
PROJECT_ROOT="$(dirname "$(dirname "$SCRIPT_DIR")")"

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para log colorido
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

warn() {
    echo -e "${YELLOW}[WARN] $1${NC}"
}

error() {
    echo -e "${RED}[ERROR] $1${NC}"
}

info() {
    echo -e "${BLUE}[INFO] $1${NC}"
}

# Verificar dependências
check_dependencies() {
    log "🔍 Verificando dependências..."
    
    if ! command -v node &> /dev/null; then
        error "Node.js não encontrado. Instale Node.js 18+ para continuar."
        exit 1
    fi
    
    if ! command -v dotnet &> /dev/null; then
        error ".NET não encontrado. Instale .NET 8+ para continuar."
        exit 1
    fi
    
    local node_version=$(node --version | cut -d'v' -f2 | cut -d'.' -f1)
    if [ "$node_version" -lt 18 ]; then
        error "Node.js versão 18+ é necessário. Versão atual: $(node --version)"
        exit 1
    fi
    
    info "✅ Node.js $(node --version) encontrado"
    info "✅ .NET $(dotnet --version) encontrado"
}

# Instalar dependências Node.js se necessário
install_dependencies() {
    log "📦 Verificando dependências do gerador..."
    
    cd "$SCRIPT_DIR"
    
    if [ ! -f "package.json" ]; then
        error "package.json não encontrado em $SCRIPT_DIR"
        exit 1
    fi
    
    if [ ! -d "node_modules" ]; then
        log "🔄 Instalando dependências npm..."
        npm install
    else
        info "📚 Dependências já instaladas"
    fi
}

# Iniciar API para gerar swagger.json
start_api() {
    log "🚀 Iniciando API para gerar documentação..."
    
    cd "$PROJECT_ROOT"
    
    # Verificar se a API já está rodando
    if curl -s "http://localhost:5000/health" > /dev/null 2>&1; then
        info "✅ API já está rodando em http://localhost:5000"
        return 0
    fi
    
    # Iniciar API em background
    log "⏳ Compilando e iniciando API..."
    cd "$PROJECT_ROOT/src/Bootstrapper/MeAjudaAi.ApiService"
    
    # Build da aplicação
    dotnet build --configuration Release --no-restore
    
    # Iniciar em background
    nohup dotnet run --configuration Release --urls="http://localhost:5000" > /tmp/meajudaai-api.log 2>&1 &
    API_PID=$!
    
    # Aguardar API estar pronta
    local attempts=0
    local max_attempts=30
    
    while [ $attempts -lt $max_attempts ]; do
        if curl -s "http://localhost:5000/health" > /dev/null 2>&1; then
            log "✅ API iniciada com sucesso (PID: $API_PID)"
            echo $API_PID > /tmp/meajudaai-api.pid
            return 0
        fi
        
        info "⏳ Aguardando API iniciar... (tentativa $((attempts+1))/$max_attempts)"
        sleep 2
        attempts=$((attempts+1))
    done
    
    error "❌ Timeout ao iniciar API após $max_attempts tentativas"
    error "Verifique os logs em /tmp/meajudaai-api.log"
    exit 1
}

# Gerar Postman Collections
generate_postman() {
    log "📋 Gerando Postman Collections..."
    
    cd "$SCRIPT_DIR"
    node generate-postman-collections.js
    
    if [ $? -eq 0 ]; then
        log "✅ Postman Collections geradas com sucesso!"
    else
        error "❌ Erro ao gerar Postman Collections"
        return 1
    fi
}

# Gerar outras collections (Insomnia, etc.) - futuro
generate_other_collections() {
    log "🔄 Gerando outras collections..."
    
    # TODO: Implementar geração de Insomnia collections
    # TODO: Implementar geração de Thunder Client collections
    
    info "ℹ️  Outros formatos serão implementados em versões futuras"
}

# Validar collections geradas
validate_collections() {
    log "🔍 Validando collections geradas..."
    
    local output_dir="$PROJECT_ROOT/src/Shared/API.Collections/Generated"
    
    if [ ! -d "$output_dir" ]; then
        error "Diretório de output não encontrado: $output_dir"
        return 1
    fi
    
    local collection_file="$output_dir/MeAjudaAi-API-Collection.json"
    if [ ! -f "$collection_file" ]; then
        error "Collection principal não encontrada: $collection_file"
        return 1
    fi
    
    # Validar JSON
    if ! cat "$collection_file" | jq empty > /dev/null 2>&1; then
        if command -v jq &> /dev/null; then
            error "Collection JSON é inválida"
            return 1
        else
            warn "jq não encontrado, pulando validação JSON"
        fi
    fi
    
    # Contar endpoints
    local endpoint_count=0
    if command -v jq &> /dev/null; then
        endpoint_count=$(cat "$collection_file" | jq '[.item[] | .item[]? // .] | length')
        info "📊 Collection gerada com $endpoint_count endpoints"
    fi
    
    log "✅ Collections validadas com sucesso!"
}

# Parar API se foi iniciada por este script
cleanup() {
    if [ -f "/tmp/meajudaai-api.pid" ]; then
        local pid=$(cat /tmp/meajudaai-api.pid)
        log "🔄 Parando API (PID: $pid)..."
        kill $pid > /dev/null 2>&1 || true
        rm -f /tmp/meajudaai-api.pid
        log "✅ API parada"
    fi
}

# Exibir resultados
show_results() {
    log "🎉 Geração de collections concluída!"
    
    local output_dir="$PROJECT_ROOT/src/Shared/API.Collections/Generated"
    
    echo ""
    info "📁 Arquivos gerados em: $output_dir"
    echo ""
    
    if [ -d "$output_dir" ]; then
        ls -la "$output_dir" | grep -E '\.(json|md)$' | while read -r line; do
            local filename=$(echo "$line" | awk '{print $NF}')
            local size=$(echo "$line" | awk '{print $5}')
            info "  📄 $filename ($size bytes)"
        done
    fi
    
    echo ""
    info "📖 Como usar:"
    info "  1. Importe os arquivos .json no Postman"
    info "  2. Configure o ambiente desejado (development/staging/production)"
    info "  3. Execute 'Get Keycloak Token' para autenticar"
    info "  4. Execute 'Health Check' para testar conectividade"
    echo ""
    info "🔄 Para regenerar: $0"
    echo ""
}

# Função principal
main() {
    log "🚀 Iniciando geração de API Collections - MeAjudaAi"
    echo ""
    
    # Trap para limpeza em caso de interrupção
    trap cleanup EXIT INT TERM
    
    check_dependencies
    install_dependencies
    start_api
    generate_postman
    generate_other_collections
    validate_collections
    show_results
    
    log "✨ Processo concluído com sucesso!"
}

# Verificar se está sendo executado diretamente
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi