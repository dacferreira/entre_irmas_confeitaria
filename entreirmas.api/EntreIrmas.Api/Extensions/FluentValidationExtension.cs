using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.Validations;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace EntreIrmas.Api.Extensions;

public static class FluentValidationExtension
{
    public static IMvcBuilder AddFluentValidation(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.Services.AddFluentValidationAutoValidation();
        mvcBuilder.Services.AddValidatorsFromAssemblyContaining<CriarPedidoValidation>();

        // Resposta 400 padronizada para erros de entrada.
        mvcBuilder.ConfigureApiBehaviorOptions(options =>
            options.InvalidModelStateResponseFactory = context =>
                new BadRequestObjectResult(ValidationResult.FromModelState(context.ModelState)));

        return mvcBuilder;
    }
}
