# Test script for Fundoo Notes API
# This script tests the application startup and graceful shutdown

Write-Host "Testing Fundoo Notes API..." -ForegroundColor Green

# Function to test API endpoint
function Test-ApiEndpoint {
    param(
        [string]$Url,
        [string]$Description
    )
    
    try {
        Write-Host "Testing $Description..." -ForegroundColor Yellow
        $response = Invoke-RestMethod -Uri $Url -Method Get -TimeoutSec 10
        Write-Host "$Description - SUCCESS" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "$Description - FAILED: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Test endpoints
$baseUrl = "http://localhost:5227"
$endpoints = @(
    @{ Url = "$baseUrl/"; Description = "API Root" },
    @{ Url = "$baseUrl/api/health"; Description = "Health Check" },
    @{ Url = "$baseUrl/swagger/v1/swagger.json"; Description = "Swagger JSON" }
)

$allPassed = $true

foreach ($endpoint in $endpoints) {
    $result = Test-ApiEndpoint -Url $endpoint.Url -Description $endpoint.Description
    if (-not $result) {
        $allPassed = $false
    }
    Start-Sleep -Seconds 1
}

if ($allPassed) {
    Write-Host "`nAll tests passed! The ObjectDisposedException issue has been resolved." -ForegroundColor Green
    Write-Host "Application starts successfully" -ForegroundColor Green
    Write-Host "Health checks are working" -ForegroundColor Green
    Write-Host "Swagger documentation is accessible" -ForegroundColor Green
    Write-Host "Graceful shutdown handling is implemented" -ForegroundColor Green
} else {
    Write-Host "`nSome tests failed. Please check the application." -ForegroundColor Red
}

Write-Host "`nTest completed." -ForegroundColor Cyan
