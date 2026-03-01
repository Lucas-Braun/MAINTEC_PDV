using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using System.Collections.ObjectModel;

namespace PDV.App.ViewModels;

public partial class ConsultaVendasViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly IImpressoraService _impressoraService;
    private int _limiteAtual = 50;

    public ConsultaVendasViewModel(IApiClient apiClient, IImpressoraService impressoraService)
    {
        _apiClient = apiClient;
        _impressoraService = impressoraService;
        Vendas = new ObservableCollection<VendaResumo>();
        ItensDetalhe = new ObservableCollection<ItemVendaDetalhe>();

        DataInicio = DateTime.Today;
        DataFim = DateTime.Today;

        StatusOpcoes = new List<KeyValuePair<string, string>>
        {
            new("", "Todos"),
            new("NFE_AUT", "Autorizada"),
            new("NFE_REJ", "Rejeitada"),
            new("NFE_CAN", "Cancelada")
        };

        StatusFiltro = "";
    }

    // Callbacks
    public Action? Cancelado { get; set; }

    // Estorno
    [ObservableProperty]
    private bool _confirmandoEstorno;

    [ObservableProperty]
    private string _motivoEstorno = string.Empty;

    [ObservableProperty]
    private string _mensagemSucesso = string.Empty;

    public async Task CarregarInicial()
    {
        await Pesquisar();
    }

    // Filtros
    [ObservableProperty]
    private DateTime _dataInicio;

    [ObservableProperty]
    private DateTime _dataFim;

    [ObservableProperty]
    private string _statusFiltro = "";

    [ObservableProperty]
    private string _filtroNf = "";

    public List<KeyValuePair<string, string>> StatusOpcoes { get; }

    // Resultados
    [ObservableProperty]
    private ObservableCollection<VendaResumo> _vendas;

    [ObservableProperty]
    private VendaResumo? _vendaSelecionada;

    [ObservableProperty]
    private ObservableCollection<ItemVendaDetalhe> _itensDetalhe;

    [ObservableProperty]
    private VendaDetalhe? _detalheAtual;

    // Estado
    [ObservableProperty]
    private bool _processando;

    [ObservableProperty]
    private string _mensagemErro = string.Empty;

    [ObservableProperty]
    private int _totalRegistros;

    partial void OnVendaSelecionadaChanged(VendaResumo? value)
    {
        if (value != null)
        {
            _ = CarregarDetalhe(value.NfInCodigo);
        }
        else
        {
            DetalheAtual = null;
            ItensDetalhe.Clear();
        }
    }

    [RelayCommand]
    private async Task Pesquisar()
    {
        try
        {
            Processando = true;
            MensagemErro = string.Empty;
            MensagemSucesso = string.Empty;
            DetalheAtual = null;
            ItensDetalhe.Clear();
            _limiteAtual = 50;

            var status = string.IsNullOrEmpty(StatusFiltro) ? null : StatusFiltro;
            var nf = string.IsNullOrWhiteSpace(FiltroNf) ? null : FiltroNf.Trim();
            var vendas = await _apiClient.ListarVendas(DataInicio, DataFim, status, nf, _limiteAtual);

            Vendas.Clear();
            foreach (var v in vendas)
                Vendas.Add(v);

            TotalRegistros = Vendas.Count;

            if (Vendas.Count == 0)
                MensagemErro = "Nenhuma venda encontrada para os filtros informados";
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao consultar vendas: {ex.Message}";
        }
        finally
        {
            Processando = false;
        }
    }

    private async Task CarregarDetalhe(int nfInCodigo)
    {
        try
        {
            Processando = true;
            var detalhe = await _apiClient.ObterVendaDetalhe(nfInCodigo);

            if (detalhe != null)
            {
                DetalheAtual = detalhe;
                ItensDetalhe.Clear();
                foreach (var item in detalhe.Itens)
                    ItensDetalhe.Add(item);
            }
            else
            {
                DetalheAtual = null;
                ItensDetalhe.Clear();
                MensagemErro = "Nao foi possivel carregar detalhes da venda";
            }
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao carregar detalhe: {ex.Message}";
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private async Task Reimprimir()
    {
        if (DetalheAtual == null || VendaSelecionada == null)
        {
            MensagemErro = "Selecione uma venda para reimprimir";
            return;
        }

        try
        {
            Processando = true;
            MensagemErro = string.Empty;

            // Monta Venda minima para impressao
            var venda = new Venda
            {
                NumeroVenda = VendaSelecionada.NfNumero.ToString(),
                DataVenda = DateTime.TryParse(VendaSelecionada.Data, out var dt) ? dt : DateTime.Now,
                ChaveNFCe = DetalheAtual.NfceChave,
                ClienteCpfCnpj = DetalheAtual.CpfNota,
                Itens = DetalheAtual.Itens.Select(i => new ItemVenda
                {
                    NumeroItem = i.Sequencia,
                    CodigoBarras = i.Codigo,
                    DescricaoProduto = i.Descricao,
                    UnidadeMedida = i.Unidade,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario
                }).ToList()
            };

            await _impressoraService.ImprimirCupom(venda);
            MensagemSucesso = "Cupom enviado para impressao";
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao reimprimir: {ex.Message}";
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private async Task CarregarMais()
    {
        _limiteAtual += 50;
        try
        {
            Processando = true;
            MensagemErro = string.Empty;

            var status = string.IsNullOrEmpty(StatusFiltro) ? null : StatusFiltro;
            var nf = string.IsNullOrWhiteSpace(FiltroNf) ? null : FiltroNf.Trim();
            var vendas = await _apiClient.ListarVendas(DataInicio, DataFim, status, nf, _limiteAtual);

            Vendas.Clear();
            foreach (var v in vendas)
                Vendas.Add(v);

            TotalRegistros = Vendas.Count;
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro: {ex.Message}";
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private void IniciarEstorno()
    {
        if (VendaSelecionada == null) return;
        if (VendaSelecionada.Status != "NFE_AUT")
        {
            MensagemErro = "Somente vendas autorizadas podem ser estornadas";
            return;
        }
        MotivoEstorno = string.Empty;
        ConfirmandoEstorno = true;
    }

    [RelayCommand]
    private void CancelarEstorno()
    {
        ConfirmandoEstorno = false;
        MotivoEstorno = string.Empty;
    }

    [RelayCommand]
    private async Task ConfirmarEstorno()
    {
        if (VendaSelecionada == null) return;

        if (string.IsNullOrWhiteSpace(MotivoEstorno))
        {
            MensagemErro = "Informe o motivo do estorno";
            return;
        }

        try
        {
            Processando = true;
            ConfirmandoEstorno = false;
            MensagemErro = string.Empty;

            var resultado = await _apiClient.EstornarVenda(VendaSelecionada.NfInCodigo, MotivoEstorno.Trim());

            if (resultado.Sucesso)
            {
                MensagemSucesso = resultado.Mensagem ?? "Venda estornada com sucesso";
                await Pesquisar(); // Recarrega lista
            }
            else
            {
                MensagemErro = resultado.Erro ?? "Erro ao estornar venda";
            }
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao estornar: {ex.Message}";
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private void Voltar()
    {
        Cancelado?.Invoke();
    }
}
