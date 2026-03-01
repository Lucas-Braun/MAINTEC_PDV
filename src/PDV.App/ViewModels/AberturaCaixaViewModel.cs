using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using System.Collections.ObjectModel;

namespace PDV.App.ViewModels;

public partial class AberturaCaixaViewModel : ObservableObject
{
    private readonly ICaixaService _caixaService;
    private readonly ISessaoService _sessao;

    public AberturaCaixaViewModel(ICaixaService caixaService, ISessaoService sessao)
    {
        _caixaService = caixaService;
        _sessao = sessao;

        // Carrega configuracao de terminais
        CarregarTerminais();
    }

    public Action? CaixaAberto { get; set; }

    [ObservableProperty]
    private decimal _valorAbertura;

    [ObservableProperty]
    private string _mensagemErro = string.Empty;

    [ObservableProperty]
    private bool _processando = false;

    [ObservableProperty]
    private bool _usarTerminalFixo;

    [ObservableProperty]
    private ObservableCollection<TerminalInfo> _terminais = new();

    [ObservableProperty]
    private TerminalInfo? _terminalSelecionado;

    [ObservableProperty]
    private string _nomeTerminalFixo = string.Empty;

    private void CarregarTerminais()
    {
        var config = _sessao.ConfigTerminal;
        if (config == null) return;

        UsarTerminalFixo = config.UsarTerminalFixo;

        if (config.UsarTerminalFixo && config.TerminalOperador != null)
        {
            NomeTerminalFixo = config.TerminalOperador.Nome;
            TerminalSelecionado = config.TerminalOperador;
        }
        else
        {
            Terminais = new ObservableCollection<TerminalInfo>(config.Terminais);
            if (Terminais.Count > 0)
                TerminalSelecionado = Terminais[0];
        }
    }

    [RelayCommand]
    private async Task AbrirCaixa()
    {
        try
        {
            Processando = true;
            MensagemErro = string.Empty;

            int? terInCodigo = null;
            if (!UsarTerminalFixo)
            {
                if (TerminalSelecionado == null)
                {
                    MensagemErro = "Selecione um terminal";
                    return;
                }
                terInCodigo = TerminalSelecionado.TerInCodigo;
            }

            var resultado = await _caixaService.AbrirCaixa(ValorAbertura, terInCodigo);

            if (!resultado.Sucesso)
            {
                MensagemErro = resultado.Erro ?? "Erro ao abrir caixa";
                return;
            }

            CaixaAberto?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao abrir caixa: {ex.Message}";
        }
        finally
        {
            Processando = false;
        }
    }
}
