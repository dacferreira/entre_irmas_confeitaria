using EntreIrmas.Domain.DTOs.Autenticacao;
using FluentValidation;

namespace EntreIrmas.Domain.Validations;

public class LoginValidation : BaseApiValidation<LoginDto, LoginValidation>
{
    public LoginValidation()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage(Obrigatorio("Usuário"))
            .MaximumLength(30).WithMessage(TamanhoMaximo("Usuário", 30));

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage(Obrigatorio("Senha"))
            .MaximumLength(SalvarUsuarioValidation.TamanhoMaximoDaSenha)
            .WithMessage(TamanhoMaximo("Senha", SalvarUsuarioValidation.TamanhoMaximoDaSenha));
    }
}
