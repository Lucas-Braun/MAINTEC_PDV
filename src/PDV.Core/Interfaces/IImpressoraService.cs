using PDV.Core.Models;

namespace PDV.Core.Interfaces;

public interface IImpressoraService
{
    Task ImprimirCupom(Venda venda);
    Task ImprimirComprovanteTEF(string comprovante);
    Task ImprimirFechamentoCaixa(Caixa caixa);
    Task AbrirGaveta();
    Task CortarPapel();
    bool VerificarConexao();
}
