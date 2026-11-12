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

        // AppServices
        services.AddScoped<ICategoriaAppService, CategoriaAppService>();
        services.AddScoped<IProdutoAppService, ProdutoAppService>();
        services.AddScoped<IPedidoAppService, PedidoAppService>();

        // Services
        services.AddScoped<IMensagemPedidoService, MensagemPedidoService>();

        // Repositories
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();

        return services;
    }

    public static IServiceCollection AddMapsterMapper(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(CatalogoProfile).Assembly);
        services.AddMapster();
        return services;
    }
}
