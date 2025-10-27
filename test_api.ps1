# Test API endpoints
Write-Host "Testing API endpoints..."

# Test health endpoint
try {
    Write-Host "`n=== Testing Health Endpoint ==="
    $health = Invoke-RestMethod -Uri "http://localhost:5001/health"
    Write-Host "✅ Health check successful: $health"
} catch {
    Write-Host "❌ Health check failed: $($_.Exception.Message)"
}

# Test auth endpoint with /api/auth/login
try {
    Write-Host "`n=== Testing /api/auth/login ==="
    $body = @{
        username = "admin"
        password = "Admin123!"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "http://localhost:5001/api/auth/login" -Method POST -ContentType "application/json" -Body $body
    Write-Host "✅ /api/auth/login successful!"
    Write-Host "   Username: $($response.user.username)"
    Write-Host "   Role: $($response.user.role)"
} catch {
    Write-Host "❌ /api/auth/login failed: $($_.Exception.Message)"
}

# Test auth endpoint with /api/v1/auth/login
try {
    Write-Host "`n=== Testing /api/v1/auth/login ==="
    $body = @{
        username = "admin"
        password = "Admin123!"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "http://localhost:5001/api/v1/auth/login" -Method POST -ContentType "application/json" -Body $body
    Write-Host "✅ /api/v1/auth/login successful!"
    Write-Host "   Username: $($response.user.username)"
    Write-Host "   Role: $($response.user.role)"
} catch {
    Write-Host "❌ /api/v1/auth/login failed: $($_.Exception.Message)"
}

# Test with viewer user
try {
    Write-Host "`n=== Testing viewer login ==="
    $body = @{
        username = "viewer"
        password = "Viewer123!"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "http://localhost:5001/api/v1/auth/login" -Method POST -ContentType "application/json" -Body $body
    Write-Host "✅ Viewer login successful!"
    Write-Host "   Username: $($response.user.username)"
    Write-Host "   Role: $($response.user.role)"
} catch {
    Write-Host "❌ Viewer login failed: $($_.Exception.Message)"
}
