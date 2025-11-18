# Test Database Initialization Scripts
# This script validates that all module database scripts execute successfully

param(
    [string]$PostgresPassword = "development123",
    [string]$PostgresUser = "postgres",
    [string]$PostgresDb = "meajudaai"
)

Write-Host "🧪 Testing Database Initialization Scripts" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
try {
    docker ps | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker is not running"
    }
}
catch {
    Write-Host "❌ Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Set environment variables
$env:POSTGRES_PASSWORD = $PostgresPassword
$env:POSTGRES_USER = $PostgresUser
$env:POSTGRES_DB = $PostgresDb

# Navigate to infrastructure/compose directory
$composeDir = Join-Path $PSScriptRoot "compose" "base"
if (-not (Test-Path $composeDir)) {
    Write-Host "❌ Compose directory not found: $composeDir" -ForegroundColor Red
    exit 1
}

try {
    Push-Location $composeDir
    Write-Host "🐳 Starting PostgreSQL container with initialization scripts..." -ForegroundColor Yellow
    Write-Host ""
    
    # Stop and remove existing container
    docker compose -f postgres.yml down -v 2>$null
    
    # Start container and wait for initialization
    docker compose -f postgres.yml up -d
    
    # Wait for PostgreSQL to be ready
    Write-Host "⏳ Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
    $maxAttempts = 30
    $attempt = 0
    $ready = $false
    
    while ($attempt -lt $maxAttempts -and -not $ready) {
        $attempt++
        Start-Sleep -Seconds 2
        
        $healthStatus = docker inspect --format='{{.State.Health.Status}}' meajudaai-postgres 2>$null
        if ($healthStatus -eq "healthy") {
            $ready = $true
            Write-Host "✅ PostgreSQL is ready!" -ForegroundColor Green
        }
        else {
            Write-Host "   Attempt $attempt/$maxAttempts - Status: $healthStatus" -ForegroundColor Gray
        }
    }
    
    if (-not $ready) {
        Write-Host "❌ PostgreSQL failed to start within timeout period" -ForegroundColor Red
        docker logs meajudaai-postgres
        exit 1
    }
    
    Write-Host ""
    Write-Host "🔍 Verifying database schemas..." -ForegroundColor Cyan
    
    # Test schemas
    $hasErrors = $false
    $schemas = @("users", "providers", "documents", "search", "location", "hangfire", "meajudaai_app")
    
    foreach ($schema in $schemas) {
        $query = "SELECT EXISTS(SELECT 1 FROM information_schema.schemata WHERE schema_name = '$schema');"
        $result = docker exec meajudaai-postgres psql -U $PostgresUser -d $PostgresDb -t -c $query
        
        if ($result.Trim() -eq "t") {
            Write-Host "   ✅ Schema '$schema' created successfully" -ForegroundColor Green
        }
        else {
            Write-Host "   ❌ Schema '$schema' NOT found" -ForegroundColor Red
            $hasErrors = $true
        }
    }
    
    Write-Host ""
    Write-Host "🔍 Verifying database roles..." -ForegroundColor Cyan
    
    # Test roles
    $roles = @(
        "users_role", "users_owner",
        "providers_role", "providers_owner",
        "documents_role", "documents_owner",
        "search_role", "search_owner",
        "location_role", "location_owner",
        "hangfire_role",
        "meajudaai_app_role", "meajudaai_app_owner"
    )
    
    foreach ($role in $roles) {
        $query = "SELECT EXISTS(SELECT 1 FROM pg_roles WHERE rolname = '$role');"
        $result = docker exec meajudaai-postgres psql -U $PostgresUser -d $PostgresDb -t -c $query
        
        if ($result.Trim() -eq "t") {
            Write-Host "   ✅ Role '$role' created successfully" -ForegroundColor Green
        }
        else {
            Write-Host "   ❌ Role '$role' NOT found" -ForegroundColor Red
            $hasErrors = $true
        }
    }
    
    Write-Host ""
    Write-Host "🔍 Verifying PostGIS extension..." -ForegroundColor Cyan
    
    $query = "SELECT EXISTS(SELECT 1 FROM pg_extension WHERE extname = 'postgis');"
    $result = docker exec meajudaai-postgres psql -U $PostgresUser -d $PostgresDb -t -c $query
    
    if ($result.Trim() -eq "t") {
        Write-Host "   ✅ PostGIS extension enabled" -ForegroundColor Green
    }
    else {
        Write-Host "   ❌ PostGIS extension NOT enabled" -ForegroundColor Red
        $hasErrors = $true
    }
    
    Write-Host ""
    Write-Host "📊 Database initialization logs:" -ForegroundColor Cyan
    Write-Host ""
    docker logs meajudaai-postgres 2>&1 | Select-String -Pattern "Initializing|Setting up|completed"
    
    Write-Host ""
    
    if ($hasErrors) {
        Write-Host "❌ Database validation failed! Some schemas, roles, or extensions are missing." -ForegroundColor Red
        Write-Host ""
        exit 1
    }
    
    Write-Host "✅ Database validation completed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "💡 To connect to the database:" -ForegroundColor Yellow
    Write-Host "   docker exec -it meajudaai-postgres psql -U $PostgresUser -d $PostgresDb" -ForegroundColor Gray
    Write-Host ""
    Write-Host "💡 To stop the container:" -ForegroundColor Yellow
    Write-Host "   docker compose -f $composeDir\postgres.yml down" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "❌ Error during validation: $_" -ForegroundColor Red
    exit 1
}
finally {
    # Return to original directory
    Pop-Location -ErrorAction SilentlyContinue
}
