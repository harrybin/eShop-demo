# Local Setup and Troubleshooting

This guide is for running eShop locally from the repository root.

## Prerequisites check (dotnet, docker)

Run these commands in PowerShell:

```powershell
dotnet --info
docker --version
docker info
```

Expected:
- `dotnet --info` prints SDK/runtime details (repo targets .NET 9).
- `docker --version` prints Docker client version.
- `docker info` succeeds (fails if Docker Desktop is not running).

Quick script-based prereq check:

```powershell
./build/local/run-local.ps1
```

The script validates `dotnet` and `docker` unless `-SkipPrereqCheck` is used.

## Run full app and local-only run commands

From repo root:

```powershell
# Default AppHost run
dotnet run --project src/eShop.AppHost/eShop.AppHost.csproj

# Local-only helper (HTTP endpoints, fewer cert issues)
./build/local/run-local.ps1

# Local helper with HTTPS behavior
./build/local/run-local.ps1 -UseHttps

# Skip prereq checks (use only if you already validated tools)
./build/local/run-local.ps1 -SkipPrereqCheck
```

## Access Aspire dashboard

Option 1: Use VS Code task `Open: Aspire Dashboard`.

Option 2: Run script directly:

```powershell
./build/local/open-aspire-dashboard.ps1
```

The AppHost startup also writes a tokenized dashboard URL to:

```text
.aspire/dashboard-login-url.txt
```

If no token is available yet, open:

```text
https://localhost:19888
```

## Run individual services vs full suite

Use VS Code tasks in `.vscode/tasks.json`:

Individual services:
- `Run: Basket API`
- `Run: Catalog API`
- `Run: Identity API`
- `Run: Ordering API`
- `Run: Webhooks API`
- `Run: Order Processor`
- `Run: Payment Processor`

Grouped/full runs:
- `Run: Application Parts` (runs API/processor tasks in parallel)
- `Run: All (AppHost)` (runs `build/local/run-local.ps1 -SkipPrereqCheck`)
- `Run: UI (WebApp)` (also starts AppHost via local script)
- `Open: UI in Browser` (opens `http://localhost:5045`)

Tip:
- Use `Run: All (AppHost)` for normal local development.
- Use individual tasks when isolating one service failure.

## Health check URLs and expected responses

Health endpoints are mapped by service defaults:
- `/alive` is available in all environments.
- `/health` is mapped only in Development.

Examples (replace `<service-port>` with the actual port shown in Aspire):

```text
http://localhost:<service-port>/alive
http://localhost:<service-port>/health
```

Expected responses:
- `GET /alive`: HTTP 200 with health payload/text such as `Healthy` when live checks pass.
- `GET /health` in Development: HTTP 200 with health payload/text such as `Healthy` when all checks pass.
- `GET /health` outside Development: typically HTTP 404 (endpoint not mapped).

## Common issues

### Port conflicts

Symptoms:
- Startup fails with address/port already in use.
- UI or API URLs do not bind.

Actions:
- Stop old app instances/terminals.
- Check listeners:

```powershell
Get-NetTCPConnection -State Listen | Where-Object { $_.LocalPort -in 5045,19888 }
```

### Docker not running

Symptoms:
- Startup error from `run-local.ps1` or container-related failures.

Actions:
- Start Docker Desktop.
- Verify:

```powershell
docker info
```

### Startup timing (first run is slower)

Symptoms:
- UI unavailable for 1-3 minutes.
- Services show transient failures during warm-up.

Actions:
- Wait for build/container warm-up.
- Monitor logs:

```powershell
Get-Content ./.aspire/apphost.console.log -Wait
```

### Certificate friction (HTTPS)

Symptoms:
- Browser cert warnings or HTTPS endpoint failures.

Actions:
- Prefer HTTP local mode:

```powershell
./build/local/run-local.ps1
```

- Or trust dev cert and retry:

```powershell
dotnet dev-certs https --trust
```

Then close and reopen browser tabs.

### Migration issues

Symptoms:
- Service starts but fails on DB init/migrations.

Actions:
- Check startup logs first (`.aspire/apphost.console.log`).
- If you are actively changing Ordering schema, use EF commands from `src/Ordering.Infrastructure`:

```powershell
dotnet ef migrations list --startup-project ../Ordering.API --context OrderingContext
```

## Quick troubleshooting commands

Run from repo root unless noted:

```powershell
# Verify tools
dotnet --info
docker info

# Build once to catch compile errors early
dotnet build eShop.slnx

# Start local run
./build/local/run-local.ps1

# Tail AppHost logs
Get-Content ./.aspire/apphost.console.log -Wait

# Probe WebApp
Invoke-WebRequest http://localhost:5045 -UseBasicParsing

# Probe a service health endpoint (replace port)
Invoke-WebRequest http://localhost:<service-port>/alive -UseBasicParsing

# Check common local ports
Get-NetTCPConnection -State Listen | Where-Object { $_.LocalPort -in 5045,19888 }

# List running dotnet processes (helpful for stale hosts)
Get-Process dotnet
```
