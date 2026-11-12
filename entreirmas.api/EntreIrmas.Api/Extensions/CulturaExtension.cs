using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace EntreIrmas.Api.Extensions;

public static class CulturaExtension
{
    private static readonly CultureInfo CulturaPtBr = new("pt-BR");

    public static IApplicationBuilder UseCulturaPtBr(this IApplicationBuilder app)
    {
        CultureInfo.DefaultThreadCurrentCulture = CulturaPtBr;
        CultureInfo.DefaultThreadCurrentUICulture = CulturaPtBr;

        return app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(CulturaPtBr),
            SupportedCultures = [CulturaPtBr],
            SupportedUICultures = [CulturaPtBr]
        });
    }
}
