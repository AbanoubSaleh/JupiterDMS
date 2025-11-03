# Script to generate swagger.json for Azure deployment
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Generating Swagger JSON for Azure" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Build the project first
Write-Host "[1/4] Building project..." -ForegroundColor Yellow
dotnet build --configuration Release --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "SUCCESS: Build completed" -ForegroundColor Green
Write-Host ""

# Start the application in the background
Write-Host "[2/4] Starting application..." -ForegroundColor Yellow
$appProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --no-build --configuration Release --urls http://localhost:5555" -PassThru -WindowStyle Hidden

# Wait for application to start
Write-Host "Waiting for application to start (20 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 20

try {
    # Download swagger.json
    Write-Host "[3/4] Downloading swagger.json..." -ForegroundColor Yellow
    
    $swaggerUrl = "http://localhost:5555/swagger/v1/swagger.json"
    Write-Host "Fetching from: $swaggerUrl" -ForegroundColor Gray
    
    $response = Invoke-WebRequest -Uri $swaggerUrl -TimeoutSec 30
    $response.Content | Out-File -FilePath "swagger.json" -Encoding UTF8 -NoNewline
    
    if (Test-Path "swagger.json") {
        $fileSize = (Get-Item "swagger.json").Length
        Write-Host "SUCCESS: swagger.json created ($fileSize bytes)" -ForegroundColor Green
        Write-Host "Location: $(Get-Location)\swagger.json" -ForegroundColor Cyan
    } else {
        Write-Host "ERROR: swagger.json was not created" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "ERROR: Failed to download swagger.json" -ForegroundColor Red
    Write-Host "Error details: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Make sure the application started successfully" -ForegroundColor Yellow
    Write-Host "2. Check if port 5555 is available" -ForegroundColor Yellow
    Write-Host "3. Try accessing http://localhost:5555/swagger manually" -ForegroundColor Yellow
    exit 1
}
finally {
    # Stop the application
    Write-Host ""
    Write-Host "[4/4] Stopping application..." -ForegroundColor Yellow
    
    if ($appProcess -and !$appProcess.HasExited) {
        Stop-Process -Id $appProcess.Id -Force -ErrorAction SilentlyContinue
    }
    
    # Kill any remaining processes on port 5555
    $connections = netstat -ano | Select-String ":5555"
    if ($connections) {
        foreach ($connection in $connections) {
            $processId = ($connection -split '\s+')[-1]
            if ($processId -and $processId -ne "0") {
                Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
            }
        }
    }
    
    Write-Host "Application stopped" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Swagger JSON Generation Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. The swagger.json file is now in your project" -ForegroundColor White
Write-Host "2. It will be automatically included in your publish" -ForegroundColor White
Write-Host "3. You can now publish from Visual Studio" -ForegroundColor White
Write-Host ""

