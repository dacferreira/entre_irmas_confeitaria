using EntreIrmas.Api.Extensions;
using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Endereco;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EntreIrmas.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/enderecos")]
public class EnderecosController : ControllerBase
{
    private readonly IEnderecoAppService _enderecoAppService;

    public EnderecosController(IEnderecoAppService enderecoAppService)
    {
        _enderecoAppService = enderecoAppService;
    }

    /// <summary>
    /// Endereço do CEP (consultado no ViaCEP), para preencher o checkout. O CEP pode vir com ou sem máscara.
    /// </summary>
    [HttpGet("cep/{cep}")]
    [EnableRateLimiting(RateLimitExtension.PoliticaCep)]
    [ProducesResponseType(typeof(EnderecoCepDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ConsultarCep(string cep, CancellationToken cancellationToken) =>
        Ok(await _enderecoAppService.ConsultarCepAsync(cep, cancellationToken));
}
