namespace EntreIrmas.Domain.DTOs.Endereco;

/// <summary>Endereço encontrado pelo CEP, para preencher o formulário.</summary>
public class EnderecoCepDto
{
    /// <summary>Só os 8 dígitos.</summary>
    public string Cep { get; set; } = string.Empty;

    public string Logradouro { get; set; } = string.Empty;

    /// <summary>Informação do trecho do CEP (ex.: "lado ímpar"); pode vir vazia.</summary>
    public string Complemento { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
}

/// <summary>Endereço de entrega informado no pedido, em partes.</summary>
public class EnderecoDto
{
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
}
