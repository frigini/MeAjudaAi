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
        Receive-Job -Id $job.Id | Select-Object -Last 30
        
        # Verificar se relatório foi gerado
        if (Test-Path "coverage/report/Summary.txt") {
            Write-Host ""
            Write-Host "  📊 RESUMO DE COVERAGE:" -ForegroundColor Green
            Write-Host "  ───────────────────────────────────" -ForegroundColor Gray
            Get-Content coverage/report/Summary.txt | Select-Object -First 15
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
$branch = git rev-parse --abbrev-ref HEAD
$commit = git rev-parse --short HEAD
$commitMsg = git log -1 --pretty=%s

Write-Host "🌐 PIPELINE GITHUB:" -ForegroundColor Yellow
Write-Host "───────────────────────────────────" -ForegroundColor Gray
Write-Host "  https://github.com/frigini/MeAjudaAi/actions" -ForegroundColor Cyan
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
