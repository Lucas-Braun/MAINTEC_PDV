using PDV.Core.Interfaces;
using PDV.Core.Models;

namespace PDV.Infrastructure.Services;

public class ProdutoService : IProdutoService
{
    private readonly IApiClient _apiClient;

    public ProdutoService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<Produto?> BuscarPorCodigoBarras(string codigoBarras)
    {
        return _apiClient.BuscarProdutoPorCodigo(codigoBarras);
    }

    public Task<Produto?> BuscarPorCodigo(string codigoInterno)
    {
        return _apiClient.BuscarProdutoPorCodigo(codigoInterno);
    }

    public Task<List<Produto>> Pesquisar(string termo)
    {
        return _apiClient.PesquisarProdutos(termo, 50);
    }

    public Task AtualizarCacheLocal(List<Produto> produtos)
    {
        // Cache local desativado no modo API-first
        return Task.CompletedTask;
    }
}
