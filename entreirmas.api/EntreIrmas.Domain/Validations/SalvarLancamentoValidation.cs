using EntreIrmas.Domain.DTOs.Lancamento;
using FluentValidation;

namespace EntreIrmas.Domain.Validations;

public class SalvarLancamentoValidation : BaseApiValidation<SalvarLancamentoDto, SalvarLancamentoValidation>
{
    public SalvarLancamentoValidation()
    {
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("Tipo de lançamento inválido.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage(Obrigatorio("Descrição"))
            .MaximumLength(200).WithMessage(TamanhoMaximo("Descrição", 200));

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("O valor deve ser maior que zero.")
            .LessThanOrEqualTo(SalvarProdutoValidation.ValorMaximo).WithMessage("Valor acima do permitido.")
            .PrecisionScale(10, 2, true).WithMessage("O valor aceita no máximo 2 casas decimais.");

        RuleFor(x => x.Data)
            .NotEqual(default(DateOnly)).WithMessage(Obrigatorio("Data"));
    }
}
