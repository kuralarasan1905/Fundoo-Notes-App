# Fundoo Notes Production Deployment Script
# This script builds and deploys the application for production use

param(
    [string]$OutputPath = ".\publish",
    [string]$Configuration = "Release",
    [switch]$SkipBuild = $false,
    [switch]$SkipDatabase = $false
)

Write-Host "Fundoo Notes Production Deployment" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Green
Write-Host ""

# Configuration
$projectPath = Get-Location
$publishPath = Join-Path $projectPath $OutputPath

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Project Path: $projectPath" -ForegroundColor Cyan
Write-Host "  Publish Path: $publishPath" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration" -ForegroundColor Cyan
Write-Host ""

# Step 1: Clean previous build
if (Test-Path $publishPath) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item $publishPath -Recurse -Force
    Write-Host "Previous build cleaned" -ForegroundColor Green
}

# Step 2: Build and publish
if (-not $SkipBuild) {
    Write-Host "Building application..." -ForegroundColor Yellow
    
    # Restore packages
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host " Package restore failed" -ForegroundColor Red
        exit 1
    }
    
    # Build
    dotnet build --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host " Build failed" -ForegroundColor Red
        exit 1
    }
    
    # Publish
    dotnet publish --configuration $Configuration --output $publishPath --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Host " Publish failed" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Application built and published successfully" -ForegroundColor Green
} else {
    Write-Host "Skipping build (--SkipBuild specified)" -ForegroundColor Yellow
}

# Step 3: Database setup
if (-not $SkipDatabase) {
    Write-Host "Setting up database..." -ForegroundColor Yellow
    
    # Test database connection
    try {
        $connectionString = "Server=KURALARASAN\\SQLEXPRESS;Database=master;User Id=appuser;Password=kural1905;Connection Timeout=10;Encrypt=False;TrustServerCertificate=True;"
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        $connection.Close()
        Write-Host "Database connection successful" -ForegroundColor Green
        
        # Run database script if sqlcmd is available
        $sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue
        if ($sqlcmdPath) {
            $scriptPath = Join-Path $projectPath "Database\FundooNotesDB_CreateTables.sql"
            if (Test-Path $scriptPath) {
                Write-Host "Executing database script..." -ForegroundColor Yellow
                sqlcmd -S "KURALARASAN\\SQLEXPRESS" -U "appuser" -P "kural1905" -i $scriptPath
                Write-Host "Database script executed" -ForegroundColor Green
            } else {
                Write-Host " Database script not found at: $scriptPath" -ForegroundColor Yellow
            }
        } else {
            Write-Host " sqlcmd not found. Please run database script manually." -ForegroundColor Yellow
        }
    } catch {
        Write-Host " Database connection failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Please ensure SQL Server is accessible and run the database script manually." -ForegroundColor Yellow
    }
} else {
    Write-Host "Skipping database setup (--SkipDatabase specified)" -ForegroundColor Yellow
}

# Step 4: Copy configuration files
Write-Host "Copying configuration files..." -ForegroundColor Yellow

$configFiles = @(
    "appsettings.Production.json",
    "Database\FundooNotesDB_CreateTables.sql"
)

foreach ($file in $configFiles) {
    $sourcePath = Join-Path $projectPath $file
    if (Test-Path $sourcePath) {
        $destPath = Join-Path $publishPath (Split-Path $file -Leaf)
        Copy-Item $sourcePath $destPath -Force
        Write-Host "  Copied $file" -ForegroundColor Green
    } else {
        Write-Host "   File not found: $file" -ForegroundColor Yellow
    }
}

# Step 5: Create startup scripts
Write-Host "Creating startup scripts..." -ForegroundColor Yellow

# Windows batch file
$batchContent = @"
@echo off
echo Starting Fundoo Notes API...
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=https://localhost:5001;http://localhost:5000
dotnet fundoo-notes.dll
pause
"@

