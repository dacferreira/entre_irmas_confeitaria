using EntreIrmas.Domain.EnviromentVariables;

namespace EntreIrmas.Api.EnviromentVariables;

/// <summary>
/// Configuração da API, preenchida por <c>builder.Configuration.Get&lt;Variables&gt;()</c>.
/// A variável de ambiente sobrescreve o appsettings. Segredos (MYSQL_SENHA, JWT_CHAVE) ficam em
/// <c>dotnet user-secrets</c> no desenvolvimento e no ConfigMap/secret de cada ambiente.
/// </summary>
public class Variables :
    IMySqlEnvironmentVariables,
    IWhatsAppEnvironmentVariables,
    IPedidoEnvironmentVariables,
    ISwaggerEnvironmentVariables,
    ICorsEnvironmentVariables,
    IJwtEnvironmentVariables,
    IHealthCheckEnvironmentVariables
{
    // MySQL
    public string MYSQL_SERVIDOR { get; set; } = "localhost";
    public uint MYSQL_PORTA { get; set; } = 3306;
    public string MYSQL_BANCO { get; set; } = "entreirmas";
    public string MYSQL_USUARIO { get; set; } = string.Empty;
    public string MYSQL_SENHA { get; set; } = string.Empty;
    public string MYSQL_VERSAO { get; set; } = "8.0.36";
    public uint MYSQL_TIMEOUT_SEGUNDOS { get; set; } = 15;

    // WhatsApp
    public string WHATSAPP_NUMERO { get; set; } = string.Empty;

    // Pedidos
    public int PEDIDO_ANTECEDENCIA_MINIMA_DIAS { get; set; } = 1;
    public int PEDIDO_LIMITE_POR_MINUTO { get; set; } = 10;

    // Swagger
    public bool USE_SWAGGER { get; set; }

    // CORS
    public string CORS_ORIGENS { get; set; } = string.Empty;

    // JWT
    public string JWT_CHAVE { get; set; } = string.Empty;
    public string JWT_EMISSOR { get; set; } = "entreirmas-api";
    public string JWT_AUDIENCIA { get; set; } = "entreirmas-ui";

    // Health check
    public int HEALTHCHECK_TIMEOUT_SEGUNDOS { get; set; } = 5;
}
