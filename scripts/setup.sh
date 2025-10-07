#!/bin/bash

# =============================================================================
# MeAjudaAi Project Setup Script - Onboarding para Novos Desenvolvedores
# =============================================================================
# Script completo para configuração inicial do ambiente de desenvolvimento.
# Instala dependências, configura ferramentas e prepara o ambiente local.
# 
# Uso:
#   ./scripts/setup.sh [opções]
#
# Opções:
#   -h, --help           Mostra esta ajuda
#   -v, --verbose        Modo verboso
#   -s, --skip-install   Pula instalação de dependências
#   -f, --force          Força reinstalação mesmo se já existir
#   --dev-only          Apenas dependências de desenvolvimento
#   --no-docker         Pula verificação e setup do Docker
#   --no-azure          Pula setup do Azure CLI
#
# Exemplos:
#   ./scripts/setup.sh                  # Setup completo
#   ./scripts/setup.sh --dev-only       # Apenas ferramentas de dev
#   ./scripts/setup.sh --no-docker      # Sem Docker
#
# Dependências que serão verificadas/instaladas:
#   - .NET 8 SDK
#   - Docker Desktop
#   - Azure CLI
#   - Git
#   - Visual Studio Code (opcional)
#   - PowerShell (Windows)
# =============================================================================

set -e  # Para em caso de erro

# === Configurações ===
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# === Variáveis de Controle ===
VERBOSE=false
SKIP_INSTALL=false
FORCE=false
DEV_ONLY=false
NO_DOCKER=false
NO_AZURE=false

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

print_step() {
    echo -e "${BLUE}🔧 $1${NC}"
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
        -s|--skip-install)
            SKIP_INSTALL=true
            shift
            ;;
        -f|--force)
            FORCE=true
            shift
            ;;
        --dev-only)
            DEV_ONLY=true
            shift
            ;;
        --no-docker)
            NO_DOCKER=true
            shift
            ;;
        --no-azure)
            NO_AZURE=true
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

# === Detectar Sistema Operacional ===
detect_os() {
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        OS="linux"
        DISTRO=$(lsb_release -si 2>/dev/null || echo "Unknown")
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        OS="macos"
    elif [[ "$OSTYPE" == "cygwin" ]] || [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "win32" ]]; then
        OS="windows"
    else
        OS="unknown"
    fi
    
    print_verbose "Sistema operacional detectado: $OS"
}

# === Verificar se comando existe ===
command_exists() {
    command -v "$1" &> /dev/null
}

# === Verificar e instalar .NET ===
setup_dotnet() {
    print_step "Verificando .NET SDK..."
    
    if command_exists dotnet; then
        local dotnet_version=$(dotnet --version)
        print_info ".NET SDK já instalado: $dotnet_version"
        
        # Verificar se é versão 8.x
        if [[ $dotnet_version == 8.* ]]; then
            print_info "Versão .NET 8 detectada ✓"
        else
            print_warning "Versão .NET $dotnet_version detectada. Recomendado: 8.x"
        fi
    else
        if [ "$SKIP_INSTALL" = true ]; then
            print_error ".NET SDK não encontrado e instalação foi pulada"
            return 1
        fi
        
        print_warning ".NET SDK não encontrado. Instalando..."
        
        case $OS in
            "windows")
                print_info "Baixe e instale o .NET 8 SDK de: https://dotnet.microsoft.com/download"
                print_warning "Reinicie o terminal após a instalação"
                ;;
            "macos")
                if command_exists brew; then
                    brew install --cask dotnet
                else
                    print_info "Instale o Homebrew e execute: brew install --cask dotnet"
                fi
                ;;
            "linux")
                print_info "Para Ubuntu/Debian:"
                print_info "  wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb"
                print_info "  sudo dpkg -i packages-microsoft-prod.deb"
                print_info "  sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0"
                ;;
        esac
    fi
}

