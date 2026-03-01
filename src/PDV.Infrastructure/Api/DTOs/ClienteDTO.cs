using System.Text.Json.Serialization;

namespace PDV.Infrastructure.Api.DTOs;

public class ClienteBuscaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("clientes")]
    public List<ClienteApiDTO> Clientes { get; set; } = new();

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class ClienteBuscaDocResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("cliente")]
    public ClienteApiDTO? Cliente { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class ClienteApiDTO
{
    [JsonPropertyName("agn_in_codigo")]
    public int AgnInCodigo { get; set; }

    [JsonPropertyName("agn_st_nome")]
    public string AgnStNome { get; set; } = string.Empty;

    [JsonPropertyName("agn_st_cnpj_cpf")]
    public string? AgnStCnpjCpf { get; set; }

    [JsonPropertyName("agn_st_email")]
    public string? AgnStEmail { get; set; }

    [JsonPropertyName("agn_st_telefone")]
    public string? AgnStTelefone { get; set; }
}

public class CadastrarClienteRequest
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("cpf_cnpj")]
    public string? CpfCnpj { get; set; }

    [JsonPropertyName("telefone")]
    public string? Telefone { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

public class CadastrarClienteResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("cliente")]
    public ClienteApiDTO? Cliente { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("duplicado")]
    public bool? Duplicado { get; set; }

    [JsonPropertyName("cliente_existente")]
    public ClienteApiDTO? ClienteExistente { get; set; }
}
