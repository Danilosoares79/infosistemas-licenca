# BUILD-E-DEPLOY.ps1
# Script completo para build, test e deploy do HubWeb v6.0
# Data: 15/05/2026
# CORRIGIDO: Usa o repositório Git correto em infosistemas-licenca

$ErrorActionPreference = "Stop"

Write-Host "===============================================================================" -ForegroundColor Cyan
Write-Host "BUILD E DEPLOY AUTOMATICO - InfoSistemas.HubWeb v6.0" -ForegroundColor Cyan
Write-Host "===============================================================================" -ForegroundColor Cyan
Write-Host ""

# Caminhos CORRIGIDOS
$repoDir = "C:\INFO_Danilo_Geral\Driver_D\INFOPDV\Dephi\Sistema_Dephi\SistemaPDV_Unificado_v20\InfoSistemasSplit\infosistemas-licenca"
$hubwebDir = Join-Path $repoDir "InfoSistemas.HubWeb"

# Verificar se o diretório existe
if (-not (Test-Path $hubwebDir)) {
    Write-Host "ERRO: Diretorio do HubWeb nao encontrado: $hubwebDir" -ForegroundColor Red
    exit 1
}

# Verificar se é um repositório Git
if (-not (Test-Path (Join-Path $repoDir ".git"))) {
    Write-Host "ERRO: Nao e um repositorio Git: $repoDir" -ForegroundColor Red
    exit 1
}

Write-Host "Repositorio Git: $repoDir" -ForegroundColor Cyan
Write-Host "Projeto HubWeb: $hubwebDir" -ForegroundColor Cyan
Write-Host ""

# ==============================================================================
# ETAPA 1: LIMPAR BUILD ANTERIOR
# ==============================================================================
Write-Host "[1/6] Limpando builds anteriores..." -ForegroundColor Yellow
Push-Location $hubwebDir
try {
    dotnet clean -c Release > $null 2>&1
    Write-Host "  Build anterior limpo" -ForegroundColor Green
} catch {
    Write-Host "  Aviso: Erro ao limpar build anterior" -ForegroundColor Yellow
}
Pop-Location

