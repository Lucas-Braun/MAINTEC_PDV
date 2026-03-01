using System.Text.Json.Serialization;

namespace PDV.Infrastructure.Api.DTOs;

public class LoginResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("usuario")]
    public UsuarioDTO? Usuario { get; set; }

    [JsonPropertyName("empresa")]
    public EmpresaDTO? Empresa { get; set; }

    [JsonPropertyName("filial")]
    public FilialDTO? Filial { get; set; }

    [JsonPropertyName("filiais")]
    public List<FilialDTO>? Filiais { get; set; }

    // Erros
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("tentativas_restantes")]
    public int? TentativasRestantes { get; set; }

    [JsonPropertyName("minutos_restantes")]
    public int? MinutosRestantes { get; set; }
}

public class UsuarioDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class EmpresaDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;
}

public class FilialDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("org_id")]
    public int OrgId { get; set; }

    [JsonPropertyName("org_nome")]
    public string? OrgNome { get; set; }
}

public class RefreshTokenResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}

public class SessaoResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("empresa")]
    public SessaoEmpresaDTO? Empresa { get; set; }

    [JsonPropertyName("filial")]
    public SessaoFilialDTO? Filial { get; set; }

    [JsonPropertyName("usuario")]
    public SessaoUsuarioDTO? Usuario { get; set; }
}

public class SessaoEmpresaDTO
{
    [JsonPropertyName("emp_in_codigo")]
    public int EmpInCodigo { get; set; }

    [JsonPropertyName("emp_st_nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("emp_st_cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("emp_st_ie")]
    public string? Ie { get; set; }
}

public class SessaoFilialDTO
{
    [JsonPropertyName("fil_in_codigo")]
    public int FilInCodigo { get; set; }

    [JsonPropertyName("fil_st_nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("fil_st_cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("fil_st_endereco")]
    public string? Endereco { get; set; }

    [JsonPropertyName("fil_st_cidade")]
    public string? Cidade { get; set; }

    [JsonPropertyName("fil_st_uf")]
    public string? Uf { get; set; }

    [JsonPropertyName("fil_st_cep")]
    public string? Cep { get; set; }

    [JsonPropertyName("fil_st_telefone")]
    public string? Telefone { get; set; }
}

public class SessaoUsuarioDTO
{
    [JsonPropertyName("user_in_codigo")]
    public int UserInCodigo { get; set; }

    [JsonPropertyName("user_st_nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("user_st_email")]
    public string? Email { get; set; }
}
