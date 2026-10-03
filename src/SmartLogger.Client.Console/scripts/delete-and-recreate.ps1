<#
.SYNOPSIS
    Scenario 6: Deletes the watched config file, waits, then recreates it with
    rootLogLevel=DEBUG (instead of the identical baseline) so a successful reload
    is visibly distinguishable from "nothing happened".
    FileConfigurationProviderBase now wires Created/Deleted/Renamed in addition to
    Changed, so the recreate below is expected to trigger a reload.
#>
param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot "..\bin\Debug\net8.0\smartlogger.json")
)

$debugConfig = @{
    rootLogLevel = "DEBUG"
    appenders    = @(
        @{
            destination = @{ type = "Console" }
            formatter   = @{ outputFormat = "PlainText"; layoutType = "Simple" }
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Deleting config file..."
Remove-Item $ConfigPath -Force
Start-Sleep -Seconds 3

Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Recreating config file with rootLogLevel=DEBUG..."
Set-Content $ConfigPath $debugConfig
