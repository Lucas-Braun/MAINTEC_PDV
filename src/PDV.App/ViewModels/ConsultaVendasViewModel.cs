using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using System.Collections.ObjectModel;

namespace PDV.App.ViewModels;

public partial class ConsultaVendasViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    public ConsultaVendasViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
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
            DetalheAtual = null;
            ItensDetalhe.Clear();

            var status = string.IsNullOrEmpty(StatusFiltro) ? null : StatusFiltro;
            var vendas = await _apiClient.ListarVendas(DataInicio, DataFim, status);

            Vendas.Clear();
            foreach (var v in vendas)
            {
                // Filtro local por NF se informado
                if (!string.IsNullOrWhiteSpace(FiltroNf))
                {
                    if (!v.NfNumero.ToString().Contains(FiltroNf.Trim()))
                        continue;
                }
                Vendas.Add(v);
            }

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
    private void Voltar()
    {
        Cancelado?.Invoke();
    }
}
