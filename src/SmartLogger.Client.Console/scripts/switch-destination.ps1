<#
.SYNOPSIS
    Scenario 5: Flips the appender destination between Console and FileSystem at runtime.
    Expectation: LoggerFactory soft-reload rewires appenders without restart; no duplicate
    file handles/locks are left behind by FileAppenderRegistry.
#>
param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot "..\bin\Debug\net8.0\smartlogger.json"),
    [int]$Iterations = 4,
    [int]$IntervalSeconds = 5
)

$console = @{
    rootLogLevel = "INFO"
    appenders    = @(
        @{
            destination = @{ type = "Console" }
            formatter   = @{ outputFormat = "PlainText"; layoutType = "Simple" }
        }
    )
} | ConvertTo-Json -Depth 10

$file = @{
    rootLogLevel = "INFO"
    appenders    = @(
        @{
            destination = @{
                type = "FileSystem"
                file = @{ directory = "Logs"; fileName = "ReloadTest"; extension = "log" }
            }
            formatter = @{ outputFormat = "Json" }
        }
    )
} | ConvertTo-Json -Depth 10

for ($i = 0; $i -lt $Iterations; $i++) {
    if ($i % 2 -eq 0) {
        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Switching to Console destination..."
        Set-Content $ConfigPath $console
    } else {
        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Switching to FileSystem destination..."
        Set-Content $ConfigPath $file
    }

    Start-Sleep -Seconds $IntervalSeconds
}
