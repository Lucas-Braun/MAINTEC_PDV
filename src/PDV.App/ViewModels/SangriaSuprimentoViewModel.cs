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
    private readonly IApiClient _apiClient;

    public SangriaSuprimentoViewModel(ICaixaService caixaService, IApiClient apiClient)
    {
        _caixaService = caixaService;
        _apiClient = apiClient;
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
    private ObservableCollection<MovimentoExibicao> _movimentos = new();

    public bool TemMovimentos => Movimentos.Count > 0;

    // Titulo dinamico baseado no tipo
    public string Titulo => Tipo == TipoMovimentoCaixa.Sangria ? "SANGRIA" : "SUPRIMENTO";
    public string Descricao => Tipo == TipoMovimentoCaixa.Sangria
        ? "Retirada de valores do caixa"
        : "Entrada de valores no caixa";

    partial void OnTipoChanged(TipoMovimentoCaixa value)
    {
        OnPropertyChanged(nameof(Titulo));
        OnPropertyChanged(nameof(Descricao));
    }

    public async Task CarregarDados()
    {
        try
        {
            var resumo = await _caixaService.ObterResumoCaixa();
            if (resumo.Sucesso)
                SaldoCaixa = resumo.SaldoAtual;

            var movimentos = await _apiClient.ObterMovimentosCaixa(10);
            Movimentos = new ObservableCollection<MovimentoExibicao>(
                movimentos
                    .Where(m => m.Tipo == TipoMovimentoCaixa.Sangria || m.Tipo == TipoMovimentoCaixa.Suprimento)
                    .Select(m => new MovimentoExibicao
                    {
                        DataHora = m.DataHora,
                        Tipo = m.Tipo == TipoMovimentoCaixa.Sangria ? "Sangria" : "Suprimento",
                        Valor = m.Valor
                    }));
            OnPropertyChanged(nameof(TemMovimentos));
        }
        catch { /* silencioso - dados opcionais */ }
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

            ResultadoOperacao resultado;
            if (Tipo == TipoMovimentoCaixa.Sangria)
                resultado = await _caixaService.RegistrarSangria(Valor, Observacao);
            else
                resultado = await _caixaService.RegistrarSuprimento(Valor, Observacao);

            if (!resultado.Sucesso)
            {
                MensagemErro = resultado.Erro ?? "Erro ao registrar operacao";
                return;
            }

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

public class MovimentoExibicao
{
    public DateTime DataHora { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
