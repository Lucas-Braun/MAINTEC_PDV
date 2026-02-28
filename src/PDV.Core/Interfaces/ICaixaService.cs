using PDV.Core.Models;

namespace PDV.Core.Interfaces;

public interface ICaixaService
{
    Task<Caixa> AbrirCaixa(int operadorId, int numeroCaixa, decimal valorAbertura);
    Task<Caixa?> ObterCaixaAberto(int operadorId);
    Task RegistrarSangria(decimal valor, string observacao);
    Task RegistrarSuprimento(decimal valor, string observacao);
    Task<Caixa> FecharCaixa(decimal valorFechamento);
}
