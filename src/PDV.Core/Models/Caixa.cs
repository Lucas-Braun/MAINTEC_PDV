namespace PDV.Core.Models;

public class Caixa
{
    public int Id { get; set; }
    public int NumeroCaixa { get; set; }
    public int OperadorId { get; set; }
    public string NomeOperador { get; set; } = string.Empty;

    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public decimal ValorAbertura { get; set; }

    // Totais calculados
    public decimal TotalVendas { get; set; }
    public decimal TotalDinheiro { get; set; }
    public decimal TotalCartaoCredito { get; set; }
    public decimal TotalCartaoDebito { get; set; }
    public decimal TotalPix { get; set; }
    public decimal TotalSangria { get; set; }
    public decimal TotalSuprimento { get; set; }
    public decimal TotalCancelamentos { get; set; }

    public decimal SaldoEsperado => ValorAbertura + TotalDinheiro + TotalSuprimento - TotalSangria;
    public decimal? ValorFechamento { get; set; }
    public decimal? Diferenca => ValorFechamento.HasValue ? ValorFechamento.Value - SaldoEsperado : null;

    public bool Aberto => DataFechamento == null;
    public List<MovimentoCaixa> Movimentos { get; set; } = new();
}
