<#
.SYNOPSIS
    Scenario 4: Fires many rapid writes to simulate a FileSystemWatcher event storm.
    Expectation: reloads are serialized/debounced (100ms sleep + lock) - no crash,
    no torn reads, and the final write wins.
#>
param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot "..\bin\Debug\net8.0\smartlogger.json"),
    [int]$Writes = 25
)

for ($i = 0; $i -lt $Writes; $i++) {
    $level = if ($i % 2 -eq 0) { "INFO" } else { "DEBUG" }

    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    $config.rootLogLevel = $level
    $config | ConvertTo-Json -Depth 10 | Set-Content $ConfigPath

    Start-Sleep -Milliseconds 50
}

$finalLevel = if (($Writes - 1) % 2 -eq 0) { "INFO" } else { "DEBUG" }
Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Storm complete - final rootLogLevel should be $finalLevel."
