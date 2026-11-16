using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Loja;
using Microsoft.AspNetCore.Mvc;

namespace EntreIrmas.Api.Controllers.Admin;

/// <summary>Abre e fecha a loja no site (exige login).</summary>
[ApiController]
[Route("api/admin/loja")]
public class AdminLojaController : ControllerBase
{
    private readonly ILojaAppService _lojaAppService;

    public AdminLojaController(ILojaAppService lojaAppService)
    {
        _lojaAppService = lojaAppService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(LojaAdminDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obter(CancellationToken cancellationToken) =>
        Ok(await _lojaAppService.ObterAsync(cancellationToken));

    [HttpPut]
    [ProducesResponseType(typeof(LojaAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Alterar([FromBody] AlterarLojaDto dto, CancellationToken cancellationToken) =>
        Ok(await _lojaAppService.AlterarAsync(dto, cancellationToken));
}
