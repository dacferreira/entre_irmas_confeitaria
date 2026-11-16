using EntreIrmas.Domain.DTOs.Usuario;
using FluentValidation;

namespace EntreIrmas.Domain.Validations;

/// <summary>A obrigatoriedade da senha na criação é conferida pelo AppService (na edição ela é opcional).</summary>
public class SalvarUsuarioValidation : BaseApiValidation<SalvarUsuarioDto, SalvarUsuarioValidation>
{
    public const int TamanhoMinimoDaSenha = 8;
    public const int TamanhoMaximoDaSenha = 128;

    public SalvarUsuarioValidation()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage(Obrigatorio("Login"))
            .Matches("^[a-zA-Z0-9._-]{3,30}$")
            .WithMessage("O login deve ter de 3 a 30 letras, números, ponto, hífen ou sublinhado, sem espaços.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage(Obrigatorio("Nome"))
            .MaximumLength(100).WithMessage(TamanhoMaximo("Nome", 100));

        RuleFor(x => x.Senha)
            .MinimumLength(TamanhoMinimoDaSenha)
            .WithMessage($"A senha deve ter pelo menos {TamanhoMinimoDaSenha} caracteres.")
            .MaximumLength(TamanhoMaximoDaSenha).WithMessage(TamanhoMaximo("Senha", TamanhoMaximoDaSenha))
            .When(x => !string.IsNullOrEmpty(x.Senha));
    }
}
