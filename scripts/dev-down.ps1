$ErrorActionPreference = "Stop"

Write-Host "Stopping/removing Redis container (rat-redis) if it exists..."
try {
  docker rm -f rat-redis 2>$null | Out-Null
} catch {
  # ignore: container doesn't exist
}

Write-Host "Stopping/removing SAM local containers (best-effort)..."
try {
  $samContainers = docker ps -aq --filter "label=com.amazonaws.sam.local=true"
  if ($samContainers) {
    docker rm -f $samContainers 2>$null | Out-Null
  }
} catch {
  # ignore: no containers / docker not ready
}

Write-Host "Done."
