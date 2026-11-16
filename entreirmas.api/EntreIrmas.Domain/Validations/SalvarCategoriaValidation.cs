using EntreIrmas.Domain.DTOs.Categoria;
using FluentValidation;

namespace EntreIrmas.Domain.Validations;

public class SalvarCategoriaValidation : BaseApiValidation<SalvarCategoriaDto, SalvarCategoriaValidation>
{
    public SalvarCategoriaValidation()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage(Obrigatorio("Nome"))
            .MaximumLength(100).WithMessage(TamanhoMaximo("Nome", 100));

        RuleFor(x => x.Descricao)
            .MaximumLength(300).WithMessage(TamanhoMaximo("Descrição", 300));

        RuleFor(x => x.Ordem)
            .InclusiveBetween(0, 999).WithMessage("A ordem deve estar entre 0 e 999.");
    }
}
