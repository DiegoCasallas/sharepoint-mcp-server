# SharePoint MCP Server

Servidor MCP (Model Context Protocol) para acceder a listas de SharePoint usando Microsoft Graph API.

## Características

- Acceso a cualquier lista de SharePoint en tu dominio
- Soporte para múltiples sitios
- Autenticación segura via Azure AD
- Despliegue via Docker
- Configuración por variables de entorno

## Configuración

### 1. Variables de Entorno

Crea un archivo `.env` basado en `.env.example`:

```bash
cp .env.example .env
```

Edita `.env` con tus credenciales:

```env
SP_CLIENT_ID=tu-client-id
SP_CLIENT_SECRET=tu-client-secret
SP_TENANT_ID=tu-tenant-id
```

### 2. Ejecución Local

```bash
# Compilar y ejecutar
dotnet run

# O publicar como single executable
dotnet publish -c Release -r linux-x64 --self-contained
```

### 3. Despliegue con Docker

```bash
# Construir imagen
docker build -t sharepoint-mcp-server .

# Ejecutar con variables de entorno
docker run -e SP_CLIENT_ID="$SP_CLIENT_ID" \
           -e SP_CLIENT_SECRET="$SP_CLIENT_SECRET" \
           -e SP_TENANT_ID="$SP_TENANT_ID" \
           sharepoint-mcp-server

# O con docker-compose
cp .env.example .env
# Editar .env con valores reales
docker-compose up -d
```

## Uso como Herramienta MCP

El servidor expone las siguientes herramientas:

### `ListarElementosSharePoint`
Lista elementos de cualquier lista de SharePoint.

Parámetros:
- `listName` (requerido): Nombre de la lista
- `siteName` (opcional): Nombre del sitio (default: "ARCAComitesJD-UAT")

Ejemplo:
```json
{
  "tool": "ListarElementosSharePoint",
  "arguments": {
    "listName": "Lugar",
    "siteName": "ARCAComitesJD-UAT"
  }
}
```

### Herramientas Adicionales

- `Saludar`: Saludo simple
- `ObtenerFechaHora`: Fecha y hora actual
- `DiasHastaFecha`: Días hasta una fecha específica
- `Calcular`: Operaciones matemáticas básicas
- `GenerarPassword`: Generador de contraseñas seguras

## Despliegue en la Nube

### Opción 1: VPS/VM (DigitalOcean, Linode, etc.)

1. Instala Docker
2. Copia los archivos del proyecto
3. Configura las variables de entorno
4. Ejecuta `docker-compose up -d`

### Opción 2: Railway/Vercel/Render

1. Conecta tu repo Git
2. Configura las variables de entorno en el dashboard
3. Despliega automáticamente

### Opción 3: Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: sharepoint-mcp-server
spec:
  replicas: 1
  selector:
    matchLabels:
      app: sharepoint-mcp-server
  template:
    metadata:
      labels:
        app: sharepoint-mcp-server
    spec:
      containers:
      - name: sharepoint-mcp-server
        image: sharepoint-mcp-server:latest
        env:
        - name: SP_CLIENT_ID
          valueFrom:
            secretKeyRef:
              name: sharepoint-secrets
              key: client-id
        - name: SP_CLIENT_SECRET
          valueFrom:
            secretKeyRef:
              name: sharepoint-secrets
              key: client-secret
        - name: SP_TENANT_ID
          valueFrom:
            secretKeyRef:
              name: sharepoint-secrets
              key: tenant-id
```

## Seguridad

- Nunca expongas las credenciales en el código
- Usa variables de entorno o secrets management
- Considera usar Azure Key Vault para producción
- Rotación regular de secrets

## Troubleshooting

### Errores Comunes

1. **Authentication failed**: Verifica que las credenciales sean correctas
2. **List not found**: Confirma el nombre exacto de la lista y sitio
3. **Permission denied**: Asegura que la app tenga permisos en SharePoint

### Logs

Los logs se envían a stderr para compatibilidad con MCP.

## Contribuir

1. Fork el repo
2. Crea una feature branch
3. Submit un pull request
