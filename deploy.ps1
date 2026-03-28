# Script de despliegue para SharePoint MCP Server (PowerShell)

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("run", "docker", "compose")]
    [string]$Action = "help"
)

Write-Host "🚀 Desplegando SharePoint MCP Server..." -ForegroundColor Green

# Verificar variables de entorno
$clientId = $env:SP_CLIENT_ID
$clientSecret = $env:SP_CLIENT_SECRET
$tenantId = $env:SP_TENANT_ID

if (-not $clientId -or -not $clientSecret -or -not $tenantId) {
    Write-Host "❌ Error: Las variables de entorno SP_CLIENT_ID, SP_CLIENT_SECRET y SP_TENANT_ID son requeridas" -ForegroundColor Red
    Write-Host "   Ejemplo:" -ForegroundColor Yellow
    Write-Host "   `$env:SP_CLIENT_ID='your-client-id'" -ForegroundColor Yellow
    Write-Host "   `$env:SP_CLIENT_SECRET='your-client-secret'" -ForegroundColor Yellow
    Write-Host "   `$env:SP_TENANT_ID='your-tenant-id'" -ForegroundColor Yellow
    exit 1
}

# Opción 1: Ejecutar directamente
if ($Action -eq "run") {
    Write-Host "▶️  Ejecutando servidor MCP..." -ForegroundColor Blue
    .\sharepoint-mcp-server
    exit 0
}

# Opción 2: Despliegue con Docker
if ($Action -eq "docker") {
    Write-Host "🐳 Construyendo imagen Docker..." -ForegroundColor Blue
    docker build -t sharepoint-mcp-server .
    
    Write-Host "🚀 Ejecutando contenedor Docker..." -ForegroundColor Blue
    docker run -d `
        --name sharepoint-mcp-server `
        --restart unless-stopped `
        -e SP_CLIENT_ID="$clientId" `
        -e SP_CLIENT_SECRET="$clientSecret" `
        -e SP_TENANT_ID="$tenantId" `
        sharepoint-mcp-server
    
    Write-Host "✅ Contenedor desplegado. Verificar logs con: docker logs sharepoint-mcp-server" -ForegroundColor Green
    exit 0
}

# Opción 3: Docker Compose
if ($Action -eq "compose") {
    Write-Host "🐳 Preparando Docker Compose..." -ForegroundColor Blue
    Copy-Item ".env.example" ".env"
    Write-Host "⚠️  Por favor edita el archivo .env con tus credenciales reales" -ForegroundColor Yellow
    Write-Host "📝 Luego ejecuta: docker-compose up -d" -ForegroundColor Yellow
    exit 0
}

# Mostrar ayuda
Write-Host "Uso: .\deploy.ps1 [-Action run|docker|compose]" -ForegroundColor Cyan
Write-Host ""
Write-Host "Opciones:" -ForegroundColor White
Write-Host "  run     - Ejecutar el servidor directamente" -ForegroundColor White
Write-Host "  docker  - Construir y ejecutar con Docker" -ForegroundColor White
Write-Host "  compose - Preparar Docker Compose" -ForegroundColor White
Write-Host ""
Write-Host "Variables de entorno requeridas:" -ForegroundColor White
Write-Host "  SP_CLIENT_ID" -ForegroundColor Gray
Write-Host "  SP_CLIENT_SECRET" -ForegroundColor Gray
Write-Host "  SP_TENANT_ID" -ForegroundColor Gray
