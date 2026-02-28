using Microsoft.EntityFrameworkCore;
using PDV.Core.Enums;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using PDV.Infrastructure.LocalDb;

namespace PDV.Infrastructure.Services;

public class CaixaService : ICaixaService
{
    private readonly PdvDbContext _db;
    private readonly IOperadorService _operadorService;
    private Caixa? _caixaAtual;

    public CaixaService(PdvDbContext db, IOperadorService operadorService)
    {
        _db = db;
        _operadorService = operadorService;
    }

    public async Task<Caixa> AbrirCaixa(int operadorId, int numeroCaixa, decimal valorAbertura)
    {
        // Verifica se ja existe caixa aberto para este operador
        var caixaAberto = await ObterCaixaAberto(operadorId);
        if (caixaAberto != null)
            throw new InvalidOperationException("Ja existe um caixa aberto para este operador");

        var operador = _operadorService.OperadorLogado;

        var caixa = new Caixa
        {
            NumeroCaixa = numeroCaixa,
            OperadorId = operadorId,
            NomeOperador = operador?.Nome ?? "Operador",
            DataAbertura = DateTime.Now,
            ValorAbertura = valorAbertura
        };

        // Registra movimento de abertura
        caixa.Movimentos.Add(new MovimentoCaixa
        {
            Tipo = TipoMovimentoCaixa.Abertura,
            Valor = valorAbertura,
            Observacao = "Abertura de caixa",
            DataHora = DateTime.Now,
            OperadorId = operadorId
        });

        _db.Caixas.Add(caixa);
        await _db.SaveChangesAsync();

        _caixaAtual = caixa;
        return caixa;
    }

    public async Task<Caixa?> ObterCaixaAberto(int operadorId)
    {
        if (_caixaAtual != null && _caixaAtual.Aberto && _caixaAtual.OperadorId == operadorId)
            return _caixaAtual;

        _caixaAtual = await _db.Caixas
            .Include(c => c.Movimentos)
            .Where(c => c.OperadorId == operadorId && c.DataFechamento == null)
            .OrderByDescending(c => c.DataAbertura)
            .FirstOrDefaultAsync();

        return _caixaAtual;
    }

    public async Task RegistrarSangria(decimal valor, string observacao)
    {
        var caixa = _caixaAtual
            ?? throw new InvalidOperationException("Nenhum caixa aberto");

        var movimento = new MovimentoCaixa
        {
            CaixaId = caixa.Id,
            Tipo = TipoMovimentoCaixa.Sangria,
            Valor = valor,
            Observacao = observacao,
            DataHora = DateTime.Now,
            OperadorId = caixa.OperadorId
        };

        caixa.TotalSangria += valor;
        caixa.Movimentos.Add(movimento);

        _db.MovimentosCaixa.Add(movimento);
        await _db.SaveChangesAsync();
    }

    public async Task RegistrarSuprimento(decimal valor, string observacao)
    {
        var caixa = _caixaAtual
            ?? throw new InvalidOperationException("Nenhum caixa aberto");

        var movimento = new MovimentoCaixa
        {
            CaixaId = caixa.Id,
            Tipo = TipoMovimentoCaixa.Suprimento,
            Valor = valor,
            Observacao = observacao,
            DataHora = DateTime.Now,
            OperadorId = caixa.OperadorId
        };

        caixa.TotalSuprimento += valor;
        caixa.Movimentos.Add(movimento);

        _db.MovimentosCaixa.Add(movimento);
        await _db.SaveChangesAsync();
    }

    public async Task<Caixa> FecharCaixa(decimal valorFechamento)
    {
        var caixa = _caixaAtual
            ?? throw new InvalidOperationException("Nenhum caixa aberto");

        // Recalcula totais das vendas do dia
        var vendas = await _db.Vendas
            .Include(v => v.Pagamentos)
            .Where(v => v.OperadorId == caixa.OperadorId
                && v.DataVenda >= caixa.DataAbertura
                && v.Status == StatusVenda.Finalizada)
            .ToListAsync();

        caixa.TotalVendas = vendas.Sum(v => v.Itens.Sum(i => (i.Quantidade * i.PrecoUnitario) - i.DescontoValor) - v.DescontoTotal + v.AcrescimoTotal);

        caixa.TotalDinheiro = vendas
            .SelectMany(v => v.Pagamentos)
            .Where(p => p.FormaPagamento == FormaPagamento.Dinheiro)
            .Sum(p => p.Valor);

        caixa.TotalCartaoCredito = vendas
            .SelectMany(v => v.Pagamentos)
            .Where(p => p.FormaPagamento == FormaPagamento.CartaoCredito)
            .Sum(p => p.Valor);

        caixa.TotalCartaoDebito = vendas
            .SelectMany(v => v.Pagamentos)
            .Where(p => p.FormaPagamento == FormaPagamento.CartaoDebito)
            .Sum(p => p.Valor);

        caixa.TotalPix = vendas
            .SelectMany(v => v.Pagamentos)
            .Where(p => p.FormaPagamento == FormaPagamento.PIX)
            .Sum(p => p.Valor);

        var canceladas = await _db.Vendas
            .Where(v => v.OperadorId == caixa.OperadorId
                && v.DataVenda >= caixa.DataAbertura
                && v.Status == StatusVenda.Cancelada)
            .CountAsync();

        caixa.TotalCancelamentos = canceladas;
        caixa.ValorFechamento = valorFechamento;
        caixa.DataFechamento = DateTime.Now;

        // Movimento de fechamento
        caixa.Movimentos.Add(new MovimentoCaixa
        {
            CaixaId = caixa.Id,
            Tipo = TipoMovimentoCaixa.Fechamento,
            Valor = valorFechamento,
            Observacao = $"Fechamento - Diferenca: {caixa.Diferenca:N2}",
            DataHora = DateTime.Now,
            OperadorId = caixa.OperadorId
        });

        await _db.SaveChangesAsync();

        _caixaAtual = null;
        return caixa;
    }
}
