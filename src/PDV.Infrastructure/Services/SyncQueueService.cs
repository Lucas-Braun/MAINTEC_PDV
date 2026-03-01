using Microsoft.Extensions.DependencyInjection;
using PDV.Core.Interfaces;
using PDV.Core.Models;

namespace PDV.Infrastructure.Services;

public class SyncQueueService : ISyncQueueService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PdvLogger _logger;
    private Timer? _timer;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private int _vendasPendentes;
    private bool _disposed;

    public int VendasPendentes => _vendasPendentes;
    public event Action<int>? PendentesAlterados;

    public SyncQueueService(IServiceProvider serviceProvider, PdvLogger logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Iniciar()
    {
        _timer = new Timer(async _ => await ExecutarCiclo(), null,
            TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2));
        _logger.Info("SyncQueueService iniciado (ciclo 2min)");
    }

    public void Parar()
    {
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        _logger.Info("SyncQueueService parado");
    }

    public async Task<int> SincronizarAgora()
    {
        return await ExecutarCiclo();
    }

    private async Task<int> ExecutarCiclo()
    {
        if (!await _semaphore.WaitAsync(0)) return 0;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var vendaService = scope.ServiceProvider.GetRequiredService<IVendaService>();
            var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();

            var pendentes = await vendaService.ObterVendasNaoSincronizadas();
            AtualizarContador(pendentes.Count);

            if (pendentes.Count == 0) return 0;

            int sincronizadas = 0;

            foreach (var venda in pendentes)
            {
                try
                {
                    var itensApi = venda.Itens.Select(i => new ItemVendaApi
                    {
                        ProInCodigo = i.ProdutoId,
                        Quantidade = i.Quantidade,
                        PrecoUnitario = i.PrecoUnitario,
                        DescontoPerc = i.DescontoPercentual > 0 ? i.DescontoPercentual : null
                    }).ToList();

                    var parcelasApi = venda.Pagamentos.Select(p => new ParcelaApi
                    {
                        FcbInCodigo = p.FcbInCodigo,
                        Valor = p.Valor,
                        Vencimento = venda.DataVenda.ToString("yyyy-MM-dd")
                    }).ToList();

                    var chave = venda.ChaveIdempotencia ?? Guid.NewGuid().ToString();

                    var resultado = await apiClient.FinalizarVendaDireta(
                        itensApi, parcelasApi,
                        venda.ClienteCpfCnpj, null, chave);

                    if (resultado.Sucesso || resultado.FromCache)
                    {
                        // Atualiza dados vindos do ERP
                        if (resultado.PedidoCodigo.HasValue)
                            venda.NumeroVenda = resultado.PedidoCodigo.Value.ToString();
                        if (!string.IsNullOrEmpty(resultado.NfceChave))
                            venda.ChaveNFCe = resultado.NfceChave;

                        venda.SincronizadoERP = true;
                        venda.DataSincronizacao = DateTime.Now;
                        await vendaService.SalvarVenda(venda);
                        await vendaService.MarcarComoSincronizada(venda.Id);

                        sincronizadas++;
                        _logger.Operacao("SYNC", "VENDA_SINCRONIZADA",
                            $"VendaId={venda.Id} Pedido={resultado.PedidoCodigo} FromCache={resultado.FromCache}");
                    }
                    else
                    {
                        _logger.Erro($"Sync falhou para venda {venda.Id}: {resultado.Erro}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    // API caiu de novo — para o loop
                    _logger.Erro($"Sync interrompido (API offline): {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Erro($"Erro ao sincronizar venda {venda.Id}", ex);
                }
            }

            // Atualiza contador final
            var restantes = await vendaService.ObterVendasNaoSincronizadas();
            AtualizarContador(restantes.Count);

            if (sincronizadas > 0)
                _logger.Info($"Sync: {sincronizadas} venda(s) sincronizada(s)");

            return sincronizadas;
        }
        catch (Exception ex)
        {
            _logger.Erro("Erro no ciclo de sincronizacao", ex);
            return 0;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void AtualizarContador(int count)
    {
        if (_vendasPendentes != count)
        {
            _vendasPendentes = count;
            PendentesAlterados?.Invoke(count);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer?.Dispose();
        _semaphore.Dispose();
    }
}
