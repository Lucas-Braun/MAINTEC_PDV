using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Enums;
using PDV.Core.Models;
using PDV.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows.Threading;

namespace PDV.App.ViewModels;

public partial class PDVViewModel : ObservableObject
{
    private readonly IProdutoService _produtoService;
    private readonly IApiClient _apiClient;
    private readonly ISessaoService _sessao;
    private readonly IImpressoraService _impressoraService;
    private readonly ITEFService _tefService;

    public PDVViewModel(
        IProdutoService produtoService,
        IApiClient apiClient,
        ISessaoService sessao,
        IImpressoraService impressoraService,
        ITEFService tefService)
    {
        _produtoService = produtoService;
        _apiClient = apiClient;
        _sessao = sessao;
        _impressoraService = impressoraService;
        _tefService = tefService;

        VendaAtual = new Venda();
        Itens = new ObservableCollection<ItemVenda>();

        // Numero do caixa da sessao
        NumeroCaixa = _sessao.CaixaCodigo?.ToString("D3") ?? "---";

        // Relogio ao vivo - atualiza a cada segundo
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => DataHoraAtual = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
        timer.Start();

        // Verifica status de conexoes ao iniciar
        _ = VerificarConexoes();
    }

    // =========================================
    // CALLBACKS DE NAVEGACAO
    // =========================================

    public Action<decimal>? SolicitarPagamento { get; set; }
    public Action? SolicitarConsulta { get; set; }
    public Action? SolicitarConsultaCliente { get; set; }
    public Action? SolicitarSangria { get; set; }
    public Action? SolicitarSuprimento { get; set; }
    public Action? SolicitarFechamento { get; set; }
    public Action? SolicitarConsultaVendas { get; set; }
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

    // Cliente associado a venda
    [ObservableProperty]
    private string _nomeCliente = string.Empty;

    [ObservableProperty]
    private string _cpfCliente = string.Empty;

    public bool ClienteDefinido => !string.IsNullOrEmpty(NomeCliente);

    // Numero do caixa (da sessao)
    [ObservableProperty]
    private string _numeroCaixa = "---";

    // Status de conexoes
    [ObservableProperty]
    private bool _apiConectada;

    [ObservableProperty]
    private string _statusApiTexto = "ERP: Verificando...";

    [ObservableProperty]
    private bool _tefConectado;

    [ObservableProperty]
    private string _statusTefTexto = "TEF: Verificando...";

    [ObservableProperty]
    private bool _nfceOnline;

    [ObservableProperty]
    private string _statusNfceTexto = "NFC-e: Verificando...";

    // Confirmacao de cancelamento
    [ObservableProperty]
    private bool _confirmandoCancelamento;

    // Desconto F8
    [ObservableProperty]
    private bool _descontoOverlayVisivel;

    [ObservableProperty]
    private string _descontoPercentualInput = string.Empty;

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
            var codigo = CodigoBarrasInput.Trim();

            var produto = await _produtoService.BuscarPorCodigoBarras(codigo);

