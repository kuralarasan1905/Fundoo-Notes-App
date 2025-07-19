# Fundoo Notes Database Setup Script
# This script helps set up the database for the Fundoo Notes application

param(
    [string]$ServerInstance = "KURALARASAN\\SQLEXPRESS",
    [string]$Database = "FundooNotesDB",
    [string]$Username = "appuser",
    [string]$Password = "kural1905"
)

Write-Host "Fundoo Notes Database Setup" -ForegroundColor Green
Write-Host "===========================" -ForegroundColor Green
Write-Host ""

Write-Host "Database Configuration:" -ForegroundColor Yellow
Write-Host "  Server: $ServerInstance" -ForegroundColor Cyan
Write-Host "  Database: $Database" -ForegroundColor Cyan
Write-Host "  Username: $Username" -ForegroundColor Cyan
Write-Host ""

# Test SQL Server connection
Write-Host "Testing SQL Server connection..." -ForegroundColor Yellow

try {
    $connectionString = "Server=$ServerInstance;Database=master;User Id=$Username;Password=$Password;Connection Timeout=10;Encrypt=False;TrustServerCertificate=True;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $connection.Close()
    Write-Host "SQL Server connection successful" -ForegroundColor Green
} catch {
    Write-Host " SQL Server connection failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check the following:" -ForegroundColor Yellow
    Write-Host "  1. SQL Server is running and accessible at $ServerInstance" -ForegroundColor White
    Write-Host "  2. User '$Username' exists and has appropriate permissions" -ForegroundColor White
    Write-Host "  3. SQL Server is configured to allow remote connections" -ForegroundColor White
    Write-Host "  4. Windows Firewall allows connections on port 1433" -ForegroundColor White
    Write-Host ""
    Write-Host "Manual Setup Instructions:" -ForegroundColor Yellow
    Write-Host "  1. Open SQL Server Management Studio" -ForegroundColor White
    Write-Host "  2. Connect to your SQL Server instance" -ForegroundColor White
    Write-Host "  3. Open the file: Database/FundooNotesDB_CreateTables.sql" -ForegroundColor White
    Write-Host "  4. Execute the script to create the database and tables" -ForegroundColor White
    exit 1
}

# Check if sqlcmd is available
$sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue

if ($sqlcmdPath) {
    Write-Host "Found sqlcmd, attempting to create database..." -ForegroundColor Yellow
    
    $scriptPath = Join-Path $PSScriptRoot "..\Database\FundooNotesDB_CreateTables.sql"
    
    if (Test-Path $scriptPath) {
        try {
            sqlcmd -S $ServerInstance -U $Username -P $Password -i $scriptPath
            Write-Host "Database setup completed successfully" -ForegroundColor Green
        } catch {
            Write-Host " Database setup failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host " Database script not found at: $scriptPath" -ForegroundColor Red
    }
} else {
    Write-Host "sqlcmd not found. Please run the database script manually:" -ForegroundColor Yellow
    Write-Host "  1. Open SQL Server Management Studio" -ForegroundColor White
    Write-Host "  2. Connect to $ServerInstance" -ForegroundColor White
    Write-Host "  3. Open: Database/FundooNotesDB_CreateTables.sql" -ForegroundColor White
    Write-Host "  4. Execute the script" -ForegroundColor White
}

Write-Host ""
Write-Host "Database setup process completed." -ForegroundColor Green
Write-Host "You can now start the application using: .\Scripts\start-application.ps1" -ForegroundColor Cyan
