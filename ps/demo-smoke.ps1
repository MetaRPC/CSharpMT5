param(
  [string]$Profile = 'demo',
  [string]$Symbol  = 'GBPUSD',
  [double]$Volume  = 0.10,
  [int]   $Timeout = 90000
)

# Pick up shortcuts
. "$PSScriptRoot\shortcasts.ps1"

Write-Host "=== Smoke test: $Profile / $Symbol / vol=$Volume ==="

# 1) Account Information
info

# 2) Make sure that the symbol is visible
q -s $Symbol | Out-Null   # quick tick (check at the same time)

# 3) Purchase
b -s $Symbol -v $Volume

# 4) Positions
positions

# 5) Close by symbol
cs -s $Symbol

Write-Host "=== Done."

#Starting from the root of the project:

#Unblock-File .\ps\demo-smoke.ps1
#.\ps\demo-smoke.ps1                                 # with defaults
#.\ps\demo-smoke.ps1 -Symbol EURUSD -Volume 0.2
#.\ps\demo-smoke.ps1 -Profile demo -Timeout 120000


