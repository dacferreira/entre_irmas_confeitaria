using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EntreIrmas.Api.Filters;

/// <summary>Corpo das respostas 400.</summary>
public class ValidationResult
{
    public string Mensagem { get; init; } = "Verifique os dados informados.";
    public IReadOnlyList<ErroDeCampo> Erros { get; init; } = Array.Empty<ErroDeCampo>();

    public static ValidationResult FromMessage(string mensagem) => new() { Mensagem = mensagem };

    public static ValidationResult FromModelState(ModelStateDictionary modelState) => new()
    {
        Erros = modelState
            .Where(m => m.Value?.Errors.Count > 0)
            .SelectMany(m => m.Value!.Errors.Select(e => new ErroDeCampo(
                NormalizarCampo(m.Key),
                string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Valor inválido." : e.ErrorMessage)))
            .ToList()
    };

    // "$.itens[0].quantidade" (erro de JSON) e "Itens[0].Quantidade" (FluentValidation) viram "itens[0].quantidade".
    private static string NormalizarCampo(string campo)
    {
        var limpo = campo.StartsWith("$.") ? campo[2..] : campo;
        return limpo.Length == 0 ? limpo : char.ToLowerInvariant(limpo[0]) + limpo[1..];
    }
}

public record ErroDeCampo(string Campo, string Mensagem);

/// <summary>Corpo das respostas de erro que não são de validação.</summary>
public record ErroResponse(string Error);
