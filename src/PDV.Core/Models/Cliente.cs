namespace PDV.Core.Models;

public class Cliente
{
    public int Id { get; set; }          // agn_in_codigo
    public string Nome { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
}
