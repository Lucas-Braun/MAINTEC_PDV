using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Enums;
using PDV.Core.Models;
using PDV.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace PDV.App.ViewModels;

public partial class PDVViewModel : ObservableObject
{
    private readonly IVendaService _vendaService;
    private readonly IProdutoService _produtoService;
    private readonly ICaixaService _caixaService;
    private readonly INFCeService _nfceService;
    private readonly ITEFService _tefService;
    private readonly IImpressoraService _impressoraService;
    private readonly IApiClient _apiClient;

    public PDVViewModel(
        IVendaService vendaService,
        IProdutoService produtoService,
        ICaixaService caixaService,
        INFCeService nfceService,
        ITEFService tefService,
        IImpressoraService impressoraService,
        IApiClient apiClient)
    {
        _vendaService = vendaService;
        _produtoService = produtoService;
        _caixaService = caixaService;
        _nfceService = nfceService;
        _tefService = tefService;
        _impressoraService = impressoraService;
        _apiClient = apiClient;

        VendaAtual = new Venda();
        Itens = new ObservableCollection<ItemVenda>();

        // Relogio ao vivo - atualiza a cada segundo
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => DataHoraAtual = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
        timer.Start();
    }

    // =========================================
    // CALLBACKS DE NAVEGACAO
    // =========================================

    public Action<decimal>? SolicitarPagamento { get; set; }
    public Action? SolicitarConsulta { get; set; }
    public Action? SolicitarSangria { get; set; }
    public Action? SolicitarSuprimento { get; set; }
    public Action? SolicitarFechamento { get; set; }
    public Action? SolicitarConfiguracoes { get; set; }
    public Action<Venda>? VendaFinalizada { get; set; }

    // =========================================
    // PROPRIEDADES OBSERVAVEIS
    // =========================================

    [ObservableProperty]
    private Venda _vendaAtual;

    [ObservableProperty]
    private ObservableCollection<ItemVenda> _itens;

    [ObservableProperty]
    private ItemVenda? _itemSelecionado;

    [ObservableProperty]
    private string _codigoBarrasInput = string.Empty;

    [ObservableProperty]
    private decimal _quantidadeInput = 1;

    [ObservableProperty]
    private string _mensagemStatus = "Caixa Aberto - Aguardando venda...";

    [ObservableProperty]
    private string _mensagemOperador = string.Empty;

    [ObservableProperty]
    private bool _vendaEmAndamento = false;

    [ObservableProperty]
    private bool _processando = false;

    [ObservableProperty]
    private string _dataHoraAtual = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");

    [ObservableProperty]
    private string _ultimoItemDescricao = string.Empty;

    [ObservableProperty]
    private string _ultimoItemPreco = string.Empty;

    [ObservableProperty]
    private string _ultimoItemQtd = string.Empty;

    // Totais exibidos na tela
    public decimal SubTotal => Itens.Sum(i => i.ValorTotal);
    public decimal DescontoTotal => VendaAtual.DescontoTotal;
    public decimal ValorTotal => SubTotal - DescontoTotal;
    public int TotalItens => Itens.Count;

    // =========================================
    // COMANDOS - OPERACOES DO CAIXA
    // =========================================

    [RelayCommand]
    private async Task AdicionarProduto()
    {
        if (string.IsNullOrWhiteSpace(CodigoBarrasInput)) return;

        try
        {
            Processando = true;

            var produto = await _produtoService.BuscarPorCodigoBarras(CodigoBarrasInput);

            if (produto == null)
            {
                MensagemStatus = $"Produto nao encontrado: {CodigoBarrasInput}";
                return;
            }

            if (!produto.Ativo)
            {
                MensagemStatus = $"Produto inativo: {produto.Descricao}";
                return;
            }

            AdicionarItemVenda(produto, QuantidadeInput);

            CodigoBarrasInput = string.Empty;
            QuantidadeInput = 1;
        }
        catch (Exception ex)
        {
            MensagemStatus = $"Erro ao adicionar produto: {ex.Message}";
        }
        finally
        {
            Processando = false;
        }
    }

    public void InserirProdutoConsultado(Produto produto)
    {
        AdicionarItemVenda(produto, 1);
    }

    private void AdicionarItemVenda(Produto produto, decimal quantidade)
    {
        var itemExistente = Itens.FirstOrDefault(i => i.ProdutoId == produto.Id);

        if (itemExistente != null)
        {
            itemExistente.Quantidade += quantidade;
            var index = Itens.IndexOf(itemExistente);
            Itens[index] = itemExistente;
        }
        else
        {
            var novoItem = new ItemVenda
            {
                ProdutoId = produto.Id,
                NumeroItem = Itens.Count + 1,
                CodigoBarras = produto.CodigoBarras,
                DescricaoProduto = produto.Descricao,
                UnidadeMedida = produto.UnidadeMedida,
                Quantidade = quantidade,
                PrecoUnitario = produto.PrecoVenda,
                NCM = produto.NCM,
                CFOP = produto.CFOP,
                CST_ICMS = produto.CST_ICMS,
                AliquotaICMS = produto.AliquotaICMS
            };

            Itens.Add(novoItem);
        }

        VendaAtual.Itens = Itens.ToList();
        VendaEmAndamento = true;

        MensagemStatus = $"{produto.Descricao} - {quantidade} x {produto.PrecoVenda:C2}";
        UltimoItemDescricao = produto.Descricao;
        UltimoItemPreco = (quantidade * produto.PrecoVenda).ToString("C2");
        UltimoItemQtd = $"{quantidade:N0} x {produto.PrecoVenda:C2}";
        AtualizarTotais();
    }

