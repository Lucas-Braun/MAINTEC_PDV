using PDV.Core.Models;

namespace PDV.Core.Interfaces;

public interface IApiClient
{
    Task<bool> Autenticar(string usuario, string senha);
    Task<List<Produto>> SincronizarProdutos(DateTime? ultimaSincronizacao = null);
    Task<Produto?> BuscarProduto(string codigoBarras);
    Task<bool> EnviarVenda(Venda venda);
    Task<List<Venda>> ObterVendasPendentes();
    Task<bool> VerificarConexao();
}
