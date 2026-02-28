using Microsoft.EntityFrameworkCore;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using PDV.Infrastructure.LocalDb;

namespace PDV.Infrastructure.Services;

public class ProdutoService : IProdutoService
{
    private readonly PdvDbContext _db;

    public ProdutoService(PdvDbContext db)
    {
        _db = db;
    }

    public async Task<Produto?> BuscarPorCodigoBarras(string codigoBarras)
    {
        return await _db.Produtos
            .FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras && p.Ativo);
    }

    public async Task<Produto?> BuscarPorCodigo(string codigoInterno)
    {
        return await _db.Produtos
            .FirstOrDefaultAsync(p => p.CodigoInterno == codigoInterno && p.Ativo);
    }

    public async Task<List<Produto>> Pesquisar(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return new List<Produto>();

        var termoLower = termo.ToLower();

        return await _db.Produtos
            .Where(p => p.Ativo && (
                p.Descricao.ToLower().Contains(termoLower) ||
                p.CodigoBarras.Contains(termo) ||
                p.CodigoInterno.Contains(termo)))
            .OrderBy(p => p.Descricao)
            .Take(50)
            .ToListAsync();
    }

    public async Task AtualizarCacheLocal(List<Produto> produtos)
    {
        foreach (var produto in produtos)
        {
            var existente = await _db.Produtos
                .FirstOrDefaultAsync(p => p.Id == produto.Id);

            if (existente != null)
            {
                // Atualiza produto existente
                existente.CodigoBarras = produto.CodigoBarras;
                existente.CodigoInterno = produto.CodigoInterno;
                existente.Descricao = produto.Descricao;
                existente.UnidadeMedida = produto.UnidadeMedida;
                existente.PrecoVenda = produto.PrecoVenda;
                existente.EstoqueAtual = produto.EstoqueAtual;
                existente.NCM = produto.NCM;
                existente.CFOP = produto.CFOP;
                existente.CST_ICMS = produto.CST_ICMS;
                existente.CST_PIS = produto.CST_PIS;
                existente.CST_COFINS = produto.CST_COFINS;
                existente.CSOSN = produto.CSOSN;
                existente.AliquotaICMS = produto.AliquotaICMS;
                existente.CEST = produto.CEST;
                existente.Ativo = produto.Ativo;
                existente.UltimaAtualizacao = produto.UltimaAtualizacao;
            }
            else
            {
                _db.Produtos.Add(produto);
            }
        }

        await _db.SaveChangesAsync();
    }
}
