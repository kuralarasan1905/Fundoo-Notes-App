# Detailed health check test
Write-Host "Testing health endpoint in detail..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5227/api/health" -Method Get -UseBasicParsing
    Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Content: $($response.Content)" -ForegroundColor Green
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}
