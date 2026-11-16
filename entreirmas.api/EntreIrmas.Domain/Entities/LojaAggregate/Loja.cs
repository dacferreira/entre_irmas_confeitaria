using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Entities.LojaAggregate;

/// <summary>
/// Situação da loja no site (registro único). Fechada, o site só aceita encomendas; a mensagem
/// aparece no aviso de fechado (ex.: "Voltamos segunda às 9h").
/// </summary>
public class Loja : Entity, IAggregateRoot
{
    /// <summary>Id do registro único, criado pelo script do banco.</summary>
    public const long IdUnico = 1;
    public const int TamanhoMaximoDaMensagem = 200;

    protected Loja()
    {
    }

    public Loja(string cadastradoPor)
    {
        Id = IdUnico;
        Aberta = true;
        DefinirCadastro(cadastradoPor);
        AlteradoPor = CadastradoPor;
        DataEHoraDaAlteracao = DataEHoraDeCadastro;
    }

    public bool Aberta { get; private set; }

    /// <summary>Mensagem do aviso de fechado (vale só com a loja fechada).</summary>
    public string? MensagemFechada { get; private set; }

    public string AlteradoPor { get; private set; } = string.Empty;
    public DateTime DataEHoraDaAlteracao { get; private set; }

    public void Abrir(string usuario)
    {
        Aberta = true;
        MensagemFechada = null;
        RegistrarAlteracao(usuario);
    }

    public void Fechar(string? mensagem, string usuario)
    {
        var texto = string.IsNullOrWhiteSpace(mensagem) ? null : mensagem.Trim();
        if (texto?.Length > TamanhoMaximoDaMensagem)
            throw new DomainException($"A mensagem pode ter até {TamanhoMaximoDaMensagem} caracteres.");

        Aberta = false;
        MensagemFechada = texto;
        RegistrarAlteracao(usuario);
    }

    private void RegistrarAlteracao(string usuario)
    {
        AlteradoPor = string.IsNullOrWhiteSpace(usuario) ? "SISTEMA" : usuario.Trim();
        DataEHoraDaAlteracao = DateTime.Now;
    }
}
