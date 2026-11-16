using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Entities.LancamentoAggregate;

/// <summary>Despesa ou perda da confeitaria, descontada do lucro no painel.</summary>
public class Lancamento : Entity, IAggregateRoot
{
    protected Lancamento()
    {
    }

    public Lancamento(TipoLancamento tipo, string descricao, decimal valor, DateOnly data, string cadastradoPor)
    {
        Atualizar(tipo, descricao, valor, data);
        DefinirCadastro(cadastradoPor);
    }

    public TipoLancamento Tipo { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public DateOnly Data { get; private set; }

    public void Atualizar(TipoLancamento tipo, string descricao, decimal valor, DateOnly data)
    {
        if (!Enum.IsDefined(tipo))
            throw new DomainException("Tipo de lançamento inválido.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("A descrição do lançamento é obrigatória.");
        if (valor <= 0)
            throw new DomainException("O valor do lançamento deve ser maior que zero.");
        if (data == default)
            throw new DomainException("A data do lançamento é obrigatória.");

        Tipo = tipo;
        Descricao = descricao.Trim();
        Valor = valor;
        Data = data;
    }
}
