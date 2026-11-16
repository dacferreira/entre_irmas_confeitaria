using EntreIrmas.Domain.DTOs.Produto;
using FluentValidation;

namespace EntreIrmas.Domain.Validations;

public class SalvarProdutoValidation : BaseApiValidation<SalvarProdutoDto, SalvarProdutoValidation>
{
    public const decimal ValorMaximo = 99_999_999.99m;

    public SalvarProdutoValidation()
    {
        RuleFor(x => x.CategoriaId).GreaterThan(0).WithMessage(Obrigatorio("Categoria"));

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage(Obrigatorio("Nome"))
            .MaximumLength(100).WithMessage(TamanhoMaximo("Nome", 100));

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage(Obrigatorio("Descrição"))
            .MaximumLength(1000).WithMessage(TamanhoMaximo("Descrição", 1000));

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.")
            .LessThanOrEqualTo(ValorMaximo).WithMessage("Preço acima do permitido.")
            .PrecisionScale(10, 2, true).WithMessage("O preço aceita no máximo 2 casas decimais.");

        RuleFor(x => x.Custo)
            .GreaterThanOrEqualTo(0).WithMessage("O custo não pode ser negativo.")
            .LessThanOrEqualTo(ValorMaximo).WithMessage("Custo acima do permitido.")
            .PrecisionScale(10, 2, true).WithMessage("O custo aceita no máximo 2 casas decimais.");

        RuleFor(x => x.Unidade)
            .NotEmpty().WithMessage(Obrigatorio("Unidade de venda"))
            .MaximumLength(40).WithMessage(TamanhoMaximo("Unidade de venda", 40));

        RuleFor(x => x.QuantidadeMinima)
            .InclusiveBetween(1, CriarPedidoValidation.QuantidadeMaximaPorItem)
            .WithMessage($"A quantidade mínima deve estar entre 1 e {CriarPedidoValidation.QuantidadeMaximaPorItem}.");

        RuleFor(x => x.Ilustracao)
            .NotEmpty().WithMessage(Obrigatorio("Ilustração"))
            .MaximumLength(30).WithMessage(TamanhoMaximo("Ilustração", 30));
    }
}
