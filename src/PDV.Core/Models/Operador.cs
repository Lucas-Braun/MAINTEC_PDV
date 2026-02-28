namespace PDV.Core.Models;

public class Operador
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Perfil { get; set; } = "caixa"; // caixa, supervisor, admin
    public bool Ativo { get; set; } = true;
}
