param(
    [Parameter(Mandatory=$false)]
    [int]$Port = 5227
)

Write-Host "Fundoo Notes API Port Manager" -ForegroundColor Green
Write-Host "==============================" -ForegroundColor Green
Write-Host ""

if ($Port -eq 0) {
    Write-Host "Usage: .\run-with-port.ps1 [-Port <port>]" -ForegroundColor Yellow
    Write-Host "Example: .\run-with-port.ps1 -Port 5227" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Available ports:" -ForegroundColor Cyan
    Write-Host "  5227 - Default VS Code port" -ForegroundColor White
    Write-Host "  5228 - Visual Studio port" -ForegroundColor White
    Write-Host "  5229 - Alternative port" -ForegroundColor White
    exit
}

Write-Host "Starting Fundoo Notes API on port $Port..." -ForegroundColor Yellow
Write-Host ""

# Kill any existing processes on the port
try {
    $processes = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    if ($processes) {
        foreach ($process in $processes) {
            $pid = $process.OwningProcess
            Write-Host "Killing process $pid using port $Port" -ForegroundColor Red
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        }
        Start-Sleep -Seconds 2
    }
} catch {
    Write-Host "No existing processes found on port $Port" -ForegroundColor Green
}

# Set environment variable and run
$env:ASPNETCORE_PORT = $Port.ToString()
Write-Host "Environment variable ASPNETCORE_PORT set to: $env:ASPNETCORE_PORT" -ForegroundColor Cyan

try {
    dotnet run
} catch {
    Write-Host "Error running application: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
