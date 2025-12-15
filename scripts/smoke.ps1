$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$EnvFile = Join-Path $Root ".env"

if (-not (Test-Path $EnvFile)) { throw "Missing .env at repo root: $EnvFile" }

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

# read AFTER loading dotenv
$ADMIN_TOKEN = [Environment]::GetEnvironmentVariable("ADMIN_TOKEN")
$SAM_PORT = [Environment]::GetEnvironmentVariable("SAM_PORT")
if (-not $SAM_PORT) { $SAM_PORT = "3000" }

$baseUrl = "http://127.0.0.1:$SAM_PORT"

if (-not $ADMIN_TOKEN) { throw "ADMIN_TOKEN is missing (did .env load? is it set?)" }

Write-Host "Using baseUrl=$baseUrl"
Write-Host "ADMIN_TOKEN length=$($ADMIN_TOKEN.Length)"
