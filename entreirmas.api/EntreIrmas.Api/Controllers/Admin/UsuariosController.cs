using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Comum;
using EntreIrmas.Domain.DTOs.Usuario;
using Microsoft.AspNetCore.Mvc;

namespace EntreIrmas.Api.Controllers.Admin;

/// <summary>Usuários da área administrativa (exige login).</summary>
[ApiController]
[Route("api/admin/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioAppService _usuarioAppService;

    public UsuariosController(IUsuarioAppService usuarioAppService)
    {
        _usuarioAppService = usuarioAppService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken) =>
        Ok(await _usuarioAppService.ListarAsync(cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] SalvarUsuarioDto dto, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await _usuarioAppService.CriarAsync(dto, cancellationToken));

    /// <summary>Altera o nome e, se informada, a senha. O login não muda.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(long id, [FromBody] SalvarUsuarioDto dto, CancellationToken cancellationToken) =>
        Ok(await _usuarioAppService.AtualizarAsync(id, dto, cancellationToken));

    [HttpPatch("{id:long}/situacao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarSituacao(long id, [FromBody] AlterarSituacaoDto dto, CancellationToken cancellationToken)
    {
        await _usuarioAppService.AlterarSituacaoAsync(id, dto.Ativo, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(long id, CancellationToken cancellationToken)
    {
        await _usuarioAppService.ExcluirAsync(id, cancellationToken);
        return NoContent();
    }
}
