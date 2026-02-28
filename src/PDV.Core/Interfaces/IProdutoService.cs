using PDV.Core.Models;

namespace PDV.Core.Interfaces;

public interface IProdutoService
{
    Task<Produto?> BuscarPorCodigoBarras(string codigoBarras);
    Task<Produto?> BuscarPorCodigo(string codigoInterno);
    Task<List<Produto>> Pesquisar(string termo);
    Task AtualizarCacheLocal(List<Produto> produtos);
}