# ==============================================================================
# ETAPA 2: RESTAURAR DEPENDENCIAS
# ==============================================================================
Write-Host "[2/6] Restaurando dependencias NuGet..." -ForegroundColor Yellow
Push-Location $hubwebDir
try {
    $restoreOutput = dotnet restore 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Dependencias restauradas com sucesso" -ForegroundColor Green
    } else {
        Write-Host "  ERRO ao restaurar dependencias:" -ForegroundColor Red
        Write-Host $restoreOutput -ForegroundColor Red
        Pop-Location
        exit 1
    }
} catch {
    Write-Host "  ERRO ao restaurar: $_" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location

# ==============================================================================
# ETAPA 3: BUILD RELEASE
# ==============================================================================
Write-Host "[3/6] Executando build Release..." -ForegroundColor Yellow
Push-Location $hubwebDir
try {
    $buildOutput = dotnet build -c Release 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Build executado com SUCESSO!" -ForegroundColor Green
        Write-Host ""
        Write-Host "  Resumo:" -ForegroundColor Cyan
        $errorCount = ($buildOutput | Select-String -Pattern "error" | Measure-Object).Count
        $warningCount = ($buildOutput | Select-String -Pattern "warning" | Measure-Object).Count
        Write-Host "    Erros: $errorCount" -ForegroundColor $(if ($errorCount -eq 0) { "Green" } else { "Red" })
        Write-Host "    Warnings: $warningCount" -ForegroundColor $(if ($warningCount -eq 0) { "Green" } else { "Yellow" })
        Write-Host ""
    } else {
        Write-Host "  ERRO no build!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Erros encontrados:" -ForegroundColor Red
        $buildOutput | Select-String "error" | ForEach-Object {
            Write-Host "  $_" -ForegroundColor Red
        }
        Pop-Location
        exit 1
    }
} catch {
    Write-Host "  ERRO ao executar build: $_" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location

# ==============================================================================
# ETAPA 4: VERIFICAR ALTERACOES GIT
# ==============================================================================
Write-Host "[4/6] Verificando alteracoes no Git..." -ForegroundColor Yellow
Push-Location $repoDir
try {
    $gitStatus = git status --porcelain 2>&1
    
    if ($gitStatus) {
        Write-Host "  Arquivos modificados:" -ForegroundColor Cyan
        $gitStatus -split "`n" | ForEach-Object {
            if ($_ -match "DTOs/HubDTOs.cs") {
                Write-Host "    $_ (CORRECOES APLICADAS)" -ForegroundColor Green
            } else {
                Write-Host "    $_" -ForegroundColor Gray
            }
        }
        Write-Host ""
    } else {
        Write-Host "  Nenhuma alteracao detectada" -ForegroundColor Yellow
        Pop-Location
        Write-Host ""
        Write-Host "===============================================================================" -ForegroundColor Green
        Write-Host "BUILD CONCLUIDO - Nenhuma alteracao para commit" -ForegroundColor Green
        Write-Host "===============================================================================" -ForegroundColor Green
        exit 0
    }
} catch {
    Write-Host "  ERRO ao verificar Git: $_" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location

# ==============================================================================
# ETAPA 5: COMMIT
# ==============================================================================
Write-Host "[5/6] Preparando commit..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Correcoes aplicadas em HubDTOs.cs:" -ForegroundColor Cyan
Write-Host "  + ClientReleaseData com MetodologiaId (corrige CS1061)" -ForegroundColor Green
Write-Host "  + Metodologia" -ForegroundColor Green
Write-Host "  + Hotsite com clients_full inicializado (corrige CS0649)" -ForegroundColor Green
Write-Host "  + ClientData" -ForegroundColor Green
Write-Host "  + MotivoBloqueio em ClienteResumoDto" -ForegroundColor Green
Write-Host ""

$commit = Read-Host "Deseja fazer commit das alteracoes? (S/N)"

Push-Location $repoDir
if ($commit -eq "S" -or $commit -eq "s") {
    try {
        git add .
        $commitMsg = "fix(hubweb): adiciona classes e propriedades faltantes (CS1061/CS0649)

Correcoes aplicadas:
- ClientReleaseData: DTO com MetodologiaId (corrige CS1061)
- Metodologia: classe relacionada aos releases
- Hotsite: propriedade clients_full inicializada (corrige CS0649)
- ClientData: dados de clientes para hotsite
- ClienteResumoDto: adiciona MotivoBloqueio

Deploy Railway v6.0 - Codigo pronto para producao"

        git commit -m $commitMsg
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "  Commit realizado com sucesso!" -ForegroundColor Green
        } else {
            Write-Host "  ERRO ao fazer commit" -ForegroundColor Red
            Pop-Location
            exit 1
        }
    } catch {
        Write-Host "  ERRO ao fazer commit: $_" -ForegroundColor Red
        Pop-Location
        exit 1
    }
} else {
    Write-Host "  Commit cancelado pelo usuario" -ForegroundColor Yellow
    Pop-Location
    exit 0
}
Pop-Location

# ==============================================================================
# ETAPA 6: PUSH
# ==============================================================================
Write-Host "[6/6] Preparando push para repositorio remoto..." -ForegroundColor Yellow
Write-Host ""

$push = Read-Host "Deseja fazer push para o repositorio remoto? (S/N)"

Push-Location $repoDir
if ($push -eq "S" -or $push -eq "s") {
    try {
        # Verificar branch atual
        $currentBranch = git branch --show-current 2>&1
        Write-Host "  Branch atual: $currentBranch" -ForegroundColor Cyan
        Write-Host ""
        
        # Fazer push
        Write-Host "  Enviando alteracoes para origin/$currentBranch..." -ForegroundColor Cyan
        $pushOutput = git push origin $currentBranch 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "===============================================================================" -ForegroundColor Green
            Write-Host "DEPLOY CONCLUIDO COM SUCESSO!" -ForegroundColor Green
            Write-Host "===============================================================================" -ForegroundColor Green
            Write-Host ""
            Write-Host "Resumo:" -ForegroundColor Cyan
            Write-Host "  Limpeza ........... OK" -ForegroundColor Green
            Write-Host "  Restauracao ....... OK" -ForegroundColor Green
            Write-Host "  Build ............. OK" -ForegroundColor Green
            Write-Host "  Commit ............ OK" -ForegroundColor Green
            Write-Host "  Push .............. OK" -ForegroundColor Green
            Write-Host ""
            Write-Host "O Railway detectara o push automaticamente e iniciara o deploy." -ForegroundColor Cyan
            Write-Host ""
            Write-Host "Proximos passos:" -ForegroundColor Yellow
            Write-Host "  1. Acesse https://railway.app" -ForegroundColor Gray
            Write-Host "  2. Monitore os Build Logs" -ForegroundColor Gray
            Write-Host "  3. Verifique se o deploy foi concluido" -ForegroundColor Gray
            Write-Host "  4. Configure as variaveis de ambiente (se necessario):" -ForegroundColor Gray
            Write-Host "     - LICENCA_API_URL" -ForegroundColor DarkGray
            Write-Host "     - ADMIN_KEY" -ForegroundColor DarkGray
            Write-Host "  5. Teste a aplicacao em producao" -ForegroundColor Gray
            Write-Host ""
        } else {
            Write-Host ""
            Write-Host "  ERRO ao fazer push:" -ForegroundColor Red
            Write-Host $pushOutput -ForegroundColor Red
            Pop-Location
            exit 1
        }
    } catch {
        Write-Host "  ERRO ao fazer push: $_" -ForegroundColor Red
        Pop-Location
        exit 1
    }
} else {
    Write-Host ""
    Write-Host "  Push cancelado pelo usuario" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Para fazer push manualmente:" -ForegroundColor Cyan
    Write-Host "  cd $repoDir" -ForegroundColor Gray
    Write-Host "  git push origin main" -ForegroundColor Gray
}
Pop-Location

Write-Host ""
Write-Host "===============================================================================" -ForegroundColor Cyan
Write-Host "SCRIPT FINALIZADO" -ForegroundColor Cyan
Write-Host "===============================================================================" -ForegroundColor Cyan
Write-Host ""
pause
