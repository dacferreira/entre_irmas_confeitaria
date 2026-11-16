namespace EntreIrmas.Domain.DTOs.Loja;

/// <summary>Situação pública da loja, para o aviso do site.</summary>
public class SituacaoLojaDto
{
    public bool Aberta { get; set; }
    public string? Mensagem { get; set; }
}

/// <summary>Situação com quem alterou e quando (área administrativa).</summary>
public class LojaAdminDto : SituacaoLojaDto
{
    public string AlteradoPor { get; set; } = string.Empty;
    public DateTime DataDaAlteracao { get; set; }
}

public class AlterarLojaDto
{
    public bool Aberta { get; set; }

    /// <summary>Aviso exibido com a loja fechada (opcional).</summary>
    public string? Mensagem { get; set; }
}
