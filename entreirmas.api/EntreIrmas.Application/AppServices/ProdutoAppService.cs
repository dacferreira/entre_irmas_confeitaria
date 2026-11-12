using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;
using MapsterMapper;

namespace EntreIrmas.Application.AppServices;

public class ProdutoAppService : IProdutoAppService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMapper _mapper;

    public ProdutoAppService(IProdutoRepository produtoRepository, IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProdutoDto>> ListarAsync(FiltroProdutoDto filtro, CancellationToken cancellationToken = default)
    {
        var produtos = await _produtoRepository.ListarAsync(filtro, cancellationToken);
        return _mapper.Map<List<ProdutoDto>>(produtos);
    }

    public async Task<ProdutoDto> ObterPorSlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var produto = await _produtoRepository.ObterPorSlugAsync(slug.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new ObjectNotFoundException("Produto não encontrado.");

        return _mapper.Map<ProdutoDto>(produto);
    }
}
