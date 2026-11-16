using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using FluentValidation;

namespace EntreIrmas.Domain.Validations;

public class CriarPedidoValidation : BaseApiValidation<CriarPedidoDto, CriarPedidoValidation>
{
    public const int MaximoDeItens = 50;
    public const int QuantidadeMaximaPorItem = 1000;

    public CriarPedidoValidation()
    {
        RuleFor(x => x.NomeCliente)
            .NotEmpty().WithMessage(Obrigatorio("Nome"))
            .MaximumLength(100).WithMessage(TamanhoMaximo("Nome", 100));

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage(Obrigatorio("Telefone"))
            .Must(t => SomenteDigitos(t).Length is 10 or 11)
            .WithMessage("Informe o telefone com DDD (10 ou 11 dígitos).");

        RuleFor(x => x.TipoEntrega).IsInEnum().WithMessage("Forma de entrega inválida.");

        RuleFor(x => x.Endereco)
            .NotNull().WithMessage(Obrigatorio("Endereço"))
            .SetValidator(new EnderecoValidation()!)
            .When(x => x.TipoEntrega == TipoEntrega.Entrega);

        RuleFor(x => x.DataDesejada)
            .NotEqual(default(DateOnly)).WithMessage(Obrigatorio("Data desejada"));

        RuleFor(x => x.Observacao)
            .MaximumLength(500).WithMessage(TamanhoMaximo("Observação", 500));

        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("Adicione pelo menos um doce ao pedido.")
            .Must(i => i.Count <= MaximoDeItens).WithMessage($"O pedido pode ter no máximo {MaximoDeItens} itens.");

        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.ProdutoId).GreaterThan(0).WithMessage("Produto inválido.");
            item.RuleFor(i => i.Quantidade)
                .InclusiveBetween(1, QuantidadeMaximaPorItem)
                .WithMessage($"A quantidade deve estar entre 1 e {QuantidadeMaximaPorItem}.");
        });
    }

    public static string SomenteDigitos(string? valor) =>
        new((valor ?? string.Empty).Where(char.IsDigit).ToArray());
}
