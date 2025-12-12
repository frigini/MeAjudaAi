#!/usr/bin/env bash

#
# Seed inicial de dados para ambiente de desenvolvimento
# Popula o banco de dados com dados iniciais para desenvolvimento e testes
#

set -euo pipefail

# Cores
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Funções de output
success() { echo -e "${GREEN}✅ $1${NC}"; }
info() { echo -e "${CYAN}ℹ️  $1${NC}"; }
warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
error() { echo -e "${RED}❌ $1${NC}"; }

# Configuração
ENVIRONMENT="${1:-Development}"
API_BASE_URL="${API_BASE_URL:-http://localhost:5000}"
KEYCLOAK_URL="${KEYCLOAK_URL:-http://localhost:8080}"

echo -e "${CYAN}🌱 Seed de Dados - MeAjudaAi [$ENVIRONMENT]${NC}"
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""

# Verificar se API está rodando
info "Verificando API em $API_BASE_URL..."
if curl -sf "$API_BASE_URL/health" > /dev/null 2>&1; then
    success "API está rodando"
else
    error "API não está acessível em $API_BASE_URL"
    echo "Inicie a API primeiro: cd src/Bootstrapper/MeAjudaAi.ApiService && dotnet run"
    exit 1
fi

# Obter token de autenticação
info "Obtendo token de autenticação..."
TOKEN_RESPONSE=$(curl -sf -X POST "$KEYCLOAK_URL/realms/meajudaai/protocol/openid-connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "client_id=meajudaai-api" \
    -d "username=admin" \
    -d "password=admin123" \
    -d "grant_type=password" \
    2>/dev/null || echo "")

if [ -z "$TOKEN_RESPONSE" ]; then
    error "Falha ao obter token do Keycloak"
    echo "Verifique se Keycloak está rodando: docker-compose up keycloak"
    exit 1
fi

TOKEN=$(echo "$TOKEN_RESPONSE" | jq -r '.access_token')
success "Token obtido com sucesso"

# Headers comuns
HEADERS=(
    -H "Authorization: Bearer $TOKEN"
    -H "Content-Type: application/json"
    -H "Api-Version: 1.0"
)

echo ""
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${YELLOW}📦 Seeding: ServiceCatalogs${NC}"
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

# Criar categorias
declare -A CATEGORY_IDS

create_category() {
    local name="$1"
    local description="$2"
    
    info "Criando categoria: $name"
    
    local response=$(curl -sf -X POST "$API_BASE_URL/api/v1/catalogs/admin/categories" \
        "${HEADERS[@]}" \
        -d "{\"name\":\"$name\",\"description\":\"$description\"}" \
        2>/dev/null || echo "")
    
    if [ -n "$response" ]; then
        local id=$(echo "$response" | jq -r '.id')
        CATEGORY_IDS[$name]=$id
        success "Categoria '$name' criada (ID: $id)"
    else
        warning "Categoria '$name' já existe ou erro ao criar"
    fi
}

create_category "Saúde" "Serviços relacionados à saúde e bem-estar"
create_category "Educação" "Serviços educacionais e de capacitação"
create_category "Assistência Social" "Programas de assistência e suporte social"
create_category "Jurídico" "Serviços jurídicos e advocatícios"
create_category "Habitação" "Moradia e programas habitacionais"
create_category "Alimentação" "Programas de segurança alimentar"

# Criar serviços
create_service() {
    local name="$1"
    local description="$2"
    local category_name="$3"
    local criteria="$4"
    local docs="$5"
    
    if [ -z "${CATEGORY_IDS[$category_name]:-}" ]; then
        warning "Categoria '$category_name' não encontrada, pulando serviço '$name'"
        return
    fi
    
    local category_id="${CATEGORY_IDS[$category_name]}"
    
    info "Criando serviço: $name"
    
    curl -sf -X POST "$API_BASE_URL/api/v1/catalogs/admin/services" \
        "${HEADERS[@]}" \
        -d "{
            \"name\":\"$name\",
            \"description\":\"$description\",
            \"categoryId\":\"$category_id\",
            \"eligibilityCriteria\":\"$criteria\",
            \"requiredDocuments\":$docs
        }" > /dev/null 2>&1 && \
    success "Serviço '$name' criado" || \
    warning "Serviço '$name' já existe ou erro ao criar"
}

create_service "Atendimento Psicológico Gratuito" \
    "Atendimento psicológico individual ou em grupo" \
    "Saúde" \
    "Renda familiar até 3 salários mínimos" \
    '["RG","CPF","Comprovante de residência","Comprovante de renda"]'

create_service "Curso de Informática Básica" \
    "Curso gratuito de informática e inclusão digital" \
    "Educação" \
    "Jovens de 14 a 29 anos" \
    '["RG","CPF","Comprovante de escolaridade"]'

create_service "Cesta Básica" \
    "Distribuição mensal de cestas básicas" \
    "Alimentação" \
    "Famílias em situação de vulnerabilidade" \
    '["Cadastro único","Comprovante de residência"]'

create_service "Orientação Jurídica Gratuita" \
    "Atendimento jurídico para questões civis e trabalhistas" \
    "Jurídico" \
    "Renda familiar até 2 salários mínimos" \
    '["RG","CPF","Documentos relacionados ao caso"]'

echo ""
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${YELLOW}📍 Seeding: Locations (AllowedCities)${NC}"
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

create_city() {
    local ibge_code="$1"
    local city_name="$2"
    local state="$3"
    
    info "Adicionando cidade: $city_name/$state"
    
    curl -sf -X POST "$API_BASE_URL/api/v1/locations/admin/allowed-cities" \
        "${HEADERS[@]}" \
        -d "{
            \"ibgeCode\":\"$ibge_code\",
            \"cityName\":\"$city_name\",
            \"state\":\"$state\",
            \"isActive\":true
        }" > /dev/null 2>&1 && \
    success "Cidade '$city_name/$state' adicionada" || \
    warning "Cidade '$city_name/$state' já existe ou erro ao adicionar"
}

create_city "3550308" "São Paulo" "SP"
create_city "3304557" "Rio de Janeiro" "RJ"
create_city "3106200" "Belo Horizonte" "MG"
create_city "4106902" "Curitiba" "PR"
create_city "4314902" "Porto Alegre" "RS"
create_city "5300108" "Brasília" "DF"
create_city "2927408" "Salvador" "BA"
create_city "2304400" "Fortaleza" "CE"
create_city "2611606" "Recife" "PE"
create_city "1302603" "Manaus" "AM"

echo ""
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}🎉 Seed Concluído!${NC}"
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""
echo -e "${CYAN}📊 Dados inseridos:${NC}"
echo "   • Categorias: 6"
echo "   • Serviços: 4"
echo "   • Cidades: 10"
echo ""
echo -e "${CYAN}💡 Próximos passos:${NC}"
echo "   1. Cadastrar providers usando Bruno collections"
echo "   2. Indexar providers para busca"
echo "   3. Testar endpoints de busca"
echo ""
