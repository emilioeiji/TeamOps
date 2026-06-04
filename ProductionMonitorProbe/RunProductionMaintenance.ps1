param(
    [switch]$ApplyCleanup,
    [string]$DatabasePath = "",
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$probeExe = Join-Path $scriptRoot "ProductionMonitorProbe.exe"
$probeDllConfig = Join-Path $scriptRoot "ProductionMonitorProbe.dll.config"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

if (-not (Test-Path $probeExe)) {
    throw "ProductionMonitorProbe.exe nao encontrado em: $scriptRoot"
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $scriptRoot "maintenance-$timestamp"
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "== $Message =="
}

function Invoke-Probe {
    param(
        [string]$Name,
        [string[]]$Arguments
    )

    $outFile = Join-Path $OutputDirectory "$Name.out.txt"
    $errFile = Join-Path $OutputDirectory "$Name.err.txt"
    Write-Step "ProductionMonitorProbe.exe $($Arguments -join ' ')"
    & $probeExe @Arguments 1> $outFile 2> $errFile
    $exitCode = $LASTEXITCODE

    Get-Content $outFile -ErrorAction SilentlyContinue | Select-Object -First 80
    if ($exitCode -ne 0) {
        Write-Host "Falha. Saida completa: $outFile"
        Write-Host "Erros: $errFile"
        exit $exitCode
    }
}

if ([string]::IsNullOrWhiteSpace($DatabasePath) -and (Test-Path $probeDllConfig)) {
    [xml]$config = Get-Content $probeDllConfig
    $databaseSetting = $config.configuration.appSettings.add |
        Where-Object { $_.key -eq "DatabasePath" } |
        Select-Object -First 1
    if ($databaseSetting -and -not [string]::IsNullOrWhiteSpace($databaseSetting.value)) {
        $DatabasePath = $databaseSetting.value
    }
}

Write-Step "Ambiente"
Write-Host "Pasta do probe: $scriptRoot"
Write-Host "Pasta de saida: $OutputDirectory"
Write-Host "ApplyCleanup: $ApplyCleanup"
Write-Host "DatabasePath: $DatabasePath"

if (-not [string]::IsNullOrWhiteSpace($DatabasePath)) {
    if (-not (Test-Path $DatabasePath)) {
        throw "Banco nao encontrado: $DatabasePath"
    }

    $backupPath = Join-Path $OutputDirectory ("teamops-before-production-maintenance-$timestamp.db")
    Write-Step "Backup do banco"
    Copy-Item -LiteralPath $DatabasePath -Destination $backupPath -Force
    Write-Host "Backup criado: $backupPath"
}
else {
    Write-Host "DatabasePath nao foi informado nem encontrado no .config. O probe ainda sera executado com a configuracao padrao."
}

Invoke-Probe -Name "01-schema-check" -Arguments @("schema-check")
Invoke-Probe -Name "02-db-index-check" -Arguments @("db-index-check")
Invoke-Probe -Name "03-production-diagnostics-before" -Arguments @("production-diagnostics")
Invoke-Probe -Name "04-machine-cleanup-dry-run" -Arguments @("machine-cleanup")

if ($ApplyCleanup) {
    Invoke-Probe -Name "05-machine-cleanup-apply" -Arguments @("machine-cleanup", "--apply")
    Invoke-Probe -Name "06-production-diagnostics-after" -Arguments @("production-diagnostics")
}
else {
    Write-Step "Limpeza nao aplicada"
    Write-Host "Revise o arquivo 04-machine-cleanup-dry-run.out.txt."
    Write-Host "Para aplicar em producao, rode novamente:"
    Write-Host ".\RunProductionMaintenance.ps1 -ApplyCleanup"
}

Write-Step "Concluido"
Write-Host "Logs e backup em: $OutputDirectory"
