namespace PDV.Core.Helpers;

public static class CpfCnpjHelper
{
    public static string ApenasDigitos(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return string.Empty;
        return new string(valor.Where(char.IsDigit).ToArray());
    }

    public static bool ValidarCpf(string? cpf)
    {
        var digitos = ApenasDigitos(cpf);
        if (digitos.Length != 11) return false;
        if (digitos.Distinct().Count() == 1) return false;

        var soma = 0;
        for (int i = 0; i < 9; i++)
            soma += (digitos[i] - '0') * (10 - i);
        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;
        if (digitos[9] - '0' != d1) return false;

        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += (digitos[i] - '0') * (11 - i);
        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;
        return digitos[10] - '0' == d2;
    }

    public static bool ValidarCnpj(string? cnpj)
    {
        var digitos = ApenasDigitos(cnpj);
        if (digitos.Length != 14) return false;
        if (digitos.Distinct().Count() == 1) return false;

        int[] peso1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] peso2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var soma = 0;
        for (int i = 0; i < 12; i++)
            soma += (digitos[i] - '0') * peso1[i];
        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;
        if (digitos[12] - '0' != d1) return false;

        soma = 0;
        for (int i = 0; i < 13; i++)
            soma += (digitos[i] - '0') * peso2[i];
        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;
        return digitos[13] - '0' == d2;
    }

    public static bool Validar(string? valor)
    {
        var digitos = ApenasDigitos(valor);
        return digitos.Length switch
        {
            11 => ValidarCpf(digitos),
            14 => ValidarCnpj(digitos),
            _ => false
        };
    }

    public static string Formatar(string? valor)
    {
        var d = ApenasDigitos(valor);
        if (d.Length == 11)
            return $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}";
        if (d.Length == 14)
            return $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}";
        return d;
    }
}
