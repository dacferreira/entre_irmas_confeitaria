using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Seedwork;
using EntreIrmas.Domain.Services;
using MapsterMapper;

namespace EntreIrmas.Application.AppServices;

public class ProdutoAppService : IProdutoAppService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IImagemProdutoRepository _imagemRepository;
    private readonly IAuthentication _authentication;
    private readonly IMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public ProdutoAppService(IProdutoRepository produtoRepository, ICategoriaRepository categoriaRepository,
        IImagemProdutoRepository imagemRepository, IAuthentication authentication, IMapper mapper,
        TimeProvider timeProvider)
    {
        _produtoRepository = produtoRepository;
        _categoriaRepository = categoriaRepository;
        _imagemRepository = imagemRepository;
        _authentication = authentication;
        _mapper = mapper;
        _timeProvider = timeProvider;
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

    public async Task<IReadOnlyList<ProdutoAdminDto>> ListarAdminAsync(FiltroProdutoAdminDto filtro,
        CancellationToken cancellationToken = default) =>
        _mapper.Map<List<ProdutoAdminDto>>(await _produtoRepository.ListarAdminAsync(filtro, cancellationToken));

    public async Task<ProdutoAdminDto> ObterAdminAsync(long id, CancellationToken cancellationToken = default) =>
        _mapper.Map<ProdutoAdminDto>(await ObterAsync(id, cancellationToken));

    public async Task<ProdutoAdminDto> CriarAsync(SalvarProdutoDto dto, CancellationToken cancellationToken = default)
    {
        var categoria = await ObterCategoriaAsync(dto.CategoriaId, cancellationToken);
        var slug = await GerarSlugAsync(dto.Nome, null, cancellationToken);

        var produto = new Produto(categoria.Id, dto.Nome, slug, dto.Descricao, dto.Preco, dto.Custo, dto.Unidade,
            dto.QuantidadeMinima, dto.Ilustracao, dto.Destaque, dto.Encomenda, _authentication.Login);

        await _produtoRepository.AdicionarAsync(produto, cancellationToken);
        await _produtoRepository.SalvarAlteracoesAsync(cancellationToken);
        return Mapear(produto, categoria);
    }

    public async Task<ProdutoAdminDto> AtualizarAsync(long id, SalvarProdutoDto dto, CancellationToken cancellationToken = default)
    {
        var produto = await ObterAsync(id, cancellationToken);
        var categoria = await ObterCategoriaAsync(dto.CategoriaId, cancellationToken);
        var slug = await GerarSlugAsync(dto.Nome, id, cancellationToken);

        produto.Atualizar(categoria.Id, dto.Nome, slug, dto.Descricao, dto.Preco, dto.Custo, dto.Unidade,
            dto.QuantidadeMinima, dto.Ilustracao, dto.Destaque, dto.Encomenda);

        await _produtoRepository.SalvarAlteracoesAsync(cancellationToken);
        return Mapear(produto, categoria);
    }

    public async Task AlterarSituacaoAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var produto = await ObterAsync(id, cancellationToken);

        if (ativo)
            produto.Ativar();
        else
            produto.Inativar();

        await _produtoRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task ExcluirAsync(long id, CancellationToken cancellationToken = default)
    {
        var produto = await ObterAsync(id, cancellationToken);

        produto.Excluir();
        // O produto excluído não aparece mais em lugar nenhum: a foto pode sair do banco.
        var imagem = await _imagemRepository.ObterPorProdutoAsync(id, cancellationToken);
        if (imagem is not null)
            _imagemRepository.Remover(imagem);

        await _produtoRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task<ProdutoAdminDto> DefinirImagemAsync(long id, byte[] conteudo,
        CancellationToken cancellationToken = default)
    {
        var produto = await ObterAsync(id, cancellationToken);
        var imagem = await _imagemRepository.ObterPorProdutoAsync(id, cancellationToken);

        if (imagem is null)
            await _imagemRepository.AdicionarAsync(new ImagemProduto(id, conteudo, _authentication.Login), cancellationToken);
        else
            imagem.Substituir(conteudo);

        // Versão nova a cada envio: a URL muda e o navegador busca a foto nova em vez da do cache.
        produto.DefinirImagem(_timeProvider.GetUtcNow().UtcTicks);
        await _produtoRepository.SalvarAlteracoesAsync(cancellationToken);
        return _mapper.Map<ProdutoAdminDto>(produto);
    }

    public async Task<ProdutoAdminDto> RemoverImagemAsync(long id, CancellationToken cancellationToken = default)
    {
        var produto = await ObterAsync(id, cancellationToken);
        var imagem = await _imagemRepository.ObterPorProdutoAsync(id, cancellationToken);

        if (imagem is not null)
            _imagemRepository.Remover(imagem);

        produto.DefinirImagem(null);
        await _produtoRepository.SalvarAlteracoesAsync(cancellationToken);
        return _mapper.Map<ProdutoAdminDto>(produto);
    }

    public async Task<ImagemDto> ObterImagemAsync(long id, CancellationToken cancellationToken = default)
    {
        var imagem = await _imagemRepository.ObterPorProdutoAsync(id, cancellationToken)
            ?? throw new ObjectNotFoundException("Foto não encontrada.");

        return new ImagemDto(imagem.TipoConteudo, imagem.Conteudo);
    }

    private async Task<Produto> ObterAsync(long id, CancellationToken cancellationToken) =>
        await _produtoRepository.ObterAdminPorIdAsync(id, cancellationToken)
        ?? throw new ObjectNotFoundException("Produto não encontrado.");

    private async Task<Categoria> ObterCategoriaAsync(long categoriaId, CancellationToken cancellationToken) =>
        await _categoriaRepository.ObterAdminPorIdAsync(categoriaId, cancellationToken)
        ?? throw new DomainException("Categoria não encontrada. Escolha outra.");

    private async Task<string> GerarSlugAsync(string nome, long? ignorarId, CancellationToken cancellationToken)
    {
        var slug = Slug.Gerar(nome);
        if (slug.Length == 0)
            throw new DomainException("Use letras ou números no nome do produto.");
        if (slug.Length > Produto.TamanhoMaximoDoSlug)
            slug = slug[..Produto.TamanhoMaximoDoSlug].TrimEnd('-');

        if (await _produtoRepository.ExisteSlugAsync(slug, ignorarId, cancellationToken))
            throw new DomainException("Já existe um produto com esse nome.");

        return slug;
    }

    // A categoria vem do cadastro (a navegação do produto pode não estar carregada).
    private ProdutoAdminDto Mapear(Produto produto, Categoria categoria)
    {
        var dto = _mapper.Map<ProdutoAdminDto>(produto);
        dto.CategoriaNome = categoria.Nome;
        return dto;
    }
}
