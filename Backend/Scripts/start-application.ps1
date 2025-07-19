# Fundoo Notes Application Startup Script
# This script helps start the application with proper configuration

param(
    [string]$Environment = "Development",
    [string]$Port = "5139",
    [string]$HttpsPort = "7139"
)

Write-Host "Starting Fundoo Notes Application..." -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "HTTP Port: $Port" -ForegroundColor Yellow
Write-Host "HTTPS Port: $HttpsPort" -ForegroundColor Yellow

# Set environment variables
$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:ASPNETCORE_URLS = "https://localhost:$HttpsPort;http://localhost:$Port"

# Check if SQL Server is accessible
Write-Host "Checking SQL Server connection..." -ForegroundColor Yellow

try {
    # DEVELOPMENT: Test SQL Server connection using Windows Authentication
    # NOTE: This connection string is for development testing only
    # Production deployments should use the connection string from appsettings.Production.json
    $connectionString = "Server=KURALARASAN\\SQLEXPRESS;Database=master;Integrated Security=true;Connection Timeout=5;Encrypt=False;TrustServerCertificate=True;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $connection.Close()
    Write-Host "SQL Server connection successful (Development Mode)" -ForegroundColor Green
} catch {
    Write-Host "SQL Server connection failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please ensure SQL Server is running and accessible at KURALARASAN\\SQLEXPRESS" -ForegroundColor Yellow
    Write-Host "For production deployment, update connection string in appsettings.Production.json" -ForegroundColor Yellow
    Write-Host "You can run the database creation script manually from SQL Server Management Studio" -ForegroundColor Yellow
}

# Build the application
Write-Host "Building application..." -ForegroundColor Yellow
dotnet build --configuration Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful" -ForegroundColor Green
    
    # Run the application
    Write-Host "Starting application..." -ForegroundColor Yellow
    Write-Host "API will be available at:" -ForegroundColor Cyan
    Write-Host "  - HTTPS: https://localhost:$HttpsPort" -ForegroundColor Cyan
    Write-Host "  - HTTP:  http://localhost:$Port" -ForegroundColor Cyan
    Write-Host "  - Swagger: https://localhost:$HttpsPort/swagger" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow
    Write-Host ""
    
    dotnet run --configuration Release
} else {
    Write-Host "Build failed" -ForegroundColor Red
    exit 1
}
