using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EntreIrmas.Api.Extensions;
using EntreIrmas.Domain.DTOs.Autenticacao;
using EntreIrmas.Domain.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

    private readonly SemaphoreSlim _travaDoLogin = new(1, 1);
    private HttpClient? _clienteLogado;

    /// <summary>Mesmas opções da API: enums como texto ("Confirmado", "Perda").</summary>
    public static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Cliente sem login (rotas públicas do site).</summary>
    public HttpClient Cliente { get; }

    public ApiFixture()
    {
        Cliente = CreateClient();

        using var escopo = Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<EntreIrmasDbContext>();
        DadosDeTeste.PopularCardapio(contexto);
        DadosDeTeste.PopularUsuarios(contexto);
    }

    /// <summary>Cliente com o token do usuário de teste (rotas da área administrativa).</summary>
    public async Task<HttpClient> ClienteLogadoAsync()
    {
        await _travaDoLogin.WaitAsync();
        try
        {
            if (_clienteLogado is not null)
                return _clienteLogado;

            var resposta = await Cliente.PostAsJsonAsync("/api/autenticacao/login",
                new LoginDto { Login = DadosDeTeste.LoginDeTeste, Senha = DadosDeTeste.SenhaDeTeste });
            resposta.EnsureSuccessStatusCode();
            var sessao = await resposta.Content.ReadFromJsonAsync<SessaoDto>();

            var cliente = CreateClient();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessao!.Token);
            return _clienteLogado = cliente;
        }
        finally
        {
            _travaDoLogin.Release();
        }
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

            // Nenhum teste chama o ViaCEP de verdade.
            services.RemoveAll<IViaCepService>();
            services.AddScoped<IViaCepService, ViaCepFalso>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Cliente.Dispose();
            _clienteLogado?.Dispose();
            _travaDoLogin.Dispose();
        }

        base.Dispose(disposing);
    }
}

[CollectionDefinition(Nome)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Nome = "Api";
}
