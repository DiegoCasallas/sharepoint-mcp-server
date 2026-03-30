using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Azure.Identity;
using Microsoft.Graph;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// MUY IMPORTANTE: Los logs van a stderr, NO a stdout
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Registrar GraphServiceClient
builder.Services.AddSingleton((serviceProvider) =>
{
    var credentials = new ClientSecretCredential(SharePointConfig.TenantId, SharePointConfig.ClientId, SharePointConfig.ClientSecret);
    return new GraphServiceClient(credentials);
});

builder.Services.AddScoped<HerramientasUtiles>();

builder.WebHost.UseUrls("http://0.0.0.0:5000");
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Prueba inicial - puedes comentar esto cuando uses el servidor MCP
    // var herramientasUtiles = scope.ServiceProvider.GetRequiredService<HerramientasUtiles>();
    // var resultado = await herramientasUtiles.ListarElementosSharePoint("Lugar");
    // Console.WriteLine(resultado);
}

app.MapMcp("/mcp");
await app.RunAsync();

public static class SharePointConfig
{
    public static readonly string SiteUrl = "https://epmco.sharepoint.com/sites/ARCAComitesJD-UAT";
    public static readonly string ClientId = Environment.GetEnvironmentVariable("SP_CLIENT_ID") ?? "your-client-id-here";
    public static readonly string ClientSecret = Environment.GetEnvironmentVariable("SP_CLIENT_SECRET") ?? "your-client-secret-here";
    public static readonly string TenantId = Environment.GetEnvironmentVariable("SP_TENANT_ID") ?? "your-tenant-id-here";
}

[McpServerToolType]
public static class HerramientasSaludo
{
    [McpServerTool, Description("Saluda al usuario por su nombre")]
    public static string Saludar(string nombre)
        => $"¡Hola {nombre}! Saludos desde el corazón de .NET!";
}

[McpServerToolType]
public class HerramientasUtiles
{
    private readonly GraphServiceClient _graphClient;

    public HerramientasUtiles(GraphServiceClient graphClient)
    {
        _graphClient = graphClient;
    }

    [McpServerTool, Description("Obtiene la fecha y hora actual del sistema")]
    public static string ObtenerFechaHora()
        => $"La fecha y hora actual es: {DateTime.Now:dddd, dd MMMM yyyy HH:mm:ss}";

    [McpServerTool, Description("Calcula cuántos días faltan para una fecha específica")]
    public static string DiasHastaFecha(
        [Description("Fecha en formato YYYY-MM-DD")] string fecha)
    {
        if (DateTime.TryParse(fecha, out var fechaObjetivo))
        {
            var dias = (fechaObjetivo - DateTime.Today).Days;
            if (dias < 0)
                return $"Esa fecha ya pasó hace {Math.Abs(dias)} días.";
            if (dias == 0)
                return "¡Esa fecha es hoy!";
            return $"Faltan {dias} días para el {fechaObjetivo:dd MMMM yyyy}.";
        }
        return "No pude entender esa fecha. Usa el formato YYYY-MM-DD, por ejemplo: 2025-12-31";
    }

    [McpServerTool, Description("Realiza operaciones matemáticas básicas")]
    public static string Calcular(
        [Description("Primer número")] double numero1,
        [Description("Operación: suma, resta, multiplica, divide")] string operacion,
        [Description("Segundo número")] double numero2)
    {
        var resultado = operacion.ToLower() switch
        {
            "suma" or "+" => numero1 + numero2,
            "resta" or "-" => numero1 - numero2,
            "multiplica" or "*" or "x" => numero1 * numero2,
            "divide" or "/" when numero2 != 0 => numero1 / numero2,
            "divide" or "/" => double.NaN,
            _ => double.NaN
        };

        if (double.IsNaN(resultado))
            return operacion.ToLower().Contains("div") && numero2 == 0
                ? "Error: No se puede dividir entre cero."
                : "Operación no reconocida. Usa: suma, resta, multiplica o divide.";

        return $"El resultado de {numero1} {operacion} {numero2} = {resultado}";
    }

    [McpServerTool, Description("Genera una contraseña aleatoria segura")]
    public static string GenerarPassword(
        [Description("Longitud de la contraseña (mínimo 8)")] int longitud = 16)
    {
        if (longitud < 8) longitud = 8;

        const string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&*";
        var random = new Random();
        var password = new string(Enumerable.Range(0, longitud)
            .Select(_ => caracteres[random.Next(caracteres.Length)])
            .ToArray());

        return $"Tu contraseña segura es: {password}";
    }

    [McpServerTool, Description("Lista elementos de cualquier lista de SharePoint usando Graph API")]
    public async Task<string> ListarElementosSharePoint(
        [Description("Nombre de la lista de SharePoint")] string listName,
        [Description("Nombre del sitio SharePoint (opcional, usa ARCAComitesJD-UAT por defecto)")] string siteName = "ARCAComitesJD-UAT")
    {
        try
        {
            // Construir la URL del sitio
            var siteHostname = "epmco.sharepoint.com";
            var sitePath = $"/sites/{siteName}";
            
            // Obtener el site ID desde la URL
            var site = await _graphClient.Sites[siteHostname + ":" + sitePath].GetAsync();
            
            // Buscar la lista por su nombre
            var lists = await _graphClient.Sites[site.Id].Lists.GetAsync();
            var targetList = lists?.Value?.FirstOrDefault(l => l.DisplayName.Equals(listName, StringComparison.OrdinalIgnoreCase));
            
            if (targetList == null)
            {
                return $"La lista '{listName}' no fue encontrada en el sitio '{siteName}'.";
            }

            // Obtener los elementos de la lista
            var listItems = await _graphClient.Sites[site.Id].Lists[targetList.Id].Items.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Expand = new string[] { "fields" };
            });

            if (listItems?.Value?.Any() == true)
            {
                var result = $"Elementos en la lista '{listName}' del sitio '{siteName}':\n";
                result += $"Total de elementos: {listItems.Value.Count()}\n\n";
                
                foreach (var item in listItems.Value)
                {
                    result += $"- ID: {item.Id}\n";
                    
                    // Mostrar todos los campos disponibles
                    if (item.Fields.AdditionalData != null)
                    {
                        foreach (var field in item.Fields.AdditionalData)
                        {
                            var value = field.Value?.ToString() ?? "null";
                            // Truncar valores largos
                            if (value.Length > 100)
                                value = value.Substring(0, 97) + "...";
                            result += $"  {field.Key}: {value}\n";
                        }
                    }
                    result += "\n";
                }
                return result;
            }
            else
            {
                return $"La lista '{listName}' del sitio '{siteName}' no contiene elementos.";
            }
        }
        catch (Exception ex)
        {
            return $"Error al acceder a SharePoint via Graph API: {ex.Message}\nDetalles: {ex.StackTrace}";
        }
    }
}
