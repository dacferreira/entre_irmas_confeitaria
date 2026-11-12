namespace EntreIrmas.Domain.EnviromentVariables;

public interface IMySqlEnvironmentVariables
{
    string MYSQL_SERVIDOR { get; }
    uint MYSQL_PORTA { get; }
    string MYSQL_BANCO { get; }
    string MYSQL_USUARIO { get; }
    string MYSQL_SENHA { get; }

    /// <summary>Versão do servidor, informada para o Pomelo não precisar conectar na inicialização.</summary>
    string MYSQL_VERSAO { get; }

    uint MYSQL_TIMEOUT_SEGUNDOS { get; }
}
