using EntreIrmas.Application.Profiles;
using EntreIrmas.Application.Services;
using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Entities.LancamentoAggregate;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Entities.UsuarioAggregate;
using EntreIrmas.Domain.Seedwork;
using EntreIrmas.Infrastructure.Persistence;
using Mapster;
using MapsterMapper;

namespace EntreIrmas.Test.Config;

/// <summary>Fábrica de entidades e do mapper usados nos testes.</summary>
internal static class DadosDeTeste
{
    public const string LoginDeTeste = "admin.teste";
    public const string SenhaDeTeste = "senha-de-teste-123";

    /// <summary>Poucas iterações: o formato é o mesmo, mas a suíte não fica lenta.</summary>
    public static SenhaService CriarSenhaService() => new(1000);

    public static IMapper CriarMapper()
    {
        var config = new TypeAdapterConfig();
        config.Scan(typeof(CatalogoProfile).Assembly);
        return new Mapper(config);
    }

    public static Produto Produto(long id = 1, decimal preco = 12m, int quantidadeMinima = 1, string nome = "Brownie",
        string unidade = "unidade", decimal custo = 4m, bool encomenda = false)
    {
        var produto = new Produto(1, nome, nome.ToLowerInvariant().Replace(' ', '-'), "Descrição", preco, custo, unidade,
            quantidadeMinima, "brownie", false, encomenda, "TESTE");
        DefinirId(produto, id);
        return produto;
    }

    public static EnderecoEntrega Endereco(string? complemento = "Apto 2") =>
        new("20000-000", "Rua das Flores", "10", complemento, "Centro", "Rio de Janeiro", "rj");

    public static Pedido Pedido(TipoEntrega tipoEntrega = TipoEntrega.Retirada, EnderecoEntrega? endereco = null,
        string? observacao = null, DateOnly? data = null, string codigo = "EI-ABC234") =>
        new(codigo, "Maria", "21999998888", tipoEntrega, endereco, data ?? new DateOnly(2026, 12, 20), observacao, "TESTE");

    public static Usuario Usuario(long id = 1, string login = LoginDeTeste, string nome = "Pessoa de Teste")
    {
        var usuario = new Usuario(login, nome, CriarSenhaService().GerarHash(SenhaDeTeste), "TESTE");
        DefinirId(usuario, id);
        return usuario;
    }

    public static Lancamento Lancamento(TipoLancamento tipo, decimal valor, DateOnly data, string descricao = "Chocolate") =>
        new(tipo, descricao, valor, data, "TESTE");

    // Só a assinatura de cada formato (o domínio confere os primeiros bytes; não decodifica a imagem).
    public static byte[] ImagemJpeg() => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];
    public static byte[] ImagemPng() => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];
    public static byte[] ImagemWebp() => [.."RIFF"u8, 0x24, 0x00, 0x00, 0x00, .."WEBPVP8 "u8];

    public static void DefinirId(Entity entidade, long id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entidade, id);

    /// <summary>Cardápio usado nos testes de repositório e de integração.</summary>
    public static void PopularCardapio(EntreIrmasDbContext contexto)
    {
        var bolos = new Categoria("Bolos", "bolos", "Bolos caseiros", 1, "TESTE");
        var docinhos = new Categoria("Docinhos", "docinhos", "Para festas", 2, "TESTE");
        var semProdutos = new Categoria("Tortas", "tortas", null, 3, "TESTE");
        contexto.Categorias.AddRange(bolos, docinhos, semProdutos);
        contexto.SaveChanges();

        var inativo = new Produto(bolos.Id, "Bolo Antigo", "bolo-antigo", "Fora do cardápio", 50m, 20m, "unidade", 1,
            "bolo", false, false, "TESTE");
        inativo.Inativar();

        contexto.Produtos.AddRange(
            new Produto(bolos.Id, "Bolo de Chocolate com Morango", "bolo-de-chocolate-com-morango",
                "Massa de chocolate, brigadeiro e morangos", 95m, 40m, "unidade (1,5 kg)", 1, "bolo", true, false, "TESTE"),
            new Produto(bolos.Id, "Bolo de Pote de Ninho", "bolo-de-pote-de-ninho",
                "Camadas de bolo e creme de leite Ninho", 15m, 6m, "pote 250 ml", 1, "bolo-pote", false, false, "TESTE"),
            new Produto(docinhos.Id, "Brigadeiro Gourmet", "brigadeiro-gourmet",
                "Chocolate belga", 2.5m, 1m, "unidade", 25, "brigadeiro", true, false, "TESTE"),
            inativo);
        contexto.SaveChanges();
    }

    /// <summary>Usuário ativo para os testes de login, mais um inativo que não pode entrar.</summary>
    public static void PopularUsuarios(EntreIrmasDbContext contexto)
    {
        var senhas = CriarSenhaService();
        var inativo = new Usuario("inativo", "Usuário Inativo", senhas.GerarHash(SenhaDeTeste), "TESTE");
        inativo.Inativar();

        contexto.Usuarios.AddRange(
            new Usuario(LoginDeTeste, "Pessoa de Teste", senhas.GerarHash(SenhaDeTeste), "TESTE"),
            new Usuario("outra.pessoa", "Outra Pessoa", senhas.GerarHash(SenhaDeTeste), "TESTE"),
            inativo);
        contexto.SaveChanges();
    }
}
