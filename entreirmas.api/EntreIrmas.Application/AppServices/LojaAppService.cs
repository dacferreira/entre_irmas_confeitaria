using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Loja;
using EntreIrmas.Domain.Entities.LojaAggregate;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Services;
using MapsterMapper;

namespace EntreIrmas.Application.AppServices;

public class LojaAppService : ILojaAppService
{
    private readonly ILojaRepository _lojaRepository;
    private readonly IAuthentication _authentication;
    private readonly IMapper _mapper;

    public LojaAppService(ILojaRepository lojaRepository, IAuthentication authentication, IMapper mapper)
    {
        _lojaRepository = lojaRepository;
        _authentication = authentication;
        _mapper = mapper;
    }

    public async Task<SituacaoLojaDto> ObterSituacaoAsync(CancellationToken cancellationToken = default)
    {
        var loja = await _lojaRepository.ObterAsync(cancellationToken);
        return loja is null ? new SituacaoLojaDto { Aberta = true } : _mapper.Map<SituacaoLojaDto>(loja);
    }

    public async Task<LojaAdminDto> ObterAsync(CancellationToken cancellationToken = default)
    {
        var loja = await _lojaRepository.ObterAsync(cancellationToken);
        return loja is null ? new LojaAdminDto { Aberta = true } : _mapper.Map<LojaAdminDto>(loja);
    }

    public async Task<LojaAdminDto> AlterarAsync(AlterarLojaDto dto, CancellationToken cancellationToken = default)
    {
        var loja = await _lojaRepository.ObterAsync(cancellationToken);
        if (loja is null)
        {
            // Banco sem o registro (script não aplicado ou base de testes): cria na primeira alteração.
            loja = new Loja(_authentication.Login);
            await _lojaRepository.AdicionarAsync(loja, cancellationToken);
        }

        if (dto.Aberta)
            loja.Abrir(_authentication.Login);
        else
            loja.Fechar(dto.Mensagem, _authentication.Login);

        await _lojaRepository.SalvarAlteracoesAsync(cancellationToken);
        return _mapper.Map<LojaAdminDto>(loja);
    }
}
