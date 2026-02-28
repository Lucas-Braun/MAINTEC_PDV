using PDV.Core.Enums;

namespace PDV.Core.Models;

public class MovimentoCaixa
{
    public int Id { get; set; }
    public int CaixaId { get; set; }
    public TipoMovimentoCaixa Tipo { get; set; }
    public decimal Valor { get; set; }
    public string? Observacao { get; set; }
    public DateTime DataHora { get; set; } = DateTime.Now;
    public int OperadorId { get; set; }
}
