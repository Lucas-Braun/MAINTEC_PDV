using PDV.Core.Models;

namespace PDV.Core.Interfaces;

public interface IVendaService
{
    Task<Venda> CriarVenda(int operadorId);
    Task SalvarVenda(Venda venda);
    Task CancelarVenda(int vendaId, string motivo);
    Task<List<Venda>> ObterVendasNaoSincronizadas();
    Task MarcarComoSincronizada(int vendaId);
}
