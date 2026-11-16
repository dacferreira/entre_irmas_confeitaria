namespace EntreIrmas.Domain.DTOs.Produto;

/// <summary>Foto pronta para ser devolvida como arquivo.</summary>
public record ImagemDto(string TipoConteudo, byte[] Conteudo);
