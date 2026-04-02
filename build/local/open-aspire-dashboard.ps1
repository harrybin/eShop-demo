$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$dashboardUrlFile = Join-Path $repoRoot ".aspire\dashboard-login-url.txt"
$runLocalScript = Join-Path $repoRoot "build\local\run-local.ps1"
$appHostLogFile = Join-Path $repoRoot ".aspire\apphost.console.log"

function Get-DashboardLoginUrl {
    if (-not (Test-Path $dashboardUrlFile)) {
        return $null
    }

    $candidate = (Get-Content -Path $dashboardUrlFile -ErrorAction SilentlyContinue | Select-Object -First 1).Trim()
    if ($candidate -match "^https://localhost:19888/login\?t=") {
        return $candidate
    }

    return $null
}

function Test-DashboardPortOpen {
    try {
        $result = Test-NetConnection -ComputerName localhost -Port 19888 -WarningAction SilentlyContinue
        return [bool]$result.TcpTestSucceeded
    }
    catch {
        return $false
    }
}

function Stop-EshopAppHostProcesses {
    Get-Process -Name "eShop.AppHost" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

    $dotnetAppHost = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -match "src\\eShop.AppHost\\eShop.AppHost.csproj" }

    foreach ($proc in $dotnetAppHost) {
        Stop-Process -Id $proc.ProcessId -Force -ErrorAction SilentlyContinue
    }
}

function Start-AppHostForDashboardToken {
    if (Test-DashboardPortOpen) {
        Write-Host "Dashboard port already in use. Restarting AppHost to mint a fresh token..."
        Stop-EshopAppHostProcesses
        Start-Sleep -Seconds 1
    }

    Remove-Item $dashboardUrlFile -ErrorAction SilentlyContinue
    Remove-Item $appHostLogFile -ErrorAction SilentlyContinue

    Start-Process powershell -ArgumentList @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", $runLocalScript,
        "-SkipPrereqCheck"
    ) -WorkingDirectory $repoRoot | Out-Null
}

$dashboardUrl = Get-DashboardLoginUrl
if ($dashboardUrl) {
    Start-Process $dashboardUrl
    exit 0
}

Write-Host "No cached dashboard token found. Starting AppHost to obtain one..."
Start-AppHostForDashboardToken

$timeoutSeconds = 45
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
while ($stopwatch.Elapsed.TotalSeconds -lt $timeoutSeconds) {
    Start-Sleep -Milliseconds 500
    $dashboardUrl = Get-DashboardLoginUrl
    if ($dashboardUrl) {
        Start-Process $dashboardUrl
        exit 0
    }
}

Write-Warning "Tokenized login URL was not discovered in time. Opening dashboard root URL."
Start-Process "https://localhost:19888"
