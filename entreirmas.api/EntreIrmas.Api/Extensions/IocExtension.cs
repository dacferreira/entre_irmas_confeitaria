using EntreIrmas.Api.Services;
using EntreIrmas.Application.AppServices;
using EntreIrmas.Application.Profiles;
using EntreIrmas.Application.Services;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Services;
using EntreIrmas.Infrastructure.Persistence.Repositories;
using Mapster;

namespace EntreIrmas.Api.Extensions;

public static class IocExtension
{
    public static IServiceCollection AddIoC(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();

        // AppServices
        services.AddScoped<ICategoriaAppService, CategoriaAppService>();
        services.AddScoped<IProdutoAppService, ProdutoAppService>();
        services.AddScoped<IPedidoAppService, PedidoAppService>();
        services.AddScoped<IAutenticacaoAppService, AutenticacaoAppService>();
        services.AddScoped<IUsuarioAppService, UsuarioAppService>();
        services.AddScoped<ILancamentoAppService, LancamentoAppService>();
        services.AddScoped<ILojaAppService, LojaAppService>();
        services.AddScoped<IPainelAppService, PainelAppService>();
        services.AddScoped<IEnderecoAppService, EnderecoAppService>();
        services.AddScoped<IEntregaAppService, EntregaAppService>();

        // Services
        services.AddScoped<IMensagemPedidoService, MensagemPedidoService>();
        services.AddScoped<IEntregaService, EntregaService>();
        services.AddScoped<IAuthentication, HttpContextAuthentication>();
        services.AddSingleton<ISenhaService, SenhaService>();
        services.AddSingleton<ITokenService, TokenService>();

        // Repositories
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IImagemProdutoRepository, ImagemProdutoRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ILancamentoRepository, LancamentoRepository>();
        services.AddScoped<ILojaRepository, LojaRepository>();

        return services;
    }

    public static IServiceCollection AddMapsterMapper(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(CatalogoProfile).Assembly);
        services.AddMapster();
        return services;
    }
}
