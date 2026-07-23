using System.Text.RegularExpressions;

namespace ConexaoSolidaria.CampaignApi.Domain.Validation;

public static partial class EmailValidator
{
    public static bool EhValido(string? email) =>
        !string.IsNullOrWhiteSpace(email) && EmailRegex().IsMatch(email);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
