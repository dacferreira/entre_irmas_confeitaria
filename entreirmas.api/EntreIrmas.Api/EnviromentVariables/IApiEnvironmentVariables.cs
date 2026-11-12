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
}

public interface IHealthCheckEnvironmentVariables
{
    int HEALTHCHECK_TIMEOUT_SEGUNDOS { get; }
}
