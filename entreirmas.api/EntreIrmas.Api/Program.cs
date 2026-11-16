using System.Text.Json.Serialization;
using EntreIrmas.Api.Extensions;
using EntreIrmas.Api.Middlewares;
using EntreIrmas.ViaCepService;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

var variaveis = builder.AddVariables();
builder.AddSerilog();

builder.Services
    .AddControllers(options => options.Filters.Add(new AuthorizeFilter()))
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddFluentValidation();

builder.Services
    .AddMySql(variaveis)
    .AddCorsPolicy(variaveis)
    .AddSwagger()
    .AddHealthCheck(variaveis)
    .AddAuthenticationJwt(variaveis)
    .AddRateLimit(variaveis, variaveis, variaveis)
    .AddViaCepApiService(variaveis)
    .AddMapsterMapper()
    .AddIoC()
    .AddResponseCompression(options => options.EnableForHttps = true);

// Atrás do nginx/ingress, o IP real do cliente vem no X-Forwarded-For (usado pelo limite de pedidos).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UsePathBase("/api");
app.UseCulturaPtBr();
app.UseSerilogRequestLog();
app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseMiddleware<GlobalErrorMiddleware>();
app.UseCors(CorsExtension.CorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.ConfigureSwaggerUi(variaveis);   // só se USE_SWAGGER = true
app.ConfiguraHealthCheck();          // /health
app.UseStaticFiles();
app.MapControllers();
app.Run();

namespace EntreIrmas.Api
{
    public partial class Program { } // para os testes de integração
}
