using System.Text.Json.Serialization;

namespace PDV.Infrastructure.Api.DTOs;

public class FinalizarVendaRequest
{
    [JsonPropertyName("itens")]
    public List<ItemVendaApiDTO> Itens { get; set; } = new();

    [JsonPropertyName("parcelas")]
    public List<ParcelaApiDTO> Parcelas { get; set; } = new();

    [JsonPropertyName("cpf_nota")]
    public string? CpfNota { get; set; }

    [JsonPropertyName("troco")]
    public decimal? Troco { get; set; }
}

public class ItemVendaApiDTO
{
    [JsonPropertyName("pro_in_codigo")]
    public int ProInCodigo { get; set; }

    [JsonPropertyName("quantidade")]
    public decimal Quantidade { get; set; }

    [JsonPropertyName("preco_unitario")]
    public decimal? PrecoUnitario { get; set; }

    [JsonPropertyName("desconto_perc")]
    public decimal? DescontoPerc { get; set; }
}

public class ParcelaApiDTO
{
    [JsonPropertyName("fcb_in_codigo")]
    public int FcbInCodigo { get; set; }

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("vencimento")]
    public string? Vencimento { get; set; }
}

public class FinalizarVendaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("from_cache")]
    public bool? FromCache { get; set; }

    [JsonPropertyName("resultado")]
    public ResultadoVendaDTO? Resultado { get; set; }
}

public class ResultadoVendaDTO
{
    [JsonPropertyName("ped_in_codigo")]
    public int PedInCodigo { get; set; }

    [JsonPropertyName("nf_in_codigo")]
    public int? NfInCodigo { get; set; }

    [JsonPropertyName("valor_venda")]
    public decimal? ValorVenda { get; set; }

    [JsonPropertyName("troco")]
    public decimal? Troco { get; set; }

    [JsonPropertyName("nfce_status")]
    public string? NfceStatus { get; set; }

    [JsonPropertyName("nfce_chave")]
    public string? NfceChave { get; set; }
}
