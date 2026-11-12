using EntreIrmas.Api.Extensions;
using EntreIrmas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EntreIrmas.Test.Config;

/// <summary>Sobe a API no ambiente Testing, com banco em memória no lugar do MySQL.</summary>
public class ApiFixture : WebApplicationFactory<Api.Program>
{
    private readonly string _nomeDoBanco = $"entreirmas-testes-{Guid.NewGuid()}";

    public HttpClient Cliente { get; }

    public ApiFixture()
    {
        Cliente = CreateClient();

        using var escopo = Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<EntreIrmasDbContext>();
        DadosDeTeste.PopularCardapio(contexto);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnviromentExtension.AmbienteTesting);

        builder.ConfigureTestServices(services =>
        {
            // Remove o registro do Pomelo antes de registrar o provedor em memória.
            var registrosDoContexto = services
                .Where(s => s.ServiceType == typeof(DbContextOptions<EntreIrmasDbContext>)
                            || s.ServiceType == typeof(DbContextOptions)
                            || s.ServiceType == typeof(IDbContextOptionsConfiguration<EntreIrmasDbContext>))
                .ToList();

            foreach (var registro in registrosDoContexto)
                services.Remove(registro);

            services.AddDbContext<EntreIrmasDbContext>(options => options.UseInMemoryDatabase(_nomeDoBanco));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Cliente.Dispose();

        base.Dispose(disposing);
    }
}

[CollectionDefinition(Nome)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Nome = "Api";
}