# === Verificar e configurar Git ===
setup_git() {
    print_step "Verificando Git..."
    
    if command_exists git; then
        local git_version=$(git --version)
        print_info "Git já instalado: $git_version"
        
        # Verificar configuração
        local git_name=$(git config --global user.name 2>/dev/null || echo "")
        local git_email=$(git config --global user.email 2>/dev/null || echo "")
        
        if [ -z "$git_name" ] || [ -z "$git_email" ]; then
            print_warning "Configuração do Git incompleta"
            print_info "Configure com: git config --global user.name 'Seu Nome'"
            print_info "Configure com: git config --global user.email 'seu@email.com'"
        else
            print_info "Git configurado para: $git_name <$git_email>"
        fi
    else
        print_error "Git não encontrado. Instale o Git primeiro."
        case $OS in
            "windows")
                print_info "Baixe de: https://git-scm.com/download/win"
                ;;
            "macos")
                print_info "Execute: brew install git"
                ;;
            "linux")
                print_info "Execute: sudo apt-get install git"
                ;;
        esac
        return 1
    fi
}

# === Verificar e configurar Docker ===
setup_docker() {
    if [ "$NO_DOCKER" = true ]; then
        print_warning "Setup do Docker foi pulado"
        return 0
    fi

    print_step "Verificando Docker..."
    
    if command_exists docker; then
        print_info "Docker já instalado"
        
        # Verificar se está rodando
        if docker info &> /dev/null; then
            print_info "Docker está rodando ✓"
        else
            print_warning "Docker está instalado mas não está rodando"
            print_info "Inicie o Docker Desktop"
        fi
    else
        if [ "$SKIP_INSTALL" = true ]; then
            print_warning "Docker não encontrado e instalação foi pulada"
            return 0
        fi
        
        print_warning "Docker não encontrado"
        case $OS in
            "windows"|"macos")
                print_info "Baixe e instale o Docker Desktop de: https://www.docker.com/products/docker-desktop"
                ;;
            "linux")
                print_info "Para Ubuntu/Debian:"
                print_info "  curl -fsSL https://get.docker.com -o get-docker.sh"
                print_info "  sudo sh get-docker.sh"
                print_info "  sudo usermod -aG docker \$USER"
                ;;
        esac
    fi
}

# === Verificar e configurar Azure CLI ===
setup_azure_cli() {
    if [ "$NO_AZURE" = true ] || [ "$DEV_ONLY" = true ]; then
        print_warning "Setup do Azure CLI foi pulado"
        return 0
    fi

    print_step "Verificando Azure CLI..."
    
    if command_exists az; then
        local az_version=$(az --version 2>/dev/null | head -n1 || echo "Unknown")
        print_info "Azure CLI já instalado: $az_version"
        
        # Verificar autenticação
        if az account show &> /dev/null; then
            local subscription=$(az account show --query name -o tsv)
            print_info "Autenticado na subscription: $subscription"
        else
            print_warning "Azure CLI não está autenticado"
            print_info "Execute: az login"
        fi
    else
        if [ "$SKIP_INSTALL" = true ]; then
            print_warning "Azure CLI não encontrado e instalação foi pulada"
            return 0
        fi
        
        print_warning "Azure CLI não encontrado"
        case $OS in
            "windows")
                print_info "Baixe de: https://aka.ms/installazurecliwindows"
                ;;
            "macos")
                print_info "Execute: brew install azure-cli"
                ;;
            "linux")
                print_info "Execute: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash"
                ;;
        esac
    fi
}

# === Configurar Visual Studio Code ===
setup_vscode() {
    print_step "Verificando Visual Studio Code..."
    
    if command_exists code; then
        print_info "Visual Studio Code já instalado"
        
        # Sugerir extensões úteis
        print_info "Extensões recomendadas para o projeto:"
        print_info "  - C# (ms-dotnettools.csharp)"
        print_info "  - Docker (ms-azuretools.vscode-docker)"
        print_info "  - Azure Tools (ms-vscode.vscode-node-azure-pack)"
        print_info "  - REST Client (humao.rest-client)"
        print_info "  - GitLens (eamodio.gitlens)"
    else
        print_warning "Visual Studio Code não encontrado (opcional)"
        print_info "Baixe de: https://code.visualstudio.com/"
    fi
}

