$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$EnvFile = Join-Path $Root ".env"

if (-not (Test-Path $EnvFile)) {
  throw "Missing .env at repo root: $EnvFile"
}

function Load-DotEnv($path) {
  Get-Content $path | ForEach-Object {
    $line = $_.Trim()
    if ($line -and -not $line.StartsWith("#")) {
      $parts = $line.Split("=",2)
      if ($parts.Count -eq 2) {
        [Environment]::SetEnvironmentVariable($parts[0].Trim(), $parts[1].Trim())
      }
    }
  }
}

Load-DotEnv $EnvFile

$ADMIN_TOKEN = [Environment]::GetEnvironmentVariable("ADMIN_TOKEN")
$REDIS_CONNECTION_STRING = [Environment]::GetEnvironmentVariable("REDIS_CONNECTION_STRING")
$SAM_PORT = [Environment]::GetEnvironmentVariable("SAM_PORT")
if (-not $SAM_PORT) { $SAM_PORT = "3000" }
$REDIS_PORT = [Environment]::GetEnvironmentVariable("REDIS_PORT")
if (-not $REDIS_PORT) { $REDIS_PORT = "6379" }

if (-not $ADMIN_TOKEN) { throw "Missing ADMIN_TOKEN in .env" }
if (-not $REDIS_CONNECTION_STRING) { throw "Missing REDIS_CONNECTION_STRING in .env" }

& (Join-Path $Root "scripts/dev-down.ps1")

Write-Host "Starting Redis fresh on port $REDIS_PORT..."
docker run -d --name rat-redis -p "$REDIS_PORT`:6379" redis | Out-Null

Write-Host "Waiting for Redis to be ready..."
$ready = $false
for ($i=0; $i -lt 20; $i++) {
  try {
    $pong = docker run --rm redis redis-cli -h host.docker.internal -p $REDIS_PORT ping 2>$null
    if ($pong -match "PONG") { $ready = $true; break }
  } catch {}
  Start-Sleep -Milliseconds 500
}
if (-not $ready) { throw "Redis did not become ready in time." }

$Backend = Join-Path $Root "backend"
Set-Location $Backend

Write-Host "Building SAM..."
sam build

$EnvJson = Join-Path $Backend "env.local.json"

$envObj = @{
  RatLimiterFunction = @{
    ADMIN_TOKEN = $ADMIN_TOKEN
    REDIS_CONNECTION_STRING = $REDIS_CONNECTION_STRING
  }
}

$json = $envObj | ConvertTo-Json -Depth 5

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($EnvJson, $json, $utf8NoBom)

Get-Content $EnvJson | Select-Object -First 1 | Write-Host

# sanity check
if ((Get-Item $EnvJson).Length -lt 5) {
  throw "env.local.json was written but is empty/too small: $EnvJson"
}

Write-Host "Starting SAM local API on http://127.0.0.1:$SAM_PORT"
Write-Host "Press Ctrl+C to stop."
sam local start-api --port $SAM_PORT --env-vars $EnvJson
