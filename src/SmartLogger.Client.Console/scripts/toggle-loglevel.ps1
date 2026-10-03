<#
.SYNOPSIS
    Scenario 1: Basic live reload - flips rootLogLevel between INFO and DEBUG.
    Run while Scenario_BasicLiveReload_LogLevelChange() is looping in the demo app.
#>
param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot "..\bin\Debug\net8.0\smartlogger.json"),
    [int]$Iterations = 6,
    [int]$IntervalSeconds = 5
)

$levels = @("INFO", "DEBUG")

for ($i = 0; $i -lt $Iterations; $i++) {
    $level = $levels[$i % 2]

    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    $config.rootLogLevel = $level
    $config | ConvertTo-Json -Depth 10 | Set-Content $ConfigPath

    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] rootLogLevel -> $level"
    Start-Sleep -Seconds $IntervalSeconds
}
