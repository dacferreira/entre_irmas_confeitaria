using EntreIrmas.Domain.DTOs.Loja;
using FluentValidation;

namespace EntreIrmas.Domain.Validations;

public class AlterarLojaValidation : BaseApiValidation<AlterarLojaDto, AlterarLojaValidation>
{
    public AlterarLojaValidation()
    {
        RuleFor(x => x.Mensagem)
            .MaximumLength(Entities.LojaAggregate.Loja.TamanhoMaximoDaMensagem)
            .WithMessage(TamanhoMaximo("Mensagem", Entities.LojaAggregate.Loja.TamanhoMaximoDaMensagem));
    }
}
