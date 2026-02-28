using Microsoft.EntityFrameworkCore;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using PDV.Infrastructure.LocalDb;

namespace PDV.Infrastructure.Services;

public class OperadorService : IOperadorService
{
    private readonly PdvDbContext _db;

    public Operador? OperadorLogado { get; private set; }

    public OperadorService(PdvDbContext db)
    {
        _db = db;
    }

    public async Task<Operador?> Autenticar(string login, string senha)
    {
        // TODO: Implementar hash de senha quando tiver tela de cadastro
        // Por enquanto aceita senha "123" para todos os operadores do seed
        var operador = await _db.Operadores
            .FirstOrDefaultAsync(o => o.Login.ToLower() == login.ToLower());

        if (operador != null && senha == "123")
        {
            OperadorLogado = operador;
            return operador;
        }

        return null;
    }

    public void Logout()
    {
        OperadorLogado = null;
    }
}
