using EntreIrmas.Domain.DTOs.Categoria;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Seedwork;
using Mapster;

namespace EntreIrmas.Application.Profiles;

public class CatalogoProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Categoria, CategoriaDto>()
            .Map(dest => dest.QuantidadeProdutos, src => src.Produtos.Count);

        config.NewConfig<Produto, ProdutoDto>()
            .Map(dest => dest.CategoriaSlug, src => src.Categoria != null ? src.Categoria.Slug : string.Empty)
            .Map(dest => dest.CategoriaNome, src => src.Categoria != null ? src.Categoria.Nome : string.Empty);

        // Área administrativa
        config.NewConfig<Categoria, CategoriaAdminDto>()
            .Map(dest => dest.Ativo, src => src.Estado == EntityStatus.Ativo)
            .Map(dest => dest.QuantidadeProdutos, src => src.Produtos.Count);

        config.NewConfig<Produto, ProdutoAdminDto>()
            .Map(dest => dest.Ativo, src => src.Estado == EntityStatus.Ativo)
            .Map(dest => dest.CategoriaNome, src => src.Categoria != null ? src.Categoria.Nome : string.Empty);
    }
}
