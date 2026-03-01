namespace PDV.Core.Models;

public class VendaResumo
{
    public int NfInCodigo { get; set; }
    public int NfNumero { get; set; }
    public string Data { get; set; } = string.Empty;
    public string Hora { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusNome { get; set; } = string.Empty;
    public string StatusCor { get; set; } = "#FFFFFF";
    public decimal Total { get; set; }
    public int QtdItens { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string? CpfNota { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public int Caixa { get; set; }
    public string? Chave { get; set; }
    public string? MotivoRejeicao { get; set; }
}

public class VendaDetalhe
{
    public int NfInCodigo { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorRecebido { get; set; }
    public decimal Troco { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusNome { get; set; } = string.Empty;
    public string DataEmissao { get; set; } = string.Empty;
    public string? CpfNota { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public int QtdItens { get; set; }
    public string? NfceChave { get; set; }
    public string? NfceStatus { get; set; }
    public List<ItemVendaDetalhe> Itens { get; set; } = new();
}

public class ItemVendaDetalhe
{
    public int Sequencia { get; set; }
    public int ProInCodigo { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Unidade { get; set; } = "UN";
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Total { get; set; }
}
