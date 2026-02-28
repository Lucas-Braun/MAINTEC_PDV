namespace PDV.Core.Models;

public class ItemVenda
{
    public int Id { get; set; }
    public int VendaId { get; set; }
    public int ProdutoId { get; set; }
    public int NumeroItem { get; set; }

    public string CodigoBarras { get; set; } = string.Empty;
    public string DescricaoProduto { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = "UN";

    public decimal Quantidade { get; set; } = 1;
    public decimal PrecoUnitario { get; set; }
    public decimal DescontoPercentual { get; set; }
    public decimal DescontoValor { get; set; }
    public decimal ValorTotal => (Quantidade * PrecoUnitario) - DescontoValor;

    // Dados fiscais do item
    public string NCM { get; set; } = string.Empty;
    public string CFOP { get; set; } = string.Empty;
    public string CST_ICMS { get; set; } = string.Empty;
    public decimal AliquotaICMS { get; set; }
    public decimal ValorICMS => ValorTotal * (AliquotaICMS / 100);

    // Navegação
    public Produto? Produto { get; set; }
}
