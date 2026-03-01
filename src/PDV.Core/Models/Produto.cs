namespace PDV.Core.Models;

public class Produto
{
    public int Id { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = "UN";
    public decimal PrecoVenda { get; set; }
    public decimal EstoqueAtual { get; set; }

    // Dados fiscais
    public string NCM { get; set; } = string.Empty;
    public string CFOP { get; set; } = "5102"; // Venda interna
    public string CST_ICMS { get; set; } = string.Empty;
    public string CST_PIS { get; set; } = string.Empty;
    public string CST_COFINS { get; set; } = string.Empty;
    public string CSOSN { get; set; } = string.Empty; // Simples Nacional
    public decimal AliquotaICMS { get; set; }
    public string CEST { get; set; } = string.Empty;

    // Imagem
    public string? FotoUrl { get; set; }

    // Controle
    public bool Ativo { get; set; } = true;
    public DateTime UltimaAtualizacao { get; set; }
}
