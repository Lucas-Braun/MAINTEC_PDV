using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Interfaces;
using PDV.Core.Models;

namespace PDV.App.ViewModels;

public partial class FechamentoCaixaViewModel : ObservableObject
{
    private readonly ICaixaService _caixaService;
    private readonly IImpressoraService _impressoraService;

    public FechamentoCaixaViewModel(ICaixaService caixaService, IImpressoraService impressoraService)
    {
        _caixaService = caixaService;
        _impressoraService = impressoraService;
    }

    // Callbacks
    public Action? CaixaFechado { get; set; }
    public Action? Cancelado { get; set; }

    [ObservableProperty]
    private Caixa? _caixaAtual;

    [ObservableProperty]
    private decimal _valorFechamento;

    [ObservableProperty]
    private string _mensagemStatus = string.Empty;

    [ObservableProperty]
    private bool _processando = false;

    // Propriedades somente-leitura do caixa
    public decimal TotalVendas => CaixaAtual?.TotalVendas ?? 0;
    public decimal TotalDinheiro => CaixaAtual?.TotalDinheiro ?? 0;
    public decimal TotalCartaoCredito => CaixaAtual?.TotalCartaoCredito ?? 0;
    public decimal TotalCartaoDebito => CaixaAtual?.TotalCartaoDebito ?? 0;
    public decimal TotalPix => CaixaAtual?.TotalPix ?? 0;
    public decimal TotalSangria => CaixaAtual?.TotalSangria ?? 0;
    public decimal TotalSuprimento => CaixaAtual?.TotalSuprimento ?? 0;
    public decimal ValorAbertura => CaixaAtual?.ValorAbertura ?? 0;
    public decimal SaldoEsperado => CaixaAtual?.SaldoEsperado ?? 0;
    public decimal Diferenca => ValorFechamento - SaldoEsperado;

    partial void OnCaixaAtualChanged(Caixa? value)
    {
        AtualizarTotais();
    }

    partial void OnValorFechamentoChanged(decimal value)
    {
        OnPropertyChanged(nameof(Diferenca));
    }

    public async Task CarregarCaixaAberto(int operadorId)
    {
        try
        {
            Processando = true;
            CaixaAtual = await _caixaService.ObterCaixaAberto(operadorId);
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private async Task FecharCaixa()
    {
        try
        {
            Processando = true;
            MensagemStatus = "Fechando caixa...";

            var caixaFechado = await _caixaService.FecharCaixa(ValorFechamento);

            MensagemStatus = "Imprimindo relatorio...";
            await _impressoraService.ImprimirFechamentoCaixa(caixaFechado);

            MensagemStatus = "Caixa fechado com sucesso!";
            CaixaFechado?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemStatus = $"Erro ao fechar caixa: {ex.Message}";
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

    private void AtualizarTotais()
    {
        OnPropertyChanged(nameof(TotalVendas));
        OnPropertyChanged(nameof(TotalDinheiro));
        OnPropertyChanged(nameof(TotalCartaoCredito));
        OnPropertyChanged(nameof(TotalCartaoDebito));
        OnPropertyChanged(nameof(TotalPix));
        OnPropertyChanged(nameof(TotalSangria));
        OnPropertyChanged(nameof(TotalSuprimento));
        OnPropertyChanged(nameof(ValorAbertura));
        OnPropertyChanged(nameof(SaldoEsperado));
        OnPropertyChanged(nameof(Diferenca));
    }
}
