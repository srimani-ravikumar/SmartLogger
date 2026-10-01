<#
.SYNOPSIS
    Starts the SmartLogger.Aggregator.Demo API, checking first whether it is already running.

.DESCRIPTION
    Dev-only helper script. Before starting the service it checks whether the
    configured port is already in use (i.e. the aggregator is already running)
    and skips the start if so.
#>

param(
    [int]$Port = 5290
)

$ErrorActionPreference = "Stop"
$projectPath = Join-Path $PSScriptRoot "SmartLogger.Aggregator.Demo.csproj"

$existing = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue

if ($existing) {
    Write-Host "SmartLogger.Aggregator.Demo already running on port $Port (PID $($existing[0].OwningProcess)). Skipping start." -ForegroundColor Yellow
    return
}

Write-Host "Starting SmartLogger.Aggregator.Demo on port $Port..." -ForegroundColor Green
dotnet run --project $projectPath --urls "http://localhost:$Port"
