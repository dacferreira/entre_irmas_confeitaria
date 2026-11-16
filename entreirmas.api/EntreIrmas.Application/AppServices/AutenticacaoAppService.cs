using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Autenticacao;
using EntreIrmas.Domain.Entities.UsuarioAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Services;

namespace EntreIrmas.Application.AppServices;

public class AutenticacaoAppService : IAutenticacaoAppService
{
    internal const string MensagemDeFalha = "Usuário ou senha inválidos.";
    private const int NaoAutorizado = 401;

    // Hash de uma senha aleatória, conferido quando o login não existe: a resposta leva o mesmo tempo
    // e não revela quais logins são válidos.
    private static string? _hashFicticio;

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISenhaService _senhaService;
    private readonly ITokenService _tokenService;
    private readonly TimeProvider _timeProvider;

    public AutenticacaoAppService(IUsuarioRepository usuarioRepository, ISenhaService senhaService,
        ITokenService tokenService, TimeProvider timeProvider)
    {
        _usuarioRepository = usuarioRepository;
        _senhaService = senhaService;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
    }

    public async Task<SessaoDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorLoginAsync(Usuario.NormalizarLogin(dto.Login), cancellationToken);

        if (usuario is null)
        {
            _hashFicticio ??= _senhaService.GerarHash(Guid.NewGuid().ToString());
            _senhaService.Conferir(dto.Senha, _hashFicticio);
            throw new ApiException(NaoAutorizado, MensagemDeFalha);
        }

        if (!_senhaService.Conferir(dto.Senha, usuario.HashSenha))
            throw new ApiException(NaoAutorizado, MensagemDeFalha);

        usuario.RegistrarAcesso(_timeProvider.GetLocalNow().DateTime);
        await _usuarioRepository.SalvarAlteracoesAsync(cancellationToken);

        return _tokenService.GerarSessao(usuario);
    }
}
