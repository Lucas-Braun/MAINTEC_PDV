using PDV.Core.Models;

namespace PDV.Core.Interfaces;

public interface ICaixaService
{
    Task<ResultadoAbrirCaixa> AbrirCaixa(decimal valorAbertura, int? terInCodigo);
    Task<ResultadoCaixaStatus> ObterStatusCaixa();
    Task<ResultadoOperacao> RegistrarSangria(decimal valor, string? observacao);
    Task<ResultadoOperacao> RegistrarSuprimento(decimal valor, string? observacao);
    Task<ResultadoFecharCaixa> FecharCaixa(decimal valorFechamento);
    Task<ResultadoResumoCaixa> ObterResumoCaixa();
}