            if (produto == null)
            {
                MensagemStatus = $"Produto nao encontrado: {codigo}";
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
            // Forca atualizacao visual removendo e reinserindo
            var index = Itens.IndexOf(itemExistente);
            Itens.RemoveAt(index);
            Itens.Insert(index, itemExistente);
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

        // Beep ao adicionar produto
        try { System.Media.SystemSounds.Beep.Play(); } catch { }

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

            // 2. Monta itens e parcelas para a API
            var itensApi = Itens.Select(i => new ItemVendaApi
            {
                ProInCodigo = i.ProdutoId,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario,
                DescontoPerc = i.DescontoPercentual > 0 ? i.DescontoPercentual : null
            }).ToList();

            var parcelasApi = pagamentos.Select(p => new ParcelaApi
            {
                FcbInCodigo = p.FcbInCodigo,
                Valor = p.Valor,
                Vencimento = DateTime.Now.ToString("yyyy-MM-dd")
            }).ToList();

            // Calcula troco
            decimal? troco = pagamentos
                .Where(p => p.FormaPagamento == FormaPagamento.Dinheiro && p.Troco > 0)
                .Sum(p => p.Troco);
            if (troco == 0) troco = null;

            var idempotencyKey = Guid.NewGuid().ToString();

            // 3. Finaliza venda na API (servidor faz NFC-e, preco, estoque)
            // Com retry automatico em caso de falha de rede
            MensagemStatus = "Finalizando venda...";
            ResultadoVenda resultado;
            try
            {
                resultado = await _apiClient.FinalizarVendaDireta(
                    itensApi, parcelasApi,
                    VendaAtual.ClienteCpfCnpj, troco, idempotencyKey);
            }
            catch (HttpRequestException)
            {
                MensagemStatus = "Falha na conexao. Tentando novamente...";
                await Task.Delay(2000);
                resultado = await _apiClient.FinalizarVendaDireta(
                    itensApi, parcelasApi,
                    VendaAtual.ClienteCpfCnpj, troco, idempotencyKey);
            }

            if (!resultado.Sucesso)
            {
                MensagemStatus = $"Erro na venda: {resultado.Erro}";
                return;
            }

            // 4. Popula venda local com resultado da API
            VendaAtual.Pagamentos = pagamentos;
            VendaAtual.Status = StatusVenda.Finalizada;
            VendaAtual.DataVenda = DateTime.Now;
            VendaAtual.NumeroVenda = resultado.PedidoCodigo?.ToString() ?? "";
            VendaAtual.ChaveNFCe = resultado.NfceChave;

            if (resultado.NfceStatus == "NFE_AUT")
            {
                VendaAtual.Status = StatusVenda.Finalizada;
            }

            // 5. Imprime cupom (nao deve impedir a venda)
            if (_sessao.Configuracao?.ImprimirCupom == true)
            {
                try
                {
                    MensagemStatus = "Imprimindo cupom...";
                    await _impressoraService.ImprimirCupom(VendaAtual);
                }
                catch
                {
                    // Impressora indisponivel - venda ja foi salva
                }
            }

            // 6. Navega para tela de comprovante
            MensagemStatus = $"Venda #{resultado.PedidoCodigo} finalizada! Total: {resultado.ValorVenda:C2}";
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
    private void CancelarVenda()
    {
        if (!VendaEmAndamento) return;

        if (!ConfirmandoCancelamento)
        {
            ConfirmandoCancelamento = true;
            MensagemStatus = "Pressione F9 novamente para confirmar o cancelamento";
            return;
        }

        ConfirmandoCancelamento = false;
        NovaVenda();
        MensagemStatus = "Venda cancelada";
    }

    [RelayCommand]
    private void DesistirCancelamento()
    {
        ConfirmandoCancelamento = false;
        MensagemStatus = "Cancelamento desfeito";
    }

    [RelayCommand]
    private void AbrirDesconto()
    {
        if (ItemSelecionado == null && !Itens.Any())
        {
            MensagemStatus = "Nenhum item para aplicar desconto";
            return;
        }
        DescontoPercentualInput = string.Empty;
        DescontoOverlayVisivel = true;
    }

    [RelayCommand]
    private void FecharDesconto()
    {
        DescontoOverlayVisivel = false;
    }

    [RelayCommand]
    private void AplicarDesconto()
    {
        if (!decimal.TryParse(DescontoPercentualInput?.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var perc) || perc <= 0 || perc > 100)
        {
            MensagemStatus = "Informe um percentual valido (0,01 a 100)";
            return;
        }

        if (ItemSelecionado != null)
        {
            // Desconto no item selecionado
            ItemSelecionado.DescontoPercentual = perc;
            ItemSelecionado.DescontoValor = Math.Round(ItemSelecionado.PrecoUnitario * ItemSelecionado.Quantidade * perc / 100m, 2);
            var index = Itens.IndexOf(ItemSelecionado);
            if (index >= 0)
            {
                var item = ItemSelecionado;
                Itens.RemoveAt(index);
                Itens.Insert(index, item);
            }
            MensagemStatus = $"Desconto de {perc:N2}% aplicado em {ItemSelecionado.DescricaoProduto}";
        }
        else
        {
            // Desconto em todos os itens
            for (int i = 0; i < Itens.Count; i++)
            {
                var item = Itens[i];
                item.DescontoPercentual = perc;
                item.DescontoValor = Math.Round(item.PrecoUnitario * item.Quantidade * perc / 100m, 2);
                Itens.RemoveAt(i);
                Itens.Insert(i, item);
            }
            MensagemStatus = $"Desconto de {perc:N2}% aplicado em todos os itens";
        }

        VendaAtual.Itens = Itens.ToList();
        AtualizarTotais();
        DescontoOverlayVisivel = false;
    }

    [RelayCommand]
    private void ConsultarProduto()
    {
        SolicitarConsulta?.Invoke();
    }

    [RelayCommand]
    private void ConsultarCliente()
    {
        SolicitarConsultaCliente?.Invoke();
    }

    public void DefinirCliente(Cliente? cliente)
    {
        if (cliente != null)
        {
            VendaAtual.ClienteId = cliente.Id;
            VendaAtual.ClienteCpfCnpj = cliente.CpfCnpj;
            NomeCliente = cliente.Nome;
            CpfCliente = cliente.CpfCnpj ?? string.Empty;
            MensagemStatus = $"Cliente: {cliente.Nome}";
        }
        else
        {
            VendaAtual.ClienteId = null;
            VendaAtual.ClienteCpfCnpj = null;
            NomeCliente = string.Empty;
            CpfCliente = string.Empty;
            MensagemStatus = "Cliente removido da venda";
        }
        OnPropertyChanged(nameof(ClienteDefinido));
    }

    [RelayCommand]
    private void ConsultarVendas()
    {
        SolicitarConsultaVendas?.Invoke();
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
        ConfirmandoCancelamento = false;
        CodigoBarrasInput = string.Empty;
        QuantidadeInput = 1;
        NomeCliente = string.Empty;
        CpfCliente = string.Empty;
        OnPropertyChanged(nameof(ClienteDefinido));
        AtualizarTotais();
    }

    private async Task VerificarConexoes()
    {
        // API / ERP
        try
        {
            var ok = await _apiClient.Ping();
            ApiConectada = ok;
            StatusApiTexto = ok ? "ERP: Sincronizado" : "ERP: Offline";
        }
        catch
        {
            ApiConectada = false;
            StatusApiTexto = "ERP: Offline";
        }

        // NFC-e (depende da API estar online + config)
        NfceOnline = ApiConectada && (_sessao.Configuracao?.EmitirNfceAuto == true);
        StatusNfceTexto = NfceOnline ? "NFC-e: Online" : "NFC-e: Offline";

        // TEF (stub por enquanto = sempre conectado)
        TefConectado = true;
        StatusTefTexto = "TEF: Conectado";
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
