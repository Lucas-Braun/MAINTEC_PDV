namespace PDV.Infrastructure.Api.DTOs;

public class VendaDTO
{
    public string NumeroVenda { get; set; } = string.Empty;
    public DateTime DataVenda { get; set; }
    public string? ClienteCpfCnpj { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal DescontoTotal { get; set; }
    public string? ChaveNfce { get; set; }
    public int? NumeroNfce { get; set; }
    public List<ItemVendaDTO> Itens { get; set; } = new();
    public List<PagamentoDTO> Pagamentos { get; set; } = new();
}

public class ItemVendaDTO
{
    public int ProdutoId { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal DescontoValor { get; set; }
    public decimal ValorTotal { get; set; }
}

public class PagamentoDTO
{
    public int FormaPagamento { get; set; }
    public decimal Valor { get; set; }
    public string? Nsu { get; set; }
    public string? CodigoAutorizacao { get; set; }
    public string? BandeiraCartao { get; set; }
    public int? Parcelas { get; set; }
}
