using System.Text.Json.Serialization;

namespace PDV.Infrastructure.Api.DTOs;

public class ProdutoBuscaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("produto")]
    public ProdutoApiDTO? Produto { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class ProdutosPesquisaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("produtos")]
    public List<ProdutoApiDTO> Produtos { get; set; } = new();
}

public class ProdutoApiDTO
{
    [JsonPropertyName("pro_in_codigo")]
    public int ProInCodigo { get; set; }

    [JsonPropertyName("pro_st_descricao")]
    public string ProStDescricao { get; set; } = string.Empty;

    [JsonPropertyName("pro_st_referencia")]
    public string? ProStReferencia { get; set; }

    [JsonPropertyName("pro_st_codigo_barras")]
    public string? ProStCodigoBarras { get; set; }

    [JsonPropertyName("unidade")]
    public string Unidade { get; set; } = "UN";

    [JsonPropertyName("preco")]
    public decimal Preco { get; set; }

    [JsonPropertyName("estoque")]
    public decimal Estoque { get; set; }

    [JsonPropertyName("ncm")]
    public string? Ncm { get; set; }

    [JsonPropertyName("cfop")]
    public string? Cfop { get; set; }

    [JsonPropertyName("cst_icms")]
    public string? CstIcms { get; set; }

    [JsonPropertyName("csosn")]
    public string? Csosn { get; set; }

    [JsonPropertyName("aliquota_icms")]
    public decimal AliquotaIcms { get; set; }

    [JsonPropertyName("cest")]
    public string? Cest { get; set; }
}
