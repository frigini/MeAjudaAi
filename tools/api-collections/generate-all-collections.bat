@echo off
REM Script para Windows - Gerador de Collections da API MeAjudaAi

setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
set "PROJECT_ROOT=%SCRIPT_DIR%..\.."

echo [%date% %time%] 🚀 Iniciando geração de API Collections - MeAjudaAi
echo.

REM Verificar Node.js
where node >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Node.js não encontrado. Instale Node.js 18+ para continuar.
    exit /b 1
)

REM Verificar .NET
where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET não encontrado. Instale .NET 8+ para continuar.
    exit /b 1
)

echo ✅ Dependências verificadas

REM Instalar dependências npm se necessário
cd /d "%SCRIPT_DIR%"
if not exist "node_modules" (
    echo 📦 Instalando dependências npm...
    call npm install
    if %errorlevel% neq 0 (
        echo ❌ Erro ao instalar dependências
        exit /b 1
    )
)

REM Verificar se API está rodando
curl -s "http://localhost:5000/health" >nul 2>&1
if %errorlevel% equ 0 (
    echo ✅ API já está rodando em http://localhost:5000
    goto :generate
)

REM Iniciar API
echo 🚀 Iniciando API...
cd /d "%PROJECT_ROOT%\src\Bootstrapper\MeAjudaAi.ApiService"

REM Build da aplicação
dotnet build --configuration Release --no-restore
if %errorlevel% neq 0 (
    echo ❌ Erro ao compilar API
    exit /b 1
)

REM Iniciar API em background
start "MeAjudaAi API" /min dotnet run --configuration Release --urls="http://localhost:5000"

REM Aguardar API estar pronta
set /a attempts=0
set /a max_attempts=30

:wait_api
curl -s "http://localhost:5000/health" >nul 2>&1
if %errorlevel% equ 0 (
    echo ✅ API iniciada com sucesso
    goto :generate
)

set /a attempts+=1
if %attempts% geq %max_attempts% (
    echo ❌ Timeout ao iniciar API
    exit /b 1
)

echo ⏳ Aguardando API iniciar... (tentativa %attempts%/%max_attempts%)
timeout /t 2 /nobreak >nul
goto :wait_api

:generate
REM Gerar Postman Collections
echo 📋 Gerando Postman Collections...
cd /d "%SCRIPT_DIR%"
node generate-postman-collections.js
if %errorlevel% neq 0 (
    echo ❌ Erro ao gerar Postman Collections
    exit /b 1
)

echo ✅ Postman Collections geradas com sucesso!

REM Mostrar resultados
echo.
echo 🎉 Geração de collections concluída!
echo.
echo 📁 Arquivos gerados em: %PROJECT_ROOT%\src\Shared\API.Collections\Generated
echo.
echo 📖 Como usar:
echo   1. Importe os arquivos .json no Postman
echo   2. Configure o ambiente desejado (development/staging/production)
echo   3. Execute 'Get Keycloak Token' para autenticar
echo   4. Execute 'Health Check' para testar conectividade
echo.
echo ✨ Processo concluído com sucesso!

pause