using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Comum;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EntreIrmas.Api.Controllers.Admin;

/// <summary>Cadastro de produtos (exige login).</summary>
[ApiController]
[Route("api/admin/produtos")]
public class AdminProdutosController : ControllerBase
{
    private readonly IProdutoAppService _produtoAppService;

    public AdminProdutosController(IProdutoAppService produtoAppService)
    {
        _produtoAppService = produtoAppService;
    }

    /// <summary>Lista os produtos ativos e inativos, com custo.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProdutoAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] FiltroProdutoAdminDto filtro, CancellationToken cancellationToken) =>
        Ok(await _produtoAppService.ListarAdminAsync(filtro, cancellationToken));

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ProdutoAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(long id, CancellationToken cancellationToken) =>
        Ok(await _produtoAppService.ObterAdminAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(ProdutoAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] SalvarProdutoDto dto, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await _produtoAppService.CriarAsync(dto, cancellationToken));

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ProdutoAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(long id, [FromBody] SalvarProdutoDto dto, CancellationToken cancellationToken) =>
        Ok(await _produtoAppService.AtualizarAsync(id, dto, cancellationToken));

    /// <summary>Mostra ou esconde o produto no site.</summary>
    [HttpPatch("{id:long}/situacao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarSituacao(long id, [FromBody] AlterarSituacaoDto dto, CancellationToken cancellationToken)
    {
        await _produtoAppService.AlterarSituacaoAsync(id, dto.Ativo, cancellationToken);
        return NoContent();
    }

    /// <summary>Exclusão lógica: os pedidos antigos continuam com o produto.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(long id, CancellationToken cancellationToken)
    {
        await _produtoAppService.ExcluirAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Envia (ou troca) a foto do produto: JPG, PNG ou WEBP de até 2 MB, no campo "arquivo".</summary>
    [HttpPut("{id:long}/imagem")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(TamanhoMaximoDaRequisicao)]
    [RequestFormLimits(MultipartBodyLengthLimit = TamanhoMaximoDaRequisicao)]
    [ProducesResponseType(typeof(ProdutoAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnviarImagem(long id, IFormFile? arquivo, CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
            throw new DomainException("Escolha uma foto.");
        if (arquivo.Length > ImagemProduto.TamanhoMaximo)
            throw new DomainException("A foto deve ter no máximo 2 MB.");

        using var memoria = new MemoryStream((int)arquivo.Length);
        await arquivo.CopyToAsync(memoria, cancellationToken);
        return Ok(await _produtoAppService.DefinirImagemAsync(id, memoria.ToArray(), cancellationToken));
    }

    /// <summary>Apaga a foto: o site volta a usar a ilustração do produto.</summary>
    [HttpDelete("{id:long}/imagem")]
    [ProducesResponseType(typeof(ProdutoAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverImagem(long id, CancellationToken cancellationToken) =>
        Ok(await _produtoAppService.RemoverImagemAsync(id, cancellationToken));

    // Foto de até 2 MB mais o envelope do multipart.
    private const long TamanhoMaximoDaRequisicao = ImagemProduto.TamanhoMaximo + 64 * 1024;
}
