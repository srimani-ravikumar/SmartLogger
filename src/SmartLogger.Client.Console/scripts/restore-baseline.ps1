<#
.SYNOPSIS
    Resets smartlogger.json back to the known-good baseline (Console, INFO).
    Run this between scenarios to start each test from a clean state.
#>
param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot "..\bin\Debug\net8.0\smartlogger.json")
)

$baseline = @{
    rootLogLevel = "INFO"
    appenders    = @(
        @{
            destination = @{ type = "Console" }
            formatter   = @{ outputFormat = "PlainText"; layoutType = "Simple" }
        }
    )
} | ConvertTo-Json -Depth 10

Set-Content $ConfigPath $baseline
Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Config restored to baseline."
