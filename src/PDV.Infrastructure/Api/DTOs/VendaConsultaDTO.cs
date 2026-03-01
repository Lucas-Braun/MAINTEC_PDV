using System.Text.Json.Serialization;

namespace PDV.Infrastructure.Api.DTOs;

public class ListarVendasResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("vendas")]
    public List<VendaResumoDTO> Vendas { get; set; } = new();

    [JsonPropertyName("total_registros")]
    public int TotalRegistros { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class VendaResumoDTO
{
    [JsonPropertyName("nf_in_codigo")]
    public int NfInCodigo { get; set; }

    [JsonPropertyName("nf_numero")]
    public int NfNumero { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    [JsonPropertyName("hora")]
    public string Hora { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("status_nome")]
    public string StatusNome { get; set; } = string.Empty;

    [JsonPropertyName("status_cor")]
    public string StatusCor { get; set; } = "#FFFFFF";

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("qtd_itens")]
    public int QtdItens { get; set; }

    [JsonPropertyName("cliente_nome")]
    public string ClienteNome { get; set; } = string.Empty;

    [JsonPropertyName("cpf_nota")]
    public string? CpfNota { get; set; }

    [JsonPropertyName("forma_pagamento")]
    public string FormaPagamento { get; set; } = string.Empty;

    [JsonPropertyName("caixa")]
    public int Caixa { get; set; }

    [JsonPropertyName("chave")]
    public string? Chave { get; set; }

    [JsonPropertyName("motivo_rejeicao")]
    public string? MotivoRejeicao { get; set; }
}

public class DetalheVendaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("venda")]
    public VendaDetalheDTO? Venda { get; set; }

    [JsonPropertyName("itens")]
    public List<ItemVendaDetalheDTO> Itens { get; set; } = new();

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class VendaDetalheDTO
{
    [JsonPropertyName("nf_in_codigo")]
    public int NfInCodigo { get; set; }

    [JsonPropertyName("valor_total")]
    public decimal ValorTotal { get; set; }

    [JsonPropertyName("valor_recebido")]
    public decimal ValorRecebido { get; set; }

    [JsonPropertyName("troco")]
    public decimal Troco { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("status_nome")]
    public string StatusNome { get; set; } = string.Empty;

    [JsonPropertyName("data_emissao")]
    public string DataEmissao { get; set; } = string.Empty;

    [JsonPropertyName("cpf_nota")]
    public string? CpfNota { get; set; }

    [JsonPropertyName("forma_pagamento")]
    public string FormaPagamento { get; set; } = string.Empty;

    [JsonPropertyName("qtd_itens")]
    public int QtdItens { get; set; }

    [JsonPropertyName("nfce")]
    public NfceInfoDTO? Nfce { get; set; }
}

public class NfceInfoDTO
{
    [JsonPropertyName("chave")]
    public string? Chave { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class ItemVendaDetalheDTO
{
    [JsonPropertyName("sequencia")]
    public int Sequencia { get; set; }

    [JsonPropertyName("pro_in_codigo")]
    public int ProInCodigo { get; set; }

    [JsonPropertyName("codigo")]
    public string Codigo { get; set; } = string.Empty;

    [JsonPropertyName("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [JsonPropertyName("unidade")]
    public string Unidade { get; set; } = "UN";

    [JsonPropertyName("quantidade")]
    public decimal Quantidade { get; set; }

    [JsonPropertyName("preco_unitario")]
    public decimal PrecoUnitario { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }
}
