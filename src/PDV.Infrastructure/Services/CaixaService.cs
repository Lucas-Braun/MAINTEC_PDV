using PDV.Core.Interfaces;
using PDV.Core.Models;

namespace PDV.Infrastructure.Services;

public class CaixaService : ICaixaService
{
    private readonly IApiClient _apiClient;

    public CaixaService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ResultadoAbrirCaixa> AbrirCaixa(decimal valorAbertura, int? terInCodigo)
    {
        return _apiClient.AbrirCaixa(valorAbertura, terInCodigo);
    }

    public Task<ResultadoCaixaStatus> ObterStatusCaixa()
    {
        return _apiClient.ObterStatusCaixa();
    }

    public Task<ResultadoOperacao> RegistrarSangria(decimal valor, string? observacao)
    {
        return _apiClient.RegistrarSangria(valor, observacao);
    }

    public Task<ResultadoOperacao> RegistrarSuprimento(decimal valor, string? observacao)
    {
        return _apiClient.RegistrarSuprimento(valor, observacao);
    }

    public Task<ResultadoFecharCaixa> FecharCaixa(decimal valorFechamento)
    {
        return _apiClient.FecharCaixa(valorFechamento);
    }

    public Task<ResultadoResumoCaixa> ObterResumoCaixa()
    {
        return _apiClient.ObterResumoCaixa();
    }
}
