using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Categoria;
using EntreIrmas.Domain.Repositories;
using MapsterMapper;

namespace EntreIrmas.Application.AppServices;

public class CategoriaAppService : ICategoriaAppService
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IMapper _mapper;

    public CategoriaAppService(ICategoriaRepository categoriaRepository, IMapper mapper)
    {
        _categoriaRepository = categoriaRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.ListarComProdutosAsync(cancellationToken);

        // Categorias sem produto ativo não aparecem no cardápio.
        return _mapper.Map<List<CategoriaDto>>(categorias.Where(c => c.Produtos.Count > 0));
    }
}
