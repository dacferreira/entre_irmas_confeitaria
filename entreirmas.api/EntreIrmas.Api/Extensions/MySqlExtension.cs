using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace EntreIrmas.Api.Extensions;

public static class MySqlExtension
{
    public static IServiceCollection AddMySql(this IServiceCollection services, IMySqlEnvironmentVariables variaveis)
    {
        var connectionString = MontarConnectionString(variaveis);

        // Versão informada (e não AutoDetect) para a API subir mesmo com o banco fora do ar.
        var versao = new MySqlServerVersion(Version.Parse(variaveis.MYSQL_VERSAO));

        services.AddDbContext<EntreIrmasDbContext>(options =>
            options.UseMySql(connectionString, versao, mySql =>
            {
                mySql.CommandTimeout((int)variaveis.MYSQL_TIMEOUT_SEGUNDOS);
                mySql.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        return services;
    }

    /// <summary>Monta a connection string pelo builder: senhas com ; = ' " quebrariam a concatenação.</summary>
    public static string MontarConnectionString(IMySqlEnvironmentVariables variaveis) =>
        new MySqlConnectionStringBuilder
        {
            Server = variaveis.MYSQL_SERVIDOR,
            Port = variaveis.MYSQL_PORTA,
            Database = variaveis.MYSQL_BANCO,
            UserID = variaveis.MYSQL_USUARIO,
            Password = variaveis.MYSQL_SENHA,
            CharacterSet = "utf8mb4",
            ConnectionTimeout = variaveis.MYSQL_TIMEOUT_SEGUNDOS
        }.ConnectionString;
}
