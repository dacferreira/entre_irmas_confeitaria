namespace EntreIrmas.Domain.EnviromentVariables;

/// <summary>
/// Uber Direct (entregas da Uber para empresas). As credenciais vêm do painel do Uber Direct
/// (direct.uber.com → Developer): são segredos, ficam em user-secrets ou variável de ambiente, nunca no appsettings.
/// Sem UBER_CLIENT_ID, UBER_CLIENT_SECRET e UBER_CUSTOMER_ID a Uber fica desligada.
/// </summary>
public interface IUberEnvironmentVariables
{
    string UBER_CLIENT_ID { get; }
    string UBER_CLIENT_SECRET { get; }
    string UBER_CUSTOMER_ID { get; }

    /// <summary>Emissão do token (OAuth client credentials).</summary>
    string UBER_AUTH_URL { get; }

    /// <summary>Base da API (ex.: https://api.uber.com/v1/).</summary>
    string UBER_API_URL { get; }

    /// <summary>
    /// Modo de teste do Uber Direct: as entregas criadas usam o entregador-robô (nenhum entregador de verdade sai).
    /// </summary>
    bool UBER_MODO_TESTE { get; }
}
