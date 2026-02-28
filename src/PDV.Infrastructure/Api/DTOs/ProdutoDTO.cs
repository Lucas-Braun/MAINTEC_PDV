namespace PDV.Infrastructure.Api.DTOs;

public class ProdutoDTO
{
    public int Id { get; set; }
    public string? CodigoBarras { get; set; }
    public string? CodigoInterno { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? UnidadeMedida { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal EstoqueAtual { get; set; }
    public string? Ncm { get; set; }
    public string? Cfop { get; set; }
    public string? CstIcms { get; set; }
    public string? CstPis { get; set; }
    public string? CstCofins { get; set; }
    public string? Csosn { get; set; }
    public decimal AliquotaIcms { get; set; }
    public string? Cest { get; set; }
    public bool Ativo { get; set; }
    public DateTime UltimaAtualizacao { get; set; }
}
