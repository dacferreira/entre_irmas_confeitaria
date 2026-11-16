using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Categoria;
using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Seedwork;
using EntreIrmas.Domain.Services;
using MapsterMapper;

namespace EntreIrmas.Application.AppServices;

public class CategoriaAppService : ICategoriaAppService
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IAuthentication _authentication;
    private readonly IMapper _mapper;

    public CategoriaAppService(ICategoriaRepository categoriaRepository, IAuthentication authentication, IMapper mapper)
    {
        _categoriaRepository = categoriaRepository;
        _authentication = authentication;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.ListarComProdutosAsync(cancellationToken);

        // Categorias sem produto ativo não aparecem no cardápio.
        return _mapper.Map<List<CategoriaDto>>(categorias.Where(c => c.Produtos.Count > 0));
    }

    public async Task<IReadOnlyList<CategoriaAdminDto>> ListarAdminAsync(CancellationToken cancellationToken = default) =>
        _mapper.Map<List<CategoriaAdminDto>>(await _categoriaRepository.ListarAdminAsync(cancellationToken));

    public async Task<CategoriaAdminDto> CriarAsync(SalvarCategoriaDto dto, CancellationToken cancellationToken = default)
    {
        var slug = await GerarSlugAsync(dto.Nome, null, cancellationToken);
        var categoria = new Categoria(dto.Nome, slug, dto.Descricao, dto.Ordem, _authentication.Login);

        await _categoriaRepository.AdicionarAsync(categoria, cancellationToken);
        await _categoriaRepository.SalvarAlteracoesAsync(cancellationToken);
        return _mapper.Map<CategoriaAdminDto>(categoria);
    }

    public async Task<CategoriaAdminDto> AtualizarAsync(long id, SalvarCategoriaDto dto,
        CancellationToken cancellationToken = default)
    {
        var categoria = await ObterAsync(id, cancellationToken);
        var slug = await GerarSlugAsync(dto.Nome, id, cancellationToken);

        categoria.Atualizar(dto.Nome, slug, dto.Descricao, dto.Ordem);
        await _categoriaRepository.SalvarAlteracoesAsync(cancellationToken);
        return _mapper.Map<CategoriaAdminDto>(categoria);
    }

    public async Task AlterarSituacaoAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var categoria = await ObterAsync(id, cancellationToken);

        if (ativo)
            categoria.Ativar();
        else
            categoria.Inativar();

        await _categoriaRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task ExcluirAsync(long id, CancellationToken cancellationToken = default)
    {
        var categoria = await ObterAsync(id, cancellationToken);

        categoria.Excluir();
        await _categoriaRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    private async Task<Categoria> ObterAsync(long id, CancellationToken cancellationToken) =>
        await _categoriaRepository.ObterAdminPorIdAsync(id, cancellationToken)
        ?? throw new ObjectNotFoundException("Categoria não encontrada.");

    private async Task<string> GerarSlugAsync(string nome, long? ignorarId, CancellationToken cancellationToken)
    {
        var slug = Slug.Gerar(nome);
        if (slug.Length == 0)
            throw new DomainException("Use letras ou números no nome da categoria.");
        if (slug.Length > Categoria.TamanhoMaximoDoSlug)
            slug = slug[..Categoria.TamanhoMaximoDoSlug].TrimEnd('-');

        if (await _categoriaRepository.ExisteSlugAsync(slug, ignorarId, cancellationToken))
            throw new DomainException("Já existe uma categoria com esse nome.");

        return slug;
    }
}
