# Use .NET 8 runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Use .NET 8 SDK for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["MiPrimerMCP.csproj", "."]
RUN dotnet restore "MiPrimerMCP.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "MiPrimerMCP.csproj" -c Release -o /app/publish

# Final stage
FROM base
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables (these should be provided at runtime)
ENV SP_CLIENT_ID=""
ENV SP_CLIENT_SECRET=""
ENV SP_TENANT_ID=""

# Expose port (if needed for health checks)
EXPOSE 8080

# Run the application
ENTRYPOINT ["./sharepoint-mcp-server"]