$batchPath = Join-Path $publishPath "start.bat"
$batchContent | Out-File -FilePath $batchPath -Encoding ASCII
Write-Host "  Created start.bat" -ForegroundColor Green

# PowerShell script
$psContent = @"
# Fundoo Notes API Startup Script
Write-Host "Starting Fundoo Notes API..." -ForegroundColor Green
`$env:ASPNETCORE_ENVIRONMENT = "Production"
`$env:ASPNETCORE_URLS = "https://localhost:5001;http://localhost:5000"

Write-Host "API will be available at:" -ForegroundColor Cyan
Write-Host "  - HTTPS: https://localhost:5001" -ForegroundColor Cyan
Write-Host "  - HTTP:  http://localhost:5000" -ForegroundColor Cyan
Write-Host "  - Swagger: https://localhost:5001/swagger" -ForegroundColor Cyan
Write-Host ""

dotnet fundoo-notes.dll
"@

$psPath = Join-Path $publishPath "start.ps1"
$psContent | Out-File -FilePath $psPath -Encoding UTF8
Write-Host "  Created start.ps1" -ForegroundColor Green

# Step 6: Create deployment summary
Write-Host "Creating deployment summary..." -ForegroundColor Yellow

$summaryContent = @"
# Fundoo Notes - Deployment Summary

## Deployment Information
- **Date**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
- **Configuration**: $Configuration
- **Output Path**: $publishPath

## Files Included
- Application binaries and dependencies
- Configuration files (appsettings.Production.json)
- Database creation script
- Startup scripts (start.bat, start.ps1)

## Next Steps

### 1. Database Setup (if not done automatically)
Run the following in SQL Server Management Studio:
- Connect to: KURALARASAN\\SQLEXPRESS
- Username: appuser
- Password: kural1905
- Execute: FundooNotesDB_CreateTables.sql

### 2. Start the Application
Choose one of the following methods:

**Method A: Using batch file**
```
start.bat
```

**Method B: Using PowerShell**
```
.\start.ps1
```

**Method C: Manual**
```
set ASPNETCORE_ENVIRONMENT=Production
dotnet fundoo-notes.dll
```

### 3. Verify Deployment
- Health Check: https://localhost:5001/api/health
- Swagger UI: https://localhost:5001/swagger
- Test registration and login endpoints

### 4. Production Configuration
Update appsettings.Production.json with:
- Strong JWT secret key (minimum 32 characters)
- Production email settings
- Production database connection string
- HTTPS certificates and bindings

## API Endpoints
- **Base URL**: https://localhost:5001
- **Health**: /api/health
- **Auth**: /api/auth/register, /api/auth/login
- **Notes**: /api/notes (GET, POST, PUT, DELETE)
- **Labels**: /api/labels (GET, POST)
- **Search**: /api/notes/search

## Support
- Check logs for any startup issues
- Verify database connectivity
- Ensure all required ports are available
- Review API documentation in Swagger UI
"@

$summaryPath = Join-Path $publishPath "DEPLOYMENT_SUMMARY.md"
$summaryContent | Out-File -FilePath $summaryPath -Encoding UTF8
Write-Host "  Created DEPLOYMENT_SUMMARY.md" -ForegroundColor Green

# Final summary
Write-Host ""
Write-Host "Deployment completed successfully!" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host ""
Write-Host "Published to: $publishPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "To start the application:" -ForegroundColor Yellow
Write-Host "  1. Navigate to: $publishPath" -ForegroundColor White
Write-Host "  2. Run: .\start.ps1 or start.bat" -ForegroundColor White
Write-Host "  3. Access: https://localhost:5001/swagger" -ForegroundColor White
Write-Host ""
Write-Host "For detailed instructions, see: DEPLOYMENT_SUMMARY.md" -ForegroundColor Cyan
"@

$psPath = Join-Path $publishPath "start.ps1"
$psContent | Out-File -FilePath $psPath -Encoding UTF8
Write-Host "  Created start.ps1" -ForegroundColor Green
