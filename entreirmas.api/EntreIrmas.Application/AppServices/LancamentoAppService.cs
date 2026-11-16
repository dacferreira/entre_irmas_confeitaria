using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Lancamento;
using EntreIrmas.Domain.Entities.LancamentoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Services;
using MapsterMapper;

namespace EntreIrmas.Application.AppServices;

public class LancamentoAppService : ILancamentoAppService
{
    private readonly ILancamentoRepository _lancamentoRepository;
    private readonly IAuthentication _authentication;
    private readonly IMapper _mapper;

    public LancamentoAppService(ILancamentoRepository lancamentoRepository, IAuthentication authentication, IMapper mapper)
    {
        _lancamentoRepository = lancamentoRepository;
        _authentication = authentication;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<LancamentoDto>> ListarAsync(FiltroLancamentoDto filtro,
        CancellationToken cancellationToken = default)
    {
        if (filtro.Inicio > filtro.Fim)
            throw new DomainException("A data final deve ser igual ou posterior à inicial.");

        return _mapper.Map<List<LancamentoDto>>(await _lancamentoRepository.ListarAsync(filtro, cancellationToken));
    }

    public async Task<LancamentoDto> CriarAsync(SalvarLancamentoDto dto, CancellationToken cancellationToken = default)
    {
        var lancamento = new Lancamento(dto.Tipo, dto.Descricao, dto.Valor, dto.Data, _authentication.Login);

        await _lancamentoRepository.AdicionarAsync(lancamento, cancellationToken);
        await _lancamentoRepository.SalvarAlteracoesAsync(cancellationToken);
        return _mapper.Map<LancamentoDto>(lancamento);
    }

    public async Task<LancamentoDto> AtualizarAsync(long id, SalvarLancamentoDto dto,
        CancellationToken cancellationToken = default)
    {
        var lancamento = await ObterAsync(id, cancellationToken);

        lancamento.Atualizar(dto.Tipo, dto.Descricao, dto.Valor, dto.Data);
        await _lancamentoRepository.SalvarAlteracoesAsync(cancellationToken);
        return _mapper.Map<LancamentoDto>(lancamento);
    }

    public async Task ExcluirAsync(long id, CancellationToken cancellationToken = default)
    {
        var lancamento = await ObterAsync(id, cancellationToken);

        lancamento.Arquivar();
        await _lancamentoRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    private async Task<Lancamento> ObterAsync(long id, CancellationToken cancellationToken) =>
        await _lancamentoRepository.ObterPorIdAsync(id, cancellationToken)
        ?? throw new ObjectNotFoundException("Lançamento não encontrado.");
}
