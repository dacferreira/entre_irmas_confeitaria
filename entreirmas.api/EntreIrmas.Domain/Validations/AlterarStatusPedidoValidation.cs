using EntreIrmas.Domain.DTOs.Pedido;
using FluentValidation;

namespace EntreIrmas.Domain.Validations;

public class AlterarStatusPedidoValidation : BaseApiValidation<AlterarStatusPedidoDto, AlterarStatusPedidoValidation>
{
    public AlterarStatusPedidoValidation()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("Status do pedido inválido.");
    }
}
