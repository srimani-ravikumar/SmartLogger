<#
.SYNOPSIS
    Scenario 2: Writes malformed JSON mid-edit, then restores the last known-good config.
    Expectation: JsonConfigurationProvider.Load() throws, FileConfigurationProviderBase
    swallows it, and the previously active configuration keeps running untouched.
#>
param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot "..\bin\Debug\net8.0\smartlogger.json")
)

$lastGood = Get-Content $ConfigPath -Raw

Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Writing malformed JSON..."
Set-Content $ConfigPath '{ "rootLogLevel": "INFO", "appenders": [ { "destination": BROKEN'
Start-Sleep -Seconds 3

Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Restoring last known-good config..."
Set-Content $ConfigPath $lastGood
