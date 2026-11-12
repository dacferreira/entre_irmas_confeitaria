using System.Reflection;
using EntreIrmas.Api.EnviromentVariables;
using Microsoft.OpenApi;

namespace EntreIrmas.Api.Extensions;

public static class SwaggerExtension
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Entre Irmãs Confeitaria - API",
                Version = "v1",
                Description = "Cardápio e pedidos do site da confeitaria."
            });

            options.EnableAnnotations();

            var arquivoXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(arquivoXml))
                options.IncludeXmlComments(arquivoXml);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Token JWT no cabeçalho Authorization."
            });

            options.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", documento)] = new List<string>()
            });
        });

        return services;
    }

    /// <summary>Publica o Swagger em /api/swagger (pelo path base) quando USE_SWAGGER = true.</summary>
    public static IApplicationBuilder ConfigureSwaggerUi(this IApplicationBuilder app, ISwaggerEnvironmentVariables variaveis)
    {
        if (!variaveis.USE_SWAGGER)
            return app;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("v1/swagger.json", "Entre Irmãs v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
