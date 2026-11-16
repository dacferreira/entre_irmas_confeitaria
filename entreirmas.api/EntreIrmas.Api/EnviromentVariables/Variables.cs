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
    IViaCepEnvironmentVariables,
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

    // ViaCEP (consulta de endereço pelo CEP)
    public string VIACEP_URL { get; set; } = "https://viacep.com.br/ws/";
    public int VIACEP_TIMEOUT_SEGUNDOS { get; set; } = 5;
    public int VIACEP_CACHE_HORAS { get; set; } = 24;
    public int CEP_LIMITE_POR_MINUTO { get; set; } = 30;

    // Pedidos
    public int PEDIDO_ANTECEDENCIA_MINIMA_DIAS { get; set; } = 1;
    public int PEDIDO_LIMITE_POR_MINUTO { get; set; } = 10;
    public int ACOMPANHAMENTO_LIMITE_POR_MINUTO { get; set; } = 30;

    // Swagger
    public bool USE_SWAGGER { get; set; }

    // CORS
    public string CORS_ORIGENS { get; set; } = string.Empty;

    // JWT
    public string JWT_CHAVE { get; set; } = string.Empty;
    public string JWT_EMISSOR { get; set; } = "entreirmas-api";
    public string JWT_AUDIENCIA { get; set; } = "entreirmas-ui";
    public int JWT_EXPIRACAO_HORAS { get; set; } = 12;
    public int LOGIN_LIMITE_POR_MINUTO { get; set; } = 5;

    // Health check
    public int HEALTHCHECK_TIMEOUT_SEGUNDOS { get; set; } = 5;
}
