using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Entities.ProdutoAggregate;

/// <summary>
/// Foto de um produto, guardada no banco (uma por produto). Fica numa tabela própria para a listagem
/// do cardápio não carregar os bytes das fotos.
/// </summary>
public class ImagemProduto : Entity, IAggregateRoot
{
    /// <summary>O navegador reduz a foto antes de enviar; 2 MB sobra para uma foto de até 1200px.</summary>
    public const int TamanhoMaximo = 2 * 1024 * 1024;

    protected ImagemProduto()
    {
    }

    public ImagemProduto(long produtoId, byte[] conteudo, string cadastradoPor)
    {
        ProdutoId = produtoId;
        Substituir(conteudo);
        DefinirCadastro(cadastradoPor);
    }

    public long ProdutoId { get; private set; }

    /// <summary>image/jpeg, image/png ou image/webp, identificado pelos bytes do arquivo.</summary>
    public string TipoConteudo { get; private set; } = string.Empty;

    public byte[] Conteudo { get; private set; } = [];

    public void Substituir(byte[] conteudo)
    {
        if (conteudo is null || conteudo.Length == 0)
            throw new DomainException("Escolha uma foto.");
        if (conteudo.Length > TamanhoMaximo)
            throw new DomainException("A foto deve ter no máximo 2 MB.");

        TipoConteudo = IdentificarTipo(conteudo)
            ?? throw new DomainException("Envie uma foto em JPG, PNG ou WEBP.");
        Conteudo = conteudo;
    }

    /// <summary>
    /// Confere a assinatura do arquivo em vez de confiar no nome ou no tipo informado pelo navegador:
    /// um arquivo qualquer renomeado para .jpg é recusado.
    /// </summary>
    public static string? IdentificarTipo(ReadOnlySpan<byte> conteudo)
    {
        if (conteudo.Length >= 3 && conteudo[0] == 0xFF && conteudo[1] == 0xD8 && conteudo[2] == 0xFF)
            return "image/jpeg";

        if (conteudo.Length >= 8 && conteudo[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return "image/png";

        if (conteudo.Length >= 12 && conteudo[..4].SequenceEqual("RIFF"u8) && conteudo[8..12].SequenceEqual("WEBP"u8))
            return "image/webp";

        return null;
    }
}
