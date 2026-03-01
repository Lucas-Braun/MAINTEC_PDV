using System.Text.Json.Serialization;

namespace PDV.Infrastructure.Api.DTOs;

/// <summary>
/// Envelope padrao de resposta da API MEINTEC.
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    // Propriedade generica para o payload — mapeada pelo caller
    [JsonIgnore]
    public T? Data { get; set; }
}

public class ApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