# === Configurar ambiente do projeto ===
setup_project_environment() {
    print_step "Configurando ambiente do projeto..."
    
    # Restaurar dependências
    print_info "Restaurando dependências .NET..."
    dotnet restore
    
    # Verificar se build funciona
    print_info "Testando build inicial..."
    dotnet build --configuration Debug --verbosity minimal
    
    if [ $? -eq 0 ]; then
        print_info "Build inicial bem-sucedido ✓"
    else
        print_error "Falha no build inicial"
        return 1
    fi
    
    # Configurar user secrets (se necessário)
    print_info "Configurando user secrets..."
    local api_project="src/Bootstrapper/MeAjudaAi.ApiService"
    if [ -d "$api_project" ]; then
        cd "$api_project"
        dotnet user-secrets init 2>/dev/null || true
        cd "$PROJECT_ROOT"
        print_info "User secrets inicializados"
    fi
    
    # Criar arquivos de configuração local se não existirem
    local env_example=".env.example"
    local env_local=".env.local"
    
    if [ -f "$env_example" ] && [ ! -f "$env_local" ]; then
        cp "$env_example" "$env_local"
        print_info "Arquivo .env.local criado a partir do exemplo"
        print_warning "Edite .env.local com suas configurações específicas"
    fi
}

# === Executar testes básicos ===
run_basic_tests() {
    print_step "Executando testes básicos..."
    
    if [ -d "tests" ]; then
        print_info "Executando testes unitários básicos..."
        dotnet test --configuration Debug --verbosity minimal --filter "Category!=Integration&Category!=E2E"
        
        if [ $? -eq 0 ]; then
            print_info "Testes básicos passaram ✓"
        else
            print_warning "Alguns testes básicos falharam"
        fi
    else
        print_info "Nenhum projeto de teste encontrado"
    fi
}

# === Mostrar próximos passos ===
show_next_steps() {
    print_header "Próximos Passos"
    
    echo "🎉 Setup concluído! Aqui estão os próximos passos:"
    echo ""
    echo "📋 Comandos úteis:"
    echo "  ./scripts/dev.sh              # Executar em modo desenvolvimento"
    echo "  ./scripts/test.sh             # Executar todos os testes"
    echo "  ./scripts/deploy.sh dev       # Deploy para ambiente dev"
    echo ""
    echo "🔧 Configurações adicionais:"
    echo "  - Edite .env.local com suas configurações"
    echo "  - Configure user secrets: dotnet user-secrets set \"key\" \"value\""
    echo "  - Autentique no Azure: az login"
    echo ""
    echo "📚 Documentação:"
    echo "  - README.md do projeto"
    echo "  - scripts/README.md"
    echo "  - docs/ (se disponível)"
    echo ""
    echo "🆘 Problemas? Execute novamente com --verbose para mais detalhes"
}

# === Execução Principal ===
main() {
    local start_time=$(date +%s)
    
    print_header "MeAjudaAi Project Setup"
    print_info "Configurando ambiente de desenvolvimento..."
    
    detect_os
    
    # Verificar dependências essenciais
    setup_git
    setup_dotnet
    
    # Verificar ferramentas opcionais
    setup_docker
    setup_azure_cli
    setup_vscode
    
    # Configurar projeto
    setup_project_environment
    run_basic_tests
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    print_header "Setup Concluído"
    print_info "Tempo total: ${duration}s"
    
    show_next_steps
    
    print_info "Ambiente pronto para desenvolvimento! 🚀"
}

# === Execução ===
main "$@"