using PDV.Core.Models;

namespace PDV.Core.Interfaces;

public class ResultadoAutenticacao
{
    public bool Sucesso { get; set; }
    public Operador? Operador { get; set; }
    public string? Erro { get; set; }
}

public interface IOperadorService
{
    Task<ResultadoAutenticacao> Autenticar(string login, string senha);
    Operador? OperadorLogado { get; }
    void Logout();
}
