namespace EntreIrmas.Domain.Seedwork;

/// <summary>Base das entidades: identificador, situação e dados de cadastro.</summary>
public abstract class Entity
{
    public long Id { get; protected set; }
    public EntityStatus Estado { get; protected set; } = EntityStatus.Ativo;
    public DateTime DataEHoraDeCadastro { get; protected set; } = DateTime.Now;
    public string CadastradoPor { get; protected set; } = string.Empty;

    public void Ativar() => Estado = EntityStatus.Ativo;
    public void Inativar() => Estado = EntityStatus.Inativo;
    public void Arquivar() => Estado = EntityStatus.Arquivado;

    /// <summary>
    /// Valor de um campo único (slug, login) após a exclusão lógica: o registro continua no banco e
    /// o valor original fica livre para outro cadastro.
    /// </summary>
    protected string ValorUnicoAposExclusao(string valor, int tamanhoMaximo)
    {
        var sufixo = $"~{Id}";
        return valor[..Math.Min(valor.Length, tamanhoMaximo - sufixo.Length)] + sufixo;
    }

    protected void DefinirCadastro(string cadastradoPor)
    {
        CadastradoPor = string.IsNullOrWhiteSpace(cadastradoPor) ? "SISTEMA" : cadastradoPor.Trim();
        DataEHoraDeCadastro = DateTime.Now;
    }
}
