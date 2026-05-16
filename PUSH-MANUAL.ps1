# PUSH-MANUAL.ps1
# Diagnostico e push manual com tratamento de erros
# Data: 15/05/2026

$ErrorActionPreference = "Continue"

Write-Host "===============================================================================" -ForegroundColor Cyan
Write-Host "DIAGNOSTICO E PUSH MANUAL - InfoSistemas.HubWeb v6.0" -ForegroundColor Cyan
Write-Host "===============================================================================" -ForegroundColor Cyan
Write-Host ""

$repoDir = "C:\INFO_Danilo_Geral\Driver_D\INFOPDV\Dephi\Sistema_Dephi\SistemaPDV_Unificado_v20\InfoSistemasSplit\infosistemas-licenca"

Push-Location $repoDir

# ==============================================================================
# DIAGNOSTICO
# ==============================================================================
Write-Host "[1/5] Verificando status do Git..." -ForegroundColor Yellow

$branch = git branch --show-current
Write-Host "  Branch atual: $branch" -ForegroundColor Cyan

$remote = git remote -v
Write-Host "  Repositorio remoto:" -ForegroundColor Cyan
Write-Host "  $remote" -ForegroundColor Gray
Write-Host ""

# ==============================================================================
# VERIFICAR SE HA COMMITS NAO ENVIADOS
# ==============================================================================
Write-Host "[2/5] Verificando commits pendentes..." -ForegroundColor Yellow

$unpushed = git log origin/$branch..$branch --oneline 2>&1
if ($LASTEXITCODE -eq 0 -and $unpushed) {
    Write-Host "  Commits pendentes para push:" -ForegroundColor Cyan
    Write-Host "  $unpushed" -ForegroundColor Green
} else {
    Write-Host "  Nenhum commit pendente (ou erro ao verificar)" -ForegroundColor Yellow
}
Write-Host ""

# ==============================================================================
# TENTAR PULL PRIMEIRO
# ==============================================================================
Write-Host "[3/5] Sincronizando com repositorio remoto..." -ForegroundColor Yellow
Write-Host "  Executando git pull..." -ForegroundColor Cyan

$pullOutput = git pull origin $branch 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "  Pull executado com sucesso!" -ForegroundColor Green
} else {
    Write-Host "  Aviso durante pull:" -ForegroundColor Yellow
    Write-Host "  $pullOutput" -ForegroundColor Gray
}
Write-Host ""

# ==============================================================================
# TENTAR PUSH
# ==============================================================================
Write-Host "[4/5] Tentando push..." -ForegroundColor Yellow
Write-Host "  Executando: git push origin $branch" -ForegroundColor Cyan
Write-Host ""

$pushOutput = git push origin $branch 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "===============================================================================" -ForegroundColor Green
    Write-Host "PUSH REALIZADO COM SUCESSO!" -ForegroundColor Green
    Write-Host "===============================================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "O Railway detectara o push e iniciara o deploy automaticamente." -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Proximos passos:" -ForegroundColor Yellow
    Write-Host "  1. Acesse https://railway.app" -ForegroundColor Gray
    Write-Host "  2. Monitore os Build Logs do projeto" -ForegroundColor Gray
    Write-Host "  3. Aguarde o deploy concluir" -ForegroundColor Gray
    Write-Host "  4. Teste a aplicacao" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "===============================================================================" -ForegroundColor Red
    Write-Host "ERRO AO FAZER PUSH" -ForegroundColor Red
    Write-Host "===============================================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Detalhes do erro:" -ForegroundColor Yellow
    Write-Host "$pushOutput" -ForegroundColor Red
    Write-Host ""
    
    # ==============================================================================
    # DIAGNOSTICO DE ERROS COMUNS
    # ==============================================================================
    Write-Host "[5/5] Diagnosticando problema..." -ForegroundColor Yellow
    Write-Host ""
    
    if ($pushOutput -match "Authentication failed" -or $pushOutput -match "fatal: Authentication") {
        Write-Host "PROBLEMA: Falha de autenticacao" -ForegroundColor Red
        Write-Host ""
        Write-Host "SOLUCOES:" -ForegroundColor Yellow
        Write-Host "  1. Configure suas credenciais do GitHub:" -ForegroundColor Cyan
        Write-Host "     git config --global user.name 'Seu Nome'" -ForegroundColor Gray
        Write-Host "     git config --global user.email 'seu@email.com'" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  2. Se usar token de acesso pessoal (recomendado):" -ForegroundColor Cyan
        Write-Host "     - Acesse: https://github.com/settings/tokens" -ForegroundColor Gray
        Write-Host "     - Gere um novo token com permissoes 'repo'" -ForegroundColor Gray
        Write-Host "     - Use o token como senha ao fazer push" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  3. Ou configure SSH:" -ForegroundColor Cyan
        Write-Host "     - Siga: https://docs.github.com/pt/authentication/connecting-to-github-with-ssh" -ForegroundColor Gray
    }
    elseif ($pushOutput -match "rejected" -or $pushOutput -match "non-fast-forward") {
        Write-Host "PROBLEMA: Push rejeitado (branch desatualizada)" -ForegroundColor Red
        Write-Host ""
        Write-Host "SOLUCAO:" -ForegroundColor Yellow
        Write-Host "  Execute:" -ForegroundColor Cyan
        Write-Host "  git pull --rebase origin $branch" -ForegroundColor Gray
        Write-Host "  git push origin $branch" -ForegroundColor Gray
    }
    elseif ($pushOutput -match "protected branch") {
        Write-Host "PROBLEMA: Branch protegida" -ForegroundColor Red
        Write-Host ""
        Write-Host "SOLUCAO:" -ForegroundColor Yellow
        Write-Host "  1. Crie um Pull Request no GitHub" -ForegroundColor Cyan
        Write-Host "  2. Ou desative a protecao da branch nas configuracoes do repo" -ForegroundColor Cyan
    }
    else {
        Write-Host "PROBLEMA: Erro desconhecido" -ForegroundColor Red
        Write-Host ""
        Write-Host "TENTE:" -ForegroundColor Yellow
        Write-Host "  1. Verificar conexao com internet" -ForegroundColor Cyan
        Write-Host "  2. Tentar novamente em alguns minutos" -ForegroundColor Cyan
        Write-Host "  3. Fazer push manualmente:" -ForegroundColor Cyan
        Write-Host "     cd $repoDir" -ForegroundColor Gray
        Write-Host "     git push origin $branch" -ForegroundColor Gray
    }
    Write-Host ""
}

Pop-Location

Write-Host ""
pause
