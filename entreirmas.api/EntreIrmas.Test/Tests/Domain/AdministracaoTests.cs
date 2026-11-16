using EntreIrmas.Domain.DTOs.Lancamento;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.DTOs.Usuario;
using EntreIrmas.Domain.Entities.LancamentoAggregate;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Entities.UsuarioAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Seedwork;
using EntreIrmas.Domain.Validations;
using EntreIrmas.Test.Config;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Domain;

public class AdministracaoTests
{
    [Fact]
    public void Usuario_Criar_NormalizaLoginENome()
    {
        var usuario = new Usuario("  Suellen ", "  Suellen Alves ", "hash", "T");

        usuario.Login.Should().Be("suellen");
        usuario.Nome.Should().Be("Suellen Alves");
        usuario.UltimoAcesso.Should().BeNull();
    }

    [Theory]
    [InlineData("ab", "Nome", "hash")]
    [InlineData("com espaco", "Nome", "hash")]
    [InlineData("suellen", " ", "hash")]
    [InlineData("suellen", "Nome", "")]
    public void Usuario_ComDadosInvalidos_LancaDomainException(string login, string nome, string hash)
    {
        var criar = () => new Usuario(login, nome, hash, "T");

        criar.Should().Throw<DomainException>();
    }

    [Fact]
    public void Usuario_ExcluirERegistrarAcesso()
    {
        var usuario = DadosDeTeste.Usuario(id: 12, login: "tatiane");
        var agora = new DateTime(2026, 11, 13, 9, 30, 0);

        usuario.RegistrarAcesso(agora);
        usuario.Excluir();

        usuario.UltimoAcesso.Should().Be(agora);
        usuario.Estado.Should().Be(EntityStatus.Arquivado);
        usuario.Login.Should().Be("tatiane~12");
    }

    [Fact]
    public void Lancamento_CriarEAtualizar()
    {
        var lancamento = new Lancamento(TipoLancamento.Despesa, " Chocolate ", 80m, new DateOnly(2026, 11, 1), "T");

        lancamento.Atualizar(TipoLancamento.Perda, "Bolo descartado", 45.5m, new DateOnly(2026, 11, 2));

        lancamento.Tipo.Should().Be(TipoLancamento.Perda);
        lancamento.Descricao.Should().Be("Bolo descartado");
        lancamento.Valor.Should().Be(45.5m);
    }

    [Theory]
    [InlineData((TipoLancamento)9, "x", 10, 2026)]
    [InlineData(TipoLancamento.Despesa, " ", 10, 2026)]
    [InlineData(TipoLancamento.Despesa, "x", 0, 2026)]
    [InlineData(TipoLancamento.Despesa, "x", 10, 1)]
    public void Lancamento_ComDadosInvalidos_LancaDomainException(TipoLancamento tipo, string descricao, decimal valor, int ano)
    {
        var data = ano == 1 ? default : new DateOnly(ano, 1, 1);

        var criar = () => new Lancamento(tipo, descricao, valor, data, "T");

        criar.Should().Throw<DomainException>();
    }

    [Fact]
    public void Pedido_CopiaOCustoECalculaOCustoTotal()
    {
        var pedido = DadosDeTeste.Pedido();
        pedido.AdicionarItem(DadosDeTeste.Produto(1, preco: 12m, custo: 4m), 3);
        pedido.AdicionarItem(DadosDeTeste.Produto(2, preco: 10m, custo: 2.5m, nome: "Fudge"), 2);

        pedido.ValorTotal.Should().Be(56m);
        pedido.CustoTotal.Should().Be(17m);
        pedido.Itens.First().CustoUnitario.Should().Be(4m);
    }

    [Theory]
    [InlineData(StatusPedido.Recebido, false)]
    [InlineData(StatusPedido.Confirmado, true)]
    [InlineData(StatusPedido.EmPreparo, true)]
    [InlineData(StatusPedido.Pronto, true)]
    [InlineData(StatusPedido.Entregue, true)]
    [InlineData(StatusPedido.Cancelado, false)]
    public void Pedido_ContaComoVenda_SoConfirmadosENaoCancelados(StatusPedido status, bool esperado)
    {
        var pedido = DadosDeTeste.Pedido();

        pedido.AlterarStatus(status);

        pedido.ContaComoVenda.Should().Be(esperado);
    }

    [Fact]
    public void Pedido_AlterarStatusInvalido_LancaDomainException()
    {
        var alterar = () => DadosDeTeste.Pedido().AlterarStatus((StatusPedido)42);

        alterar.Should().Throw<DomainException>();
    }

    [Fact]
    public void SalvarUsuarioValidation_SenhaCurtaELoginComEspaco_TemErros()
    {
        var resultado = new SalvarUsuarioValidation().Validate(new SalvarUsuarioDto
        {
            Login = "com espaco",
            Nome = "",
            Senha = "123"
        });

        resultado.Errors.Select(e => e.PropertyName).Should().BeEquivalentTo("Login", "Nome", "Senha");
    }

    [Fact]
    public void SalvarUsuarioValidation_SemSenha_EValido()
    {
        new SalvarUsuarioValidation().Validate(new SalvarUsuarioDto { Login = "suellen", Nome = "Suellen" })
            .IsValid.Should().BeTrue();
    }

    [Fact]
    public void SalvarProdutoValidation_ExigeDuasCasasDecimaisECustoPositivo()
    {
        var resultado = new SalvarProdutoValidation().Validate(new SalvarProdutoDto
        {
            CategoriaId = 1,
            Nome = "Brownie",
            Descricao = "Macio",
            Preco = 10.555m,
            Custo = -1m,
            Unidade = "unidade",
            QuantidadeMinima = 1,
            Ilustracao = "brownie"
        });

        resultado.Errors.Select(e => e.PropertyName).Should().BeEquivalentTo("Preco", "Custo");
    }

    [Fact]
    public void SalvarLancamentoValidation_ValorEDataObrigatorios()
    {
        var resultado = new SalvarLancamentoValidation().Validate(new SalvarLancamentoDto
        {
            Tipo = TipoLancamento.Despesa,
            Descricao = "Gás",
            Valor = 0
        });

        resultado.Errors.Select(e => e.PropertyName).Should().BeEquivalentTo("Valor", "Data");
    }
}
