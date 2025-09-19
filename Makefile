# =============================================================================
# MeAjudaAi Makefile - Comandos Unificados do Projeto
# =============================================================================
# Este Makefile centraliza todos os comandos principais do projeto MeAjudaAi.
# Use 'make help' para ver todos os comandos disponíveis.

.PHONY: help dev test deploy setup optimize clean install build run

# Cores para output
CYAN := \033[36m
YELLOW := \033[33m
GREEN := \033[32m
RED := \033[31m
RESET := \033[0m

# Configurações
ENVIRONMENT ?= dev
LOCATION ?= brazilsouth

# Target padrão
.DEFAULT_GOAL := help

## Ajuda e Informações
help: ## Mostra esta ajuda
	@echo "$(CYAN)MeAjudaAi - Comandos Disponíveis$(RESET)"
	@echo "$(CYAN)================================$(RESET)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(CYAN)%-20s$(RESET) %s\n", $$1, $$2}'
	@echo ""
	@echo "$(YELLOW)Exemplos de uso:$(RESET)"
	@echo "  make dev               # Executar ambiente de desenvolvimento"
	@echo "  make test-fast         # Testes otimizados"
	@echo "  make deploy ENV=prod   # Deploy para produção"
	@echo ""

status: ## Mostra status do projeto
	@echo "$(CYAN)Status do Projeto MeAjudaAi$(RESET)"
	@echo "$(CYAN)============================$(RESET)"
	@echo "Localização: $(shell pwd)"
	@echo "Branch: $(shell git branch --show-current 2>/dev/null || echo 'N/A')"
	@echo "Último commit: $(shell git log -1 --pretty=format:'%h - %s' 2>/dev/null || echo 'N/A')"
	@echo "Scripts disponíveis: $(shell ls scripts/*.sh 2>/dev/null | wc -l)"
	@echo ""

## Desenvolvimento
dev: ## Executa ambiente de desenvolvimento
	@echo "$(GREEN)🚀 Iniciando ambiente de desenvolvimento...$(RESET)"
	@./scripts/dev.sh

dev-simple: ## Executa desenvolvimento simples (sem Azure)
	@echo "$(GREEN)⚡ Iniciando desenvolvimento simples...$(RESET)"
	@./scripts/dev.sh --simple

install: ## Instala dependências do projeto
	@echo "$(GREEN)📦 Instalando dependências...$(RESET)"
	@dotnet restore

build: ## Compila a solução
	@echo "$(GREEN)🔨 Compilando solução...$(RESET)"
	@dotnet build --no-restore

run: ## Executa a aplicação (via Aspire)
	@echo "$(GREEN)▶️  Executando aplicação...$(RESET)"
	@cd src/Aspire/MeAjudaAi.AppHost && dotnet run

## Testes
test: ## Executa todos os testes
	@echo "$(GREEN)🧪 Executando todos os testes...$(RESET)"
	@./scripts/test.sh

test-unit: ## Executa apenas testes unitários
	@echo "$(GREEN)🔬 Executando testes unitários...$(RESET)"
	@./scripts/test.sh --unit

test-integration: ## Executa apenas testes de integração
	@echo "$(GREEN)🔗 Executando testes de integração...$(RESET)"
	@./scripts/test.sh --integration

test-fast: ## Executa testes com otimizações (70% mais rápido)
	@echo "$(GREEN)⚡ Executando testes otimizados...$(RESET)"
	@./scripts/test.sh --fast

test-coverage: ## Executa testes com relatório de cobertura
	@echo "$(GREEN)📊 Executando testes com cobertura...$(RESET)"
	@./scripts/test.sh --coverage

## Deploy e Infraestrutura
deploy: ## Deploy para ambiente especificado (use ENV=dev|prod)
	@echo "$(GREEN)🌐 Fazendo deploy para $(ENVIRONMENT)...$(RESET)"
	@./scripts/deploy.sh $(ENVIRONMENT) $(LOCATION)

deploy-dev: ## Deploy para ambiente de desenvolvimento
	@echo "$(GREEN)🔧 Deploy para desenvolvimento...$(RESET)"
	@./scripts/deploy.sh dev $(LOCATION)

deploy-prod: ## Deploy para produção
	@echo "$(GREEN)🚀 Deploy para produção...$(RESET)"
	@./scripts/deploy.sh prod $(LOCATION)

deploy-preview: ## Simula deploy sem executar (what-if)
	@echo "$(YELLOW)👁️  Simulando deploy para $(ENVIRONMENT)...$(RESET)"
	@./scripts/deploy.sh $(ENVIRONMENT) $(LOCATION) --what-if

## Setup e Configuração
setup: ## Configura ambiente inicial para novos desenvolvedores
	@echo "$(GREEN)⚙️  Configurando ambiente inicial...$(RESET)"
	@./scripts/setup.sh

setup-verbose: ## Setup com logs detalhados
	@echo "$(GREEN)🔍 Setup com logs detalhados...$(RESET)"
	@./scripts/setup.sh --verbose

setup-dev-only: ## Setup apenas para desenvolvimento (sem Azure/Docker)
	@echo "$(GREEN)💻 Setup apenas desenvolvimento...$(RESET)"
	@./scripts/setup.sh --dev-only

## Otimização e Performance
optimize: ## Aplica otimizações de performance para testes
	@echo "$(GREEN)⚡ Aplicando otimizações de performance...$(RESET)"
	@./scripts/optimize.sh

optimize-test: ## Aplica otimizações e executa teste de performance
	@echo "$(GREEN)🏃 Testando otimizações de performance...$(RESET)"
	@./scripts/optimize.sh --test

optimize-reset: ## Remove otimizações e restaura configurações padrão
	@echo "$(YELLOW)🔄 Restaurando configurações padrão...$(RESET)"
	@./scripts/optimize.sh --reset

## Limpeza e Manutenção
clean: ## Limpa artefatos de build e cache
	@echo "$(YELLOW)🧹 Limpando artefatos de build...$(RESET)"
	@dotnet clean
	@rm -rf **/bin **/obj
	@echo "$(GREEN)✅ Limpeza concluída!$(RESET)"

clean-docker: ## Remove containers e volumes do Docker (CUIDADO!)
	@echo "$(RED)⚠️  Removendo containers Docker do MeAjudaAi...$(RESET)"
	@echo "$(RED)Isso irá apagar TODOS os dados locais!$(RESET)"
	@read -p "Continuar? (y/N): " confirm && [ "$$confirm" = "y" ] || exit 1
	@docker ps -a --format "table {{.Names}}" | grep "meajudaai" | xargs -r docker rm -f
	@docker volume ls --format "table {{.Name}}" | grep "meajudaai" | xargs -r docker volume rm
	@echo "$(GREEN)✅ Containers e volumes removidos!$(RESET)"

clean-all: clean clean-docker ## Limpeza completa (build + docker)

## CI/CD Setup (PowerShell - Windows)
setup-cicd: ## Configura pipeline CI/CD completo (requer PowerShell)
	@echo "$(GREEN)🔧 Configurando CI/CD...$(RESET)"
	@powershell -ExecutionPolicy Bypass -File ./setup-cicd.ps1

setup-ci-only: ## Configura apenas CI sem deploy (requer PowerShell)
	@echo "$(GREEN)🧪 Configurando CI apenas...$(RESET)"
	@powershell -ExecutionPolicy Bypass -File ./setup-ci-only.ps1

## Informações e Debug
logs: ## Mostra logs da aplicação (se rodando via Docker)
	@echo "$(CYAN)📜 Logs da aplicação:$(RESET)"
	@docker logs meajudaai-apiservice 2>/dev/null || echo "Aplicação não está rodando via Docker"

ps: ## Mostra processos .NET em execução
	@echo "$(CYAN)🔍 Processos .NET:$(RESET)"
	@ps aux | grep dotnet | grep -v grep || echo "Nenhum processo .NET encontrado"

docker-ps: ## Mostra containers Docker do projeto
	@echo "$(CYAN)🐳 Containers Docker:$(RESET)"
	@docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep meajudaai || echo "Nenhum container do MeAjudaAi rodando"

check: ## Verifica dependências e configuração
	@echo "$(CYAN)✅ Verificando dependências:$(RESET)"
	@which dotnet >/dev/null && echo "✅ .NET SDK: $$(dotnet --version)" || echo "❌ .NET SDK não encontrado"
	@which docker >/dev/null && echo "✅ Docker: $$(docker --version)" || echo "❌ Docker não encontrado"
	@which az >/dev/null && echo "✅ Azure CLI: $$(az --version | head -1)" || echo "⚠️  Azure CLI não encontrado"
	@which git >/dev/null && echo "✅ Git: $$(git --version)" || echo "❌ Git não encontrado"

## Atalhos úteis
quick: install build test-unit ## Sequência rápida: install + build + testes unitários

all: install build test ## Sequência completa: install + build + todos os testes

ci: install build test-fast ## Simulação de CI: install + build + testes otimizados

## Desenvolvimento específico
watch: ## Executa em modo watch (rebuild automático)
	@echo "$(GREEN)👁️  Executando em modo watch...$(RESET)"
	@cd src/Aspire/MeAjudaAi.AppHost && dotnet watch run

format: ## Formata código usando dotnet format
	@echo "$(GREEN)✨ Formatando código...$(RESET)"
	@dotnet format

update: ## Atualiza dependências NuGet
	@echo "$(GREEN)📦 Atualizando dependências...$(RESET)"
	@dotnet list package --outdated
	@echo "Use 'dotnet add package <nome> --version <versao>' para atualizar"