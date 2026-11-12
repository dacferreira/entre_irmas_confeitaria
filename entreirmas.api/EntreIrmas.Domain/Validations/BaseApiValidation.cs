using FluentValidation;

namespace EntreIrmas.Domain.Validations;

/// <summary>Base dos validadores de entrada da API, com mensagens padronizadas.</summary>
/// <typeparam name="T">Objeto validado.</typeparam>
/// <typeparam name="TClass">O próprio validador.</typeparam>
public abstract class BaseApiValidation<T, TClass> : AbstractValidator<T>
    where TClass : BaseApiValidation<T, TClass>
{
    protected static string Obrigatorio(string campo) => $"{campo} é obrigatório.";
    protected static string TamanhoMaximo(string campo, int tamanho) => $"{campo} deve ter no máximo {tamanho} caracteres.";
}
