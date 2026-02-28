using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Enums;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using System.Collections.ObjectModel;

namespace PDV.App.ViewModels;

public partial class SangriaSuprimentoViewModel : ObservableObject
{
    private readonly ICaixaService _caixaService;

    public SangriaSuprimentoViewModel(ICaixaService caixaService)
    {
        _caixaService = caixaService;
    }

    // Callbacks
    public Action? Confirmado { get; set; }
    public Action? Cancelado { get; set; }

    [ObservableProperty]
    private TipoMovimentoCaixa _tipo = TipoMovimentoCaixa.Sangria;

    [ObservableProperty]
    private decimal _valor;

    [ObservableProperty]
    private string _observacao = string.Empty;

    [ObservableProperty]
    private string _mensagemErro = string.Empty;

    [ObservableProperty]
    private bool _processando = false;

    [ObservableProperty]
    private decimal _saldoCaixa;

    [ObservableProperty]
    private ObservableCollection<MovimentoCaixa> _movimentos = new();

    public bool TemMovimentos => Movimentos.Count > 0;

    // Titulo dinamico baseado no tipo
    public string Titulo => Tipo == TipoMovimentoCaixa.Sangria ? "SANGRIA" : "SUPRIMENTO";
    public string Descricao => Tipo == TipoMovimentoCaixa.Sangria
        ? "Retirada de valores do caixa"
        : "Entrada de valores no caixa";

    public void CarregarDadosCaixa(Caixa caixa)
    {
        SaldoCaixa = caixa.SaldoEsperado;
        var movs = caixa.Movimentos
            .Where(m => m.Tipo == TipoMovimentoCaixa.Sangria || m.Tipo == TipoMovimentoCaixa.Suprimento)
            .OrderByDescending(m => m.DataHora)
            .Take(10);
        Movimentos = new ObservableCollection<MovimentoCaixa>(movs);
        OnPropertyChanged(nameof(TemMovimentos));
    }

    partial void OnTipoChanged(TipoMovimentoCaixa value)
    {
        OnPropertyChanged(nameof(Titulo));
        OnPropertyChanged(nameof(Descricao));
    }

    [RelayCommand]
    private void DefinirValor(string valorStr)
    {
        if (decimal.TryParse(valorStr, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var v))
            Valor = v;
    }

    [RelayCommand]
    private async Task Confirmar()
    {
        if (Valor <= 0)
        {
            MensagemErro = "Informe um valor maior que zero";
            return;
        }

        try
        {
            Processando = true;
            MensagemErro = string.Empty;

            if (Tipo == TipoMovimentoCaixa.Sangria)
                await _caixaService.RegistrarSangria(Valor, Observacao);
            else
                await _caixaService.RegistrarSuprimento(Valor, Observacao);

            Confirmado?.Invoke();
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
    private void Voltar()
    {
        Cancelado?.Invoke();
    }
}
