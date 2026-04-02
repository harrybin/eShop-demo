param(
    [switch]$UseHttps,
    [switch]$SkipPrereqCheck
)

$ErrorActionPreference = "Stop"

if (-not $SkipPrereqCheck) {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw ".NET SDK is required. Install from https://dot.net/download"
    }

    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw "Docker Desktop is required. Install from https://docs.docker.com/desktop/"
    }

    try {
        docker info | Out-Null
    }
    catch {
        throw "Docker is installed but not running. Start Docker Desktop and retry."
    }
}

if ($UseHttps) {
    Remove-Item Env:ESHOP_USE_HTTP_ENDPOINTS -ErrorAction SilentlyContinue
    Write-Host "Running with HTTPS endpoints (default Aspire behavior)."
}
else {
    # Force all project endpoints to HTTP for simpler local development and test tooling.
    $env:ESHOP_USE_HTTP_ENDPOINTS = "true"
    Write-Host "Running with HTTP endpoints (ESHOP_USE_HTTP_ENDPOINTS=true)."
}

Write-Host "Starting eShop AppHost..."
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$aspireDir = Join-Path $repoRoot ".aspire"
$logFile = Join-Path $aspireDir "apphost.console.log"
$dashboardUrlFile = Join-Path $aspireDir "dashboard-login-url.txt"

New-Item -ItemType Directory -Path $aspireDir -Force | Out-Null

dotnet run --project src/eShop.AppHost/eShop.AppHost.csproj 2>&1 |
    Tee-Object -FilePath $logFile |
    ForEach-Object {
        $line = $_.ToString()
        if ($line -match "Login to the dashboard at\s+(https?://\S+)") {
            $dashboardUrl = $matches[1]
            Set-Content -Path $dashboardUrlFile -Value $dashboardUrl -Encoding utf8
        }
        Write-Host $line
    }
