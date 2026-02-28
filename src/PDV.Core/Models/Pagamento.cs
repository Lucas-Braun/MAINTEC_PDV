using PDV.Core.Enums;

namespace PDV.Core.Models;

public class Pagamento
{
    public int Id { get; set; }
    public int VendaId { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public decimal Valor { get; set; }

    // TEF
    public string? NSU { get; set; }
    public string? CodigoAutorizacao { get; set; }
    public string? BandeiraCartao { get; set; }
    public int? Parcelas { get; set; }

    // Dinheiro
    public decimal? ValorRecebido { get; set; }
    public decimal? Troco => ValorRecebido.HasValue ? ValorRecebido.Value - Valor : null;

    // PIX
    public string? TxIdPix { get; set; }

    public DateTime DataPagamento { get; set; } = DateTime.Now;
}
