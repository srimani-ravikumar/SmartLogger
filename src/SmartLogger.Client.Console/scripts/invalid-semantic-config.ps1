<#
.SYNOPSIS
    Scenario 3: Writes structurally valid but semantically invalid JSON
    (FileSystem destination with no "file" block), then restores the baseline.
    Expectation: ConfigurationValidator throws, reload is rejected, old config stays active.
#>
param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot "..\bin\Debug\net8.0\smartlogger.json")
)

$lastGood = Get-Content $ConfigPath -Raw

$invalid = @{
    rootLogLevel = "INFO"
    appenders    = @(
        @{
            destination = @{ type = "FileSystem" } # missing required "file" block
            formatter   = @{ outputFormat = "Json" }
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Writing semantically invalid config (FileSystem without 'file')..."
Set-Content $ConfigPath $invalid
Start-Sleep -Seconds 3

Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Restoring last known-good config..."
Set-Content $ConfigPath $lastGood
