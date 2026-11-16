using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Produto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntreIrmas.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoAppService _produtoAppService;

    public ProdutosController(IProdutoAppService produtoAppService)
    {
        _produtoAppService = produtoAppService;
    }

    /// <summary>Lista o cardápio, com filtro opcional por categoria, busca e destaques.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProdutoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] FiltroProdutoDto filtro, CancellationToken cancellationToken) =>
        Ok(await _produtoAppService.ListarAsync(filtro, cancellationToken));

    /// <summary>Detalhe de um produto pelo slug (ex.: brownie-de-nutella).</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorSlug(string slug, CancellationToken cancellationToken) =>
        Ok(await _produtoAppService.ObterPorSlugAsync(slug, cancellationToken));

    /// <summary>
    /// Foto do produto. A URL tem a versão (?v=), que muda a cada envio, então a resposta pode ficar
    /// em cache por um ano no navegador e no proxy.
    /// </summary>
    [HttpGet("{id:long}/imagem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterImagem(long id, CancellationToken cancellationToken)
    {
        var imagem = await _produtoAppService.ObterImagemAsync(id, cancellationToken);

        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(imagem.Conteudo, imagem.TipoConteudo);
    }
}
