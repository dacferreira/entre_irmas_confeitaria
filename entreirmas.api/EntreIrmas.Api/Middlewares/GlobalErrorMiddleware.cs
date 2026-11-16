using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.Exceptions;

namespace EntreIrmas.Api.Middlewares;

/// <summary>Converte as exceções em respostas HTTP padronizadas.</summary>
public class GlobalErrorMiddleware
{
    private const string MensagemGenerica = "Não foi possível concluir a operação. Tente novamente em instantes.";

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalErrorMiddleware> _logger;

    public GlobalErrorMiddleware(RequestDelegate next, ILogger<GlobalErrorMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // O cliente desistiu da requisição; não há a quem responder.
        }
        catch (Exception excecao)
        {
            await TratarAsync(context, excecao);
        }
    }

    private async Task TratarAsync(HttpContext context, Exception excecao)
    {
        if (context.Response.HasStarted)
            throw excecao;

        var (status, corpo) = excecao switch
        {
            DomainException or ArgumentException or InvalidOperationException =>
                (StatusCodes.Status400BadRequest, (object)ValidationResult.FromMessage(excecao.Message)),
            ObjectNotFoundException => (StatusCodes.Status404NotFound, new ErroResponse(excecao.Message)),
            ApiException api => (api.StatusCode, new ErroResponse(excecao.Message)),
            GatewayException => (StatusCodes.Status503ServiceUnavailable,
                new ErroResponse("Um serviço externo está indisponível no momento. Tente novamente em instantes.")),
            _ => (StatusCodes.Status500InternalServerError, new ErroResponse(MensagemGenerica))
        };

        if (status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(excecao, "Erro não tratado em {Metodo} {Caminho}", context.Request.Method, context.Request.Path);
        else
            _logger.LogInformation("Requisição recusada ({Status}): {Mensagem}", status, excecao.Message);

        context.Response.Clear();
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(corpo, corpo.GetType());
    }
}
