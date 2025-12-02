# Monitor de Coverage - Processos Paralelos
# Uso: .\scripts\monitor-coverage.ps1

Write-Host "📊 MONITORANDO COVERAGE - LOCAL E PIPELINE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Gray
Write-Host ""

# Verificar job local
$job = Get-Job -Name "CleanCoverage" -ErrorAction SilentlyContinue

if ($job) {
    Write-Host "🖥️ COVERAGE LOCAL (Background Job):" -ForegroundColor Yellow
    Write-Host "───────────────────────────────────" -ForegroundColor Gray
    Write-Host "  Estado: $($job.State)" -ForegroundColor $(if ($job.State -eq 'Running') { 'Cyan' } elseif ($job.State -eq 'Completed') { 'Green' } else { 'Red' })
    Write-Host "  Job ID: $($job.Id)"
    Write-Host ""
    
    if ($job.State -eq 'Running') {
        Write-Host "  ⏳ Ainda em execução..." -ForegroundColor Cyan
        Write-Host "  💡 Para ver progresso: Receive-Job -Id $($job.Id) -Keep" -ForegroundColor Gray
    }
    elseif ($job.State -eq 'Completed') {
        Write-Host "  ✅ CONCLUÍDO!" -ForegroundColor Green
        Write-Host ""
        Write-Host "  📄 Últimas 30 linhas do output:" -ForegroundColor White
        Write-Host "  ───────────────────────────────────" -ForegroundColor Gray
        Receive-Job -Id $job.Id -Keep | Select-Object -Last 30
        
        # Verificar se relatório foi gerado
        $summaryPath = "coverage/report/Summary.txt"
        if (Test-Path $summaryPath) {
            Write-Host ""
            Write-Host "  📊 RESUMO DE COVERAGE:" -ForegroundColor Green
            Write-Host "  ───────────────────────────────────" -ForegroundColor Gray
            try {
                Get-Content $summaryPath -ErrorAction Stop | Select-Object -First 15
            } catch {
                Write-Host "  ⚠️ Erro ao ler arquivo de resumo: $_" -ForegroundColor Yellow
            }
        }
    }
    elseif ($job.State -eq 'Failed') {
        Write-Host "  ❌ ERRO!" -ForegroundColor Red
        Receive-Job -Id $job.Id
    }
}
else {
    Write-Host "🖥️ COVERAGE LOCAL: Não encontrado" -ForegroundColor Red
    Write-Host "  💡 Execute: .\scripts\generate-clean-coverage.ps1" -ForegroundColor Gray
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Gray
Write-Host ""

# Link para pipeline
try {
    $branch = git rev-parse --abbrev-ref HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw }
} catch {
    $branch = "unknown-branch"
    Write-Warning "Git não disponível ou não está em um repositório - usando branch padrão"
}

try {
    $commit = git rev-parse --short HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw }
} catch {
    $commit = "unknown-commit"
    Write-Warning "Não foi possível obter commit hash"
}

try {
    $commitMsg = git log -1 --pretty=%s 2>$null
    if ($LASTEXITCODE -ne 0) { throw }
} catch {
    $commitMsg = "unknown-message"
    Write-Warning "Não foi possível obter mensagem do commit"
}

try {
    $repoUrl = (git remote get-url origin 2>$null) -replace '\.git$', '' -replace '^git@github\.com:', 'https://github.com/'
    if ($LASTEXITCODE -ne 0 -or -not $repoUrl) { throw }
} catch {
    $repoUrl = "https://github.com/frigini/MeAjudaAi"
    Write-Warning "Não foi possível obter URL do repositório - usando padrão"
}

Write-Host "🌐 PIPELINE GITHUB:" -ForegroundColor Yellow
Write-Host "───────────────────────────────────" -ForegroundColor Gray
Write-Host "  $repoUrl/actions" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Branch: $branch" -ForegroundColor White
Write-Host "  Commit: $commit ($commitMsg)" -ForegroundColor White
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Gray
Write-Host ""
Write-Host "🔄 COMANDOS ÚTEIS:" -ForegroundColor Magenta
Write-Host ""
Write-Host "  Ver progresso local:" -ForegroundColor White
Write-Host "    Receive-Job -Name CleanCoverage -Keep" -ForegroundColor Gray
Write-Host ""
Write-Host "  Remover job concluído:" -ForegroundColor White
Write-Host "    Remove-Job -Name CleanCoverage" -ForegroundColor Gray
Write-Host ""
Write-Host "  Abrir relatório local:" -ForegroundColor White
Write-Host "    Start-Process coverage/report/index.html" -ForegroundColor Gray
Write-Host ""
Write-Host "  Re-executar este monitor:" -ForegroundColor White
Write-Host "    .\scripts\monitor-coverage.ps1" -ForegroundColor Gray
Write-Host ""
