using EntreIrmas.Application.Profiles;
using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Seedwork;
using EntreIrmas.Infrastructure.Persistence;
using Mapster;
using MapsterMapper;

namespace EntreIrmas.Test.Config;

/// <summary>Fábrica de entidades e do mapper usados nos testes.</summary>
internal static class DadosDeTeste
{
    public static IMapper CriarMapper()
    {
        var config = new TypeAdapterConfig();
        config.Scan(typeof(CatalogoProfile).Assembly);
        return new Mapper(config);
    }

    public static Produto Produto(long id = 1, decimal preco = 12m, int quantidadeMinima = 1, string nome = "Brownie",
        string unidade = "unidade")
    {
        var produto = new Produto(1, nome, nome.ToLowerInvariant().Replace(' ', '-'), "Descrição", preco, unidade,
            quantidadeMinima, "brownie", null, false, "TESTE");
        DefinirId(produto, id);
        return produto;
    }

    public static Pedido Pedido(TipoEntrega tipoEntrega = TipoEntrega.Retirada, string? endereco = null,
        string? observacao = null) =>
        new("EI-ABC234", "Maria", "21999998888", tipoEntrega, endereco, new DateOnly(2026, 12, 20), observacao, "TESTE");

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

        var inativo = new Produto(bolos.Id, "Bolo Antigo", "bolo-antigo", "Fora do cardápio", 50m, "unidade", 1,
            "bolo", null, false, "TESTE");
        inativo.Inativar();

        contexto.Produtos.AddRange(
            new Produto(bolos.Id, "Bolo de Chocolate com Morango", "bolo-de-chocolate-com-morango",
                "Massa de chocolate, brigadeiro e morangos", 95m, "unidade (1,5 kg)", 1, "bolo", null, true, "TESTE"),
            new Produto(bolos.Id, "Bolo de Pote de Ninho", "bolo-de-pote-de-ninho",
                "Camadas de bolo e creme de leite Ninho", 15m, "pote 250 ml", 1, "bolo-pote", null, false, "TESTE"),
            new Produto(docinhos.Id, "Brigadeiro Gourmet", "brigadeiro-gourmet",
                "Chocolate belga", 2.5m, "unidade", 25, "brigadeiro", null, true, "TESTE"),
            inativo);
        contexto.SaveChanges();
    }
}
