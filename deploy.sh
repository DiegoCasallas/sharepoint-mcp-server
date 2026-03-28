#!/bin/bash

# Script de despliegue para SharePoint MCP Server

echo "🚀 Desplegando SharePoint MCP Server..."

# Verificar variables de entorno
if [ -z "$SP_CLIENT_ID" ] || [ -z "$SP_CLIENT_SECRET" ] || [ -z "$SP_TENANT_ID" ]; then
    echo "❌ Error: Las variables de entorno SP_CLIENT_ID, SP_CLIENT_SECRET y SP_TENANT_ID son requeridas"
    echo "   Ejemplo:"
    echo "   export SP_CLIENT_ID='your-client-id'"
    echo "   export SP_CLIENT_SECRET='your-client-secret'"
    echo "   export SP_TENANT_ID='your-tenant-id'"
    exit 1
fi

# Opción 1: Ejecutar directamente
if [ "$1" = "run" ]; then
    echo "▶️  Ejecutando servidor MCP..."
    ./sharepoint-mcp-server
    exit 0
fi

# Opción 2: Despliegue con Docker
if [ "$1" = "docker" ]; then
    echo "🐳 Construyendo imagen Docker..."
    docker build -t sharepoint-mcp-server .
    
    echo "🚀 Ejecutando contenedor Docker..."
    docker run -d \
        --name sharepoint-mcp-server \
        --restart unless-stopped \
        -e SP_CLIENT_ID="$SP_CLIENT_ID" \
        -e SP_CLIENT_SECRET="$SP_CLIENT_SECRET" \
        -e SP_TENANT_ID="$SP_TENANT_ID" \
        sharepoint-mcp-server
    
    echo "✅ Contenedor desplegado. Verificar logs con: docker logs sharepoint-mcp-server"
    exit 0
fi

# Opción 3: Docker Compose
if [ "$1" = "compose" ]; then
    echo "🐳 Desplegando con Docker Compose..."
    cp .env.example .env
    echo "⚠️  Por favor edita el archivo .env con tus credenciales reales"
    echo "📝 Luego ejecuta: docker-compose up -d"
    exit 0
fi

# Mostrar ayuda
echo "Uso: $0 [run|docker|compose]"
echo ""
echo "Opciones:"
echo "  run     - Ejecutar el servidor directamente"
echo "  docker  - Construir y ejecutar con Docker"
echo "  compose - Preparar Docker Compose"
echo ""
echo "Variables de entorno requeridas:"
echo "  SP_CLIENT_ID"
echo "  SP_CLIENT_SECRET" 
echo "  SP_TENANT_ID"
