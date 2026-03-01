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
    private readonly ICaixaService _caixaService;

    public PDVViewModel(
        IProdutoService produtoService,
        IApiClient apiClient,
        ISessaoService sessao,
        IImpressoraService impressoraService,
        ITEFService tefService,
        ICaixaService caixaService)
    {
        _produtoService = produtoService;
        _apiClient = apiClient;
        _sessao = sessao;
        _impressoraService = impressoraService;
        _tefService = tefService;
        _caixaService = caixaService;

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
    public Action? SolicitarLogout { get; set; }

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

    [ObservableProperty]
    private string? _ultimoItemFotoUrl;

    // Ultimo produto adicionado (para F1 repetir)
    private Produto? _ultimoProduto;

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

    // CPF na nota
    [ObservableProperty]
    private string _cpfNotaInput = string.Empty;

    // Timeout visual
    [ObservableProperty]
    private string _tempoProcessamento = string.Empty;

    private DispatcherTimer? _timerProcessamento;
    private DateTime _inicioProcessamento;

    partial void OnProcessandoChanged(bool value)
    {
        if (value)
        {
            _inicioProcessamento = DateTime.Now;
            TempoProcessamento = "0s";
            _timerProcessamento ??= new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timerProcessamento.Tick += TimerProcessamento_Tick;
            _timerProcessamento.Start();
        }
        else
        {
            _timerProcessamento?.Stop();
            if (_timerProcessamento != null)
                _timerProcessamento.Tick -= TimerProcessamento_Tick;
            TempoProcessamento = string.Empty;
        }
    }

    private void TimerProcessamento_Tick(object? sender, EventArgs e)
    {
        var elapsed = DateTime.Now - _inicioProcessamento;
        TempoProcessamento = elapsed.TotalSeconds < 60
            ? $"{elapsed.Seconds}s"
            : $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s";
    }

    // Toast notifications
    [ObservableProperty]
    private bool _toastVisivel;

    [ObservableProperty]
    private string _toastMensagem = string.Empty;

    [ObservableProperty]
    private string _toastTipo = "info";

    private DispatcherTimer? _timerToast;

    public void MostrarToast(string mensagem, string tipo = "info")
    {
        ToastMensagem = mensagem;
        ToastTipo = tipo;
        ToastVisivel = true;

        if (_timerToast == null)
        {
            _timerToast = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
            _timerToast.Tick += (_, _) => { ToastVisivel = false; _timerToast.Stop(); };
        }
        _timerToast.Stop();
        _timerToast.Start();
    }

    // Help overlay
    [ObservableProperty]
    private bool _ajudaVisivel;

    // Leitura X
    [ObservableProperty]
    private bool _leituraXVisivel;

    [ObservableProperty] private decimal _lxTotalVendas;
    [ObservableProperty] private decimal _lxDinheiro;
    [ObservableProperty] private decimal _lxCredito;
    [ObservableProperty] private decimal _lxDebito;
    [ObservableProperty] private decimal _lxPix;
    [ObservableProperty] private decimal _lxSangrias;
    [ObservableProperty] private decimal _lxSuprimentos;
    [ObservableProperty] private decimal _lxSaldo;
    [ObservableProperty] private decimal _lxAbertura;
    [ObservableProperty] private decimal _lxEstornos;

    // Produtos recentes (favoritos)
    [ObservableProperty]
    private ObservableCollection<Produto> _produtosRecentes = new();

    // Teclado numerico virtual
    [ObservableProperty]
    private bool _tecladoVisivel;

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
            var qtd = QuantidadeInput;

            // Suporta atalho "3*codigo" para quantidade
            if (codigo.Contains('*'))
            {
                var partes = codigo.Split('*', 2);
                if (decimal.TryParse(partes[0], out var qtdParsed) && qtdParsed > 0)
                {
                    qtd = qtdParsed;
                    codigo = partes[1].Trim();
                }
            }

            var produto = await _produtoService.BuscarPorCodigoBarras(codigo);

            // Fallback: busca por nome se nao achou por codigo
            if (produto == null && codigo.Length >= 3 && !codigo.All(char.IsDigit))
            {
                var resultados = await _apiClient.PesquisarProdutos(codigo, 1);
                if (resultados.Count > 0)
                    produto = resultados[0];
            }

            if (produto == null)
            {
                MensagemStatus = $"Produto nao encontrado: {codigo}";
                MostrarToast($"Produto nao encontrado: {codigo}", "erro");
                return;
            }

            if (!produto.Ativo)
            {
                MensagemStatus = $"Produto inativo: {produto.Descricao}";
                return;
            }

            AdicionarItemVenda(produto, qtd);

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

        _ultimoProduto = produto;

        // Track recentes (max 6, unique)
        var existente = ProdutosRecentes.FirstOrDefault(p => p.Id == produto.Id);
        if (existente != null) ProdutosRecentes.Remove(existente);
        ProdutosRecentes.Insert(0, produto);
        if (ProdutosRecentes.Count > 6) ProdutosRecentes.RemoveAt(ProdutosRecentes.Count - 1);

        MensagemStatus = $"{produto.Descricao} - {quantidade} x {produto.PrecoVenda:C2}";
        UltimoItemDescricao = produto.Descricao;
        UltimoItemPreco = (quantidade * produto.PrecoVenda).ToString("C2");
        UltimoItemQtd = $"{quantidade:N0} x {produto.PrecoVenda:C2}";
        UltimoItemFotoUrl = produto.FotoUrl;
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
        MostrarToast("Item removido", "info");
    }

    [RelayCommand]
    private void FinalizarVenda()
    {
        if (!Itens.Any())
        {
            MensagemStatus = "Nenhum item na venda";
            return;
        }

        // Se tem CPF na nota informado, associa a venda
        if (!string.IsNullOrWhiteSpace(CpfNotaInput) && string.IsNullOrEmpty(VendaAtual.ClienteCpfCnpj))
        {
            var digitos = Core.Helpers.CpfCnpjHelper.ApenasDigitos(CpfNotaInput);
            if (digitos.Length > 0 && Core.Helpers.CpfCnpjHelper.Validar(digitos))
            {
                VendaAtual.ClienteCpfCnpj = digitos;
            }
            else if (digitos.Length > 0)
            {
                MensagemStatus = "CPF/CNPJ na nota invalido";
                return;
            }
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

            // 6. Abre gaveta se pagamento em dinheiro
            if (pagamentos.Any(p => p.FormaPagamento == FormaPagamento.Dinheiro))
            {
                try { await _impressoraService.AbrirGaveta(); } catch { }
            }

            // 7. Navega para tela de comprovante
            MensagemStatus = $"Venda #{resultado.PedidoCodigo} finalizada! Total: {resultado.ValorVenda:C2}";
            MostrarToast($"Venda #{resultado.PedidoCodigo} finalizada!", "sucesso");
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
        MostrarToast("Venda cancelada", "info");
    }

    [RelayCommand]
    private void DesistirCancelamento()
    {
        ConfirmandoCancelamento = false;
        MensagemStatus = "Cancelamento desfeito";
    }

    [RelayCommand]
    private void RepetirUltimoItem()
    {
        if (_ultimoProduto == null)
        {
            MensagemStatus = "Nenhum item para repetir";
            return;
        }
        AdicionarItemVenda(_ultimoProduto, 1);
    }

    [RelayCommand]
    private void MostrarAjuda()
    {
        AjudaVisivel = !AjudaVisivel;
    }

    [RelayCommand]
    private void FecharAjuda()
    {
        AjudaVisivel = false;
    }

    // Leitura X
    [RelayCommand]
    private async Task AbrirLeituraX()
    {
        try
        {
            Processando = true;
            MensagemStatus = "Carregando leitura X...";
            var resumo = await _caixaService.ObterResumoCaixa();

            if (resumo.Sucesso)
            {
                LxTotalVendas = resumo.TotalVendas;
                LxDinheiro = resumo.TotalDinheiro;
                LxCredito = resumo.TotalCartaoCredito;
                LxDebito = resumo.TotalCartaoDebito;
                LxPix = resumo.TotalPix;
                LxSangrias = resumo.TotalSangrias;
                LxSuprimentos = resumo.TotalSuprimentos;
                LxSaldo = resumo.SaldoAtual;
                LxAbertura = resumo.ValorAbertura;
                LxEstornos = resumo.TotalEstornos;
                LeituraXVisivel = true;
                MensagemStatus = "Leitura X carregada";
            }
        }
        catch (Exception ex)
        {
            MensagemStatus = $"Erro ao carregar leitura X: {ex.Message}";
            MostrarToast("Erro ao carregar leitura X", "erro");
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private async Task ImprimirLeituraX()
    {
        try
        {
            var caixa = new Caixa
            {
                NumeroCaixa = _sessao.CaixaCodigo ?? 0,
                NomeOperador = _sessao.Usuario?.Nome ?? "Operador",
                DataAbertura = DateTime.Today,
                ValorAbertura = LxAbertura,
                TotalVendas = LxTotalVendas,
                TotalDinheiro = LxDinheiro,
                TotalCartaoCredito = LxCredito,
                TotalCartaoDebito = LxDebito,
                TotalPix = LxPix,
                TotalSangria = LxSangrias,
                TotalSuprimento = LxSuprimentos,
                TotalCancelamentos = LxEstornos,
            };
            await _impressoraService.ImprimirFechamentoCaixa(caixa);
            MostrarToast("Leitura X impressa", "sucesso");
        }
        catch
        {
            MostrarToast("Erro ao imprimir", "erro");
        }
    }

    [RelayCommand]
    private void FecharLeituraX()
    {
        LeituraXVisivel = false;
    }

    // Produtos recentes - adicionar ao clicar
    [RelayCommand]
    private void AdicionarProdutoRecente(Produto? produto)
    {
        if (produto == null) return;
        AdicionarItemVenda(produto, 1);
    }

    // Teclado numerico virtual
    [RelayCommand]
    private void ToggleTeclado()
    {
        TecladoVisivel = !TecladoVisivel;
    }

    [RelayCommand]
    private void Tecla(string? tecla)
    {
        if (tecla == null) return;

        switch (tecla)
        {
            case "C":
                CodigoBarrasInput = string.Empty;
                break;
            case "BS":
                if (CodigoBarrasInput.Length > 0)
                    CodigoBarrasInput = CodigoBarrasInput[..^1];
                break;
            case "OK":
                _ = AdicionarProduto();
                break;
            default:
                CodigoBarrasInput += tecla;
                break;
        }
    }

    [RelayCommand]
    private void IncrementarQuantidade()
    {
        if (ItemSelecionado == null) return;
        ItemSelecionado.Quantidade += 1;
        var index = Itens.IndexOf(ItemSelecionado);
        if (index >= 0)
        {
            var item = ItemSelecionado;
            Itens.RemoveAt(index);
            Itens.Insert(index, item);
            ItemSelecionado = item;
        }
        VendaAtual.Itens = Itens.ToList();
        AtualizarTotais();
        MensagemStatus = $"{ItemSelecionado.DescricaoProduto} - Qtd: {ItemSelecionado.Quantidade:N2}";
    }

    [RelayCommand]
    private void DecrementarQuantidade()
    {
        if (ItemSelecionado == null) return;
        if (ItemSelecionado.Quantidade <= 1)
        {
            MensagemStatus = "Use DEL para remover o item";
            return;
        }
        ItemSelecionado.Quantidade -= 1;
        var index = Itens.IndexOf(ItemSelecionado);
        if (index >= 0)
        {
            var item = ItemSelecionado;
            Itens.RemoveAt(index);
            Itens.Insert(index, item);
            ItemSelecionado = item;
        }
        VendaAtual.Itens = Itens.ToList();
        AtualizarTotais();
        MensagemStatus = $"{ItemSelecionado.DescricaoProduto} - Qtd: {ItemSelecionado.Quantidade:N2}";
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
            MostrarToast($"Desconto de {perc:N2}% aplicado", "sucesso");
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
            MostrarToast($"Desconto de {perc:N2}% aplicado em todos", "sucesso");
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

    [RelayCommand]
    private void TrocarOperador()
    {
        if (VendaEmAndamento)
        {
            MensagemStatus = "Finalize ou cancele a venda antes de trocar operador";
            return;
        }

        SolicitarLogout?.Invoke();
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
        CpfNotaInput = string.Empty;
        UltimoItemDescricao = string.Empty;
        UltimoItemPreco = string.Empty;
        UltimoItemQtd = string.Empty;
        UltimoItemFotoUrl = null;
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
