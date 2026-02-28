using PDV.Core.Models;

namespace PDV.Core.Interfaces;

public interface IOperadorService
{
    Task<Operador?> Autenticar(string login, string senha);
    Operador? OperadorLogado { get; }
    void Logout();
}
