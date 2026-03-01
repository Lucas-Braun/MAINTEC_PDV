using Microsoft.EntityFrameworkCore;
using PDV.Core.Enums;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using PDV.Infrastructure.LocalDb;

namespace PDV.Infrastructure.Services;

public class VendaService : IVendaService
{
    private readonly PdvDbContext _db;

    public VendaService(PdvDbContext db)
    {
        _db = db;
    }

    public async Task<Venda> CriarVenda(int operadorId)
    {
        // Gera numero sequencial: YYYYMMDD-NNNN
        var hoje = DateTime.Now.ToString("yyyyMMdd");
        var count = await _db.Vendas
            .CountAsync(v => v.NumeroVenda.StartsWith(hoje));

        var venda = new Venda
        {
            NumeroVenda = $"{hoje}-{(count + 1):D4}",
            OperadorId = operadorId,
            DataVenda = DateTime.Now,
            Status = StatusVenda.EmAberto
        };

        _db.Vendas.Add(venda);
        await _db.SaveChangesAsync();
        return venda;
    }

    public async Task SalvarVenda(Venda venda)
    {
        var existente = await _db.Vendas
            .Include(v => v.Itens)
            .Include(v => v.Pagamentos)
            .FirstOrDefaultAsync(v => v.Id == venda.Id);

        if (existente != null)
        {
            // Atualiza campos
            existente.Status = venda.Status;
            existente.DescontoTotal = venda.DescontoTotal;
            existente.AcrescimoTotal = venda.AcrescimoTotal;
            existente.ClienteId = venda.ClienteId;
            existente.ClienteCpfCnpj = venda.ClienteCpfCnpj;
            existente.ChaveNFCe = venda.ChaveNFCe;
            existente.NumeroNFCe = venda.NumeroNFCe;
            existente.ProtocoloAutorizacao = venda.ProtocoloAutorizacao;
            existente.DataVenda = venda.DataVenda;
            existente.SincronizadoERP = venda.SincronizadoERP;
            existente.DataSincronizacao = venda.DataSincronizacao;
            existente.ChaveIdempotencia = venda.ChaveIdempotencia;

            // Remove itens/pagamentos antigos e adiciona novos
            _db.ItensVenda.RemoveRange(existente.Itens);
            _db.Pagamentos.RemoveRange(existente.Pagamentos);

            foreach (var item in venda.Itens)
            {
                item.VendaId = existente.Id;
                item.Id = 0;
                _db.ItensVenda.Add(item);
            }

            foreach (var pag in venda.Pagamentos)
            {
                pag.VendaId = existente.Id;
                pag.Id = 0;
                _db.Pagamentos.Add(pag);
            }
        }
        else
        {
            // Venda nova - gera numero se nao tiver
            if (string.IsNullOrEmpty(venda.NumeroVenda))
            {
                var hoje = DateTime.Now.ToString("yyyyMMdd");
                var count = await _db.Vendas
                    .CountAsync(v => v.NumeroVenda.StartsWith(hoje));
                venda.NumeroVenda = $"{hoje}-{(count + 1):D4}";
            }

            _db.Vendas.Add(venda);
        }

        await _db.SaveChangesAsync();
    }

    public async Task CancelarVenda(int vendaId, string motivo)
    {
        var venda = await _db.Vendas.FindAsync(vendaId);
        if (venda == null) return;

        venda.Status = StatusVenda.Cancelada;
        await _db.SaveChangesAsync();
    }

    public async Task<List<Venda>> ObterVendasNaoSincronizadas()
    {
        return await _db.Vendas
            .Include(v => v.Itens)
            .Include(v => v.Pagamentos)
            .Where(v => !v.SincronizadoERP &&
                (v.Status == StatusVenda.Finalizada || v.Status == StatusVenda.Contingencia))
            .OrderBy(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task MarcarComoSincronizada(int vendaId)
    {
        var venda = await _db.Vendas.FindAsync(vendaId);
        if (venda == null) return;

        venda.SincronizadoERP = true;
        venda.DataSincronizacao = DateTime.Now;
        await _db.SaveChangesAsync();
    }
}
