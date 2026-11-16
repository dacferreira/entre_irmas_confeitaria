namespace EntreIrmas.Domain.EnviromentVariables;

public interface IViaCepEnvironmentVariables
{
    /// <summary>Endereço base do serviço (ex.: https://viacep.com.br/ws/).</summary>
    string VIACEP_URL { get; }

    /// <summary>Tempo máximo de espera por resposta do ViaCEP.</summary>
    int VIACEP_TIMEOUT_SEGUNDOS { get; }

    /// <summary>Por quanto tempo o endereço de um CEP fica guardado em memória.</summary>
    int VIACEP_CACHE_HORAS { get; }

    /// <summary>Consultas de CEP aceitas por minuto, por IP (a rota é pública).</summary>
    int CEP_LIMITE_POR_MINUTO { get; }
}
