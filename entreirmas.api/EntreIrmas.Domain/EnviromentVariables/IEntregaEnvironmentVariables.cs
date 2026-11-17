namespace EntreIrmas.Domain.EnviromentVariables;

/// <summary>Entrega por parceiros e o endereço de coleta (a loja).</summary>
public interface IEntregaEnvironmentVariables
{
    /// <summary>
    /// Liga o simulador de Uber e 99 (valores de mentira, sem chamar ninguém). Só para o ambiente local e os
    /// testes: com ele ligado, os clientes veriam taxas inventadas.
    /// </summary>
    bool ENTREGA_SIMULADOR { get; }

    /// <summary>Tempo máximo de espera por resposta de um parceiro.</summary>
    int ENTREGA_TIMEOUT_SEGUNDOS { get; }

    /// <summary>Cálculos de entrega aceitos por minuto, por IP (a rota é pública e cada cálculo consulta os parceiros).</summary>
    int ENTREGA_LIMITE_POR_MINUTO { get; }

    // Loja (onde o entregador busca o pedido)
    string LOJA_NOME { get; }
    string LOJA_LOGRADOURO { get; }
    string LOJA_BAIRRO { get; }
    string LOJA_CIDADE { get; }
    string LOJA_UF { get; }

    /// <summary>Só os 8 dígitos.</summary>
    string LOJA_CEP { get; }

    double LOJA_LATITUDE { get; }
    double LOJA_LONGITUDE { get; }
}
