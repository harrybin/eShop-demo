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
dotnet run --project src/eShop.AppHost/eShop.AppHost.csproj
