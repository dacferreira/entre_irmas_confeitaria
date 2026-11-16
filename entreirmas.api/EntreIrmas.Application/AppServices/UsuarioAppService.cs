using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Usuario;
using EntreIrmas.Domain.Entities.UsuarioAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Seedwork;
using EntreIrmas.Domain.Services;
using MapsterMapper;

namespace EntreIrmas.Application.AppServices;

public class UsuarioAppService : IUsuarioAppService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISenhaService _senhaService;
    private readonly IAuthentication _authentication;
    private readonly IMapper _mapper;

    public UsuarioAppService(IUsuarioRepository usuarioRepository, ISenhaService senhaService,
        IAuthentication authentication, IMapper mapper)
    {
        _usuarioRepository = usuarioRepository;
        _senhaService = senhaService;
        _authentication = authentication;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(CancellationToken cancellationToken = default) =>
        _mapper.Map<List<UsuarioDto>>(await _usuarioRepository.ListarAdminAsync(cancellationToken));

    public async Task<UsuarioDto> CriarAsync(SalvarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(dto.Senha))
            throw new DomainException("Informe a senha do novo usuário.");

        var login = Usuario.NormalizarLogin(dto.Login);
        if (await _usuarioRepository.ExisteLoginAsync(login, null, cancellationToken))
            throw new DomainException($"Já existe um usuário com o login {login}.");

        var usuario = new Usuario(login, dto.Nome, _senhaService.GerarHash(dto.Senha), _authentication.Login);

        await _usuarioRepository.AdicionarAsync(usuario, cancellationToken);
        await _usuarioRepository.SalvarAlteracoesAsync(cancellationToken);
        return _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto> AtualizarAsync(long id, SalvarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await ObterAsync(id, cancellationToken);

        if (Usuario.NormalizarLogin(dto.Login) != usuario.Login)
            throw new DomainException("O login não pode ser alterado. Crie um novo usuário, se precisar.");

        usuario.AlterarNome(dto.Nome);
        if (!string.IsNullOrEmpty(dto.Senha))
            usuario.DefinirHashSenha(_senhaService.GerarHash(dto.Senha));

        await _usuarioRepository.SalvarAlteracoesAsync(cancellationToken);
        return _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task AlterarSituacaoAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var usuario = await ObterAsync(id, cancellationToken);

        if (ativo)
            usuario.Ativar();
        else
        {
            await GarantirQuePodePerderOAcessoAsync(usuario, "desativar", cancellationToken);
            usuario.Inativar();
        }

        await _usuarioRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task ExcluirAsync(long id, CancellationToken cancellationToken = default)
    {
        var usuario = await ObterAsync(id, cancellationToken);
        await GarantirQuePodePerderOAcessoAsync(usuario, "excluir", cancellationToken);

        usuario.Excluir();
        await _usuarioRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    private async Task<Usuario> ObterAsync(long id, CancellationToken cancellationToken) =>
        await _usuarioRepository.ObterAdminPorIdAsync(id, cancellationToken)
        ?? throw new ObjectNotFoundException("Usuário não encontrado.");

    // Ninguém tira o próprio acesso, e sempre sobra pelo menos um usuário ativo.
    private async Task GarantirQuePodePerderOAcessoAsync(Usuario usuario, string acao, CancellationToken cancellationToken)
    {
        if (usuario.Id == _authentication.Id)
            throw new DomainException($"Você não pode {acao} o seu próprio usuário.");

        if (usuario.Estado == EntityStatus.Ativo && await _usuarioRepository.ContarAtivosAsync(cancellationToken) <= 1)
            throw new DomainException("É preciso manter pelo menos um usuário ativo.");
    }
}
