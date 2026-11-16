namespace EntreIrmas.Api.EnviromentVariables;

public interface ISwaggerEnvironmentVariables
{
    bool USE_SWAGGER { get; }
}

public interface ICorsEnvironmentVariables
{
    /// <summary>Origens permitidas, separadas por vírgula.</summary>
    string CORS_ORIGENS { get; }
}

public interface IJwtEnvironmentVariables
{
    string JWT_CHAVE { get; }
    string JWT_EMISSOR { get; }
    string JWT_AUDIENCIA { get; }

    /// <summary>Validade do token da área administrativa, em horas.</summary>
    int JWT_EXPIRACAO_HORAS { get; }

    /// <summary>Tentativas de login aceitas por minuto, por IP.</summary>
    int LOGIN_LIMITE_POR_MINUTO { get; }
}

public interface IHealthCheckEnvironmentVariables
{
    int HEALTHCHECK_TIMEOUT_SEGUNDOS { get; }
}
