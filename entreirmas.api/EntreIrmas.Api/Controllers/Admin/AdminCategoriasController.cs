using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Categoria;
using EntreIrmas.Domain.DTOs.Comum;
using Microsoft.AspNetCore.Mvc;

namespace EntreIrmas.Api.Controllers.Admin;

/// <summary>Cadastro de categorias (exige login).</summary>
[ApiController]
[Route("api/admin/categorias")]
public class AdminCategoriasController : ControllerBase
{
    private readonly ICategoriaAppService _categoriaAppService;

    public AdminCategoriasController(ICategoriaAppService categoriaAppService)
    {
        _categoriaAppService = categoriaAppService;
    }

    /// <summary>Lista as categorias ativas e inativas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken) =>
        Ok(await _categoriaAppService.ListarAdminAsync(cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(CategoriaAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] SalvarCategoriaDto dto, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await _categoriaAppService.CriarAsync(dto, cancellationToken));

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(CategoriaAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(long id, [FromBody] SalvarCategoriaDto dto, CancellationToken cancellationToken) =>
        Ok(await _categoriaAppService.AtualizarAsync(id, dto, cancellationToken));

    /// <summary>Mostra ou esconde a categoria (e os produtos dela) no site.</summary>
    [HttpPatch("{id:long}/situacao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarSituacao(long id, [FromBody] AlterarSituacaoDto dto, CancellationToken cancellationToken)
    {
        await _categoriaAppService.AlterarSituacaoAsync(id, dto.Ativo, cancellationToken);
        return NoContent();
    }

    /// <summary>Exclusão lógica. Só para categorias sem produtos.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(long id, CancellationToken cancellationToken)
    {
        await _categoriaAppService.ExcluirAsync(id, cancellationToken);
        return NoContent();
    }
}
