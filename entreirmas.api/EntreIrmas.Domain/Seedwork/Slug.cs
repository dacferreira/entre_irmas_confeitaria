using System.Globalization;
using System.Text;

namespace EntreIrmas.Domain.Seedwork;

/// <summary>Gera o identificador de URL a partir de um nome ("Bolo de Pote!" vira "bolo-de-pote").</summary>
public static class Slug
{
    public static string Gerar(string? texto)
    {
        var semAcentos = new StringBuilder();
        foreach (var caractere in (texto ?? string.Empty).Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
                semAcentos.Append(caractere);
        }

        var slug = new StringBuilder();
        foreach (var caractere in semAcentos.ToString().ToLowerInvariant())
        {
            if (caractere is >= 'a' and <= 'z' or >= '0' and <= '9')
                slug.Append(caractere);
            else if (slug.Length > 0 && slug[^1] != '-')
                slug.Append('-');
        }

        return slug.ToString().Trim('-');
    }
}
