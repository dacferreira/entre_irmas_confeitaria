using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Categoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntreIrmas.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaAppService _categoriaAppService;

    public CategoriasController(ICategoriaAppService categoriaAppService)
    {
        _categoriaAppService = categoriaAppService;
    }

    /// <summary>Lista as categorias do cardápio que têm produtos disponíveis.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken) =>
        Ok(await _categoriaAppService.ListarAsync(cancellationToken));
}