    [RelayCommand]
    private void RemoverItem()
    {
        if (ItemSelecionado == null) return;

        Itens.Remove(ItemSelecionado);

        for (int i = 0; i < Itens.Count; i++)
            Itens[i].NumeroItem = i + 1;

        VendaAtual.Itens = Itens.ToList();
        AtualizarTotais();

        if (Itens.Count == 0)
            VendaEmAndamento = false;

        MensagemStatus = "Item removido";
    }

    [RelayCommand]
    private void FinalizarVenda()
    {
        if (!Itens.Any())
        {
            MensagemStatus = "Nenhum item na venda";
            return;
        }

        SolicitarPagamento?.Invoke(ValorTotal);
    }

    public async Task ProcessarPagamento(List<Pagamento> pagamentos)
    {
        try
        {
            Processando = true;
            MensagemStatus = "Processando pagamento...";

            // 1. Processa TEF se houver cartao
            foreach (var pag in pagamentos.Where(p =>
                p.FormaPagamento == FormaPagamento.CartaoCredito ||
                p.FormaPagamento == FormaPagamento.CartaoDebito))
            {
                MensagemStatus = "Aguardando TEF...";
                var resultadoTef = await _tefService.ProcessarPagamento(
                    pag.Valor,
                    pag.FormaPagamento == FormaPagamento.CartaoCredito ? "credito" : "debito",
                    pag.Parcelas ?? 1
                );

                if (!resultadoTef.Aprovado)
                {
                    MensagemStatus = $"TEF recusado: {resultadoTef.Mensagem}";
                    return;
                }

                pag.NSU = resultadoTef.NSU;
                pag.CodigoAutorizacao = resultadoTef.CodigoAutorizacao;
                pag.BandeiraCartao = resultadoTef.Bandeira;
            }

            // 2. Salva venda localmente
            VendaAtual.Pagamentos = pagamentos;
            VendaAtual.Status = StatusVenda.Finalizada;
            VendaAtual.DataVenda = DateTime.Now;
            await _vendaService.SalvarVenda(VendaAtual);

            // 3. Emite NFC-e
            MensagemStatus = "Emitindo NFC-e...";
            var resultadoNfce = await _nfceService.EmitirNFCe(VendaAtual);

            if (resultadoNfce.Autorizada)
            {
                VendaAtual.ChaveNFCe = resultadoNfce.ChaveAcesso;
                VendaAtual.NumeroNFCe = resultadoNfce.NumeroNFCe;
                VendaAtual.ProtocoloAutorizacao = resultadoNfce.Protocolo;
            }
            else
            {
                VendaAtual.Status = StatusVenda.Contingencia;
                MensagemStatus = "NFC-e em contingencia - sera enviada depois";
            }

            // 4. Imprime cupom (nao deve impedir a venda)
            try
            {
                MensagemStatus = "Imprimindo cupom...";
                await _impressoraService.ImprimirCupom(VendaAtual);
            }
            catch
            {
                // Impressora indisponivel - venda ja foi salva
            }

            // 5. Sincroniza com ERP em background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _apiClient.EnviarVenda(VendaAtual);
                    VendaAtual.SincronizadoERP = true;
                    VendaAtual.DataSincronizacao = DateTime.Now;
                }
                catch
                {
                    // Sera sincronizado depois pelo SyncManager
                }
            });

            // 6. Navega para tela de comprovante
            MensagemStatus = $"Venda #{VendaAtual.NumeroVenda} finalizada! Total: {VendaAtual.ValorTotal:C2}";
            VendaFinalizada?.Invoke(VendaAtual);
        }
        catch (Exception ex)
        {
            MensagemStatus = $"Erro no pagamento: {ex.Message}";
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private async Task CancelarVenda()
    {
        if (!VendaEmAndamento) return;

        if (VendaAtual.ChaveNFCe != null)
        {
            await _nfceService.CancelarNFCe(VendaAtual.ChaveNFCe, "Cancelamento a pedido do cliente");
        }

        NovaVenda();
        MensagemStatus = "Venda cancelada";
    }

    [RelayCommand]
    private void ConsultarProduto()
    {
        SolicitarConsulta?.Invoke();
    }

    [RelayCommand]
    private void RealizarSangria()
    {
        SolicitarSangria?.Invoke();
    }

    [RelayCommand]
    private void RealizarSuprimento()
    {
        SolicitarSuprimento?.Invoke();
    }

    [RelayCommand]
    private void AbrirConfiguracoes()
    {
        SolicitarConfiguracoes?.Invoke();
    }

    [RelayCommand]
    private void FecharCaixa()
    {
        if (VendaEmAndamento)
        {
            MensagemStatus = "Finalize ou cancele a venda antes de fechar o caixa";
            return;
        }

        SolicitarFechamento?.Invoke();
    }

    // =========================================
    // METODOS AUXILIARES
    // =========================================

    public void NovaVenda()
    {
        VendaAtual = new Venda();
        Itens.Clear();
        VendaEmAndamento = false;
        CodigoBarrasInput = string.Empty;
        QuantidadeInput = 1;
        AtualizarTotais();
    }

    public void AtualizarAposEdicaoQuantidade()
    {
        VendaAtual.Itens = Itens.ToList();
        AtualizarTotais();
    }

    private void AtualizarTotais()
    {
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(DescontoTotal));
        OnPropertyChanged(nameof(ValorTotal));
        OnPropertyChanged(nameof(TotalItens));
    }
}
