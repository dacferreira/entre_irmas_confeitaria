using EntreIrmas.Domain.DTOs.Endereco;
using FluentValidation;

namespace EntreIrmas.Domain.Validations;

/// <summary>Endereço de entrega do pedido (os erros saem como endereco.cep, endereco.numero...).</summary>
public class EnderecoValidation : BaseApiValidation<EnderecoDto, EnderecoValidation>
{
    private static readonly HashSet<string> Ufs =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
        "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    public EnderecoValidation()
    {
        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage(Obrigatorio("CEP"))
            .Must(cep => CriarPedidoValidation.SomenteDigitos(cep).Length == 8)
            .WithMessage("Informe um CEP com 8 números.");

        RuleFor(x => x.Logradouro)
            .NotEmpty().WithMessage(Obrigatorio("Rua"))
            .MaximumLength(150).WithMessage(TamanhoMaximo("Rua", 150));

        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage(Obrigatorio("Número"))
            .MaximumLength(20).WithMessage(TamanhoMaximo("Número", 20));

        RuleFor(x => x.Complemento)
            .MaximumLength(100).WithMessage(TamanhoMaximo("Complemento", 100));

        RuleFor(x => x.Bairro)
            .NotEmpty().WithMessage(Obrigatorio("Bairro"))
            .MaximumLength(100).WithMessage(TamanhoMaximo("Bairro", 100));

        RuleFor(x => x.Cidade)
            .NotEmpty().WithMessage(Obrigatorio("Cidade"))
            .MaximumLength(100).WithMessage(TamanhoMaximo("Cidade", 100));

        RuleFor(x => x.Uf)
            .Must(uf => Ufs.Contains((uf ?? string.Empty).Trim().ToUpperInvariant()))
            .WithMessage("Informe uma UF válida (ex.: RJ).");
    }
}
