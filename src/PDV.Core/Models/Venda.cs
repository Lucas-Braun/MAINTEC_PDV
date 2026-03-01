using PDV.Core.Enums;

namespace PDV.Core.Models;

public class Venda
{
    public int Id { get; set; }
    public string NumeroVenda { get; set; } = string.Empty;
    public DateTime DataVenda { get; set; } = DateTime.Now;
    public int OperadorId { get; set; }
    public int? ClienteId { get; set; }
    public string? ClienteCpfCnpj { get; set; }

    // Valores
    public decimal SubTotal => Itens.Sum(i => i.ValorTotal);
    public decimal DescontoTotal { get; set; }
    public decimal AcrescimoTotal { get; set; }
    public decimal ValorTotal => SubTotal - DescontoTotal + AcrescimoTotal;

    // Fiscal
    public string? ChaveNFCe { get; set; }
    public int? NumeroNFCe { get; set; }
    public string? ProtocoloAutorizacao { get; set; }
    public StatusVenda Status { get; set; } = StatusVenda.EmAberto;

    // Relacionamentos
    public List<ItemVenda> Itens { get; set; } = new();
    public List<Pagamento> Pagamentos { get; set; } = new();

    // Sincronização com ERP
    public bool SincronizadoERP { get; set; } = false;
    public DateTime? DataSincronizacao { get; set; }
    public string? ChaveIdempotencia { get; set; }
}
