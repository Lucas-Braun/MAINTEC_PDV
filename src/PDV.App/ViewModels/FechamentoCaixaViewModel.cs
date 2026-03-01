using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Interfaces;
using PDV.Core.Models;

namespace PDV.App.ViewModels;

public partial class FechamentoCaixaViewModel : ObservableObject
{
    private readonly ICaixaService _caixaService;
    private readonly IImpressoraService _impressoraService;
    private readonly ISessaoService _sessao;

    public FechamentoCaixaViewModel(ICaixaService caixaService, IImpressoraService impressoraService, ISessaoService sessao)
    {
        _caixaService = caixaService;
        _impressoraService = impressoraService;
        _sessao = sessao;
    }

    // Callbacks
    public Action? CaixaFechado { get; set; }
    public Action? Cancelado { get; set; }

    [ObservableProperty]
    private decimal _valorFechamento;

    [ObservableProperty]
    private string _mensagemStatus = string.Empty;

    [ObservableProperty]
    private bool _processando = false;

    // Totais vindos do resumo da API
    [ObservableProperty]
    private decimal _totalVendas;

    [ObservableProperty]
    private decimal _totalSangrias;

    [ObservableProperty]
    private decimal _totalSuprimentos;

    [ObservableProperty]
    private decimal _totalEstornos;

    [ObservableProperty]
    private decimal _valorAbertura;

    // Por forma de pagamento
    [ObservableProperty]
    private decimal _totalDinheiro;

    [ObservableProperty]
    private decimal _totalCartaoCredito;

    [ObservableProperty]
    private decimal _totalCartaoDebito;

    [ObservableProperty]
    private decimal _totalPix;

    // Alias singular para bindings da View
    public decimal TotalSangria => TotalSangrias;
    public decimal TotalSuprimento => TotalSuprimentos;

    [ObservableProperty]
    private decimal _saldoEsperado;

    public decimal Diferenca => ValorFechamento - SaldoEsperado;

    partial void OnValorFechamentoChanged(decimal value)
    {
        OnPropertyChanged(nameof(Diferenca));
    }

    public async Task CarregarResumoCaixa()
    {
        try
        {
            Processando = true;
            var resumo = await _caixaService.ObterResumoCaixa();

            if (resumo.Sucesso)
            {
                TotalVendas = resumo.TotalVendas;
                TotalSangrias = resumo.TotalSangrias;
                TotalSuprimentos = resumo.TotalSuprimentos;
                TotalEstornos = resumo.TotalEstornos;
                ValorAbertura = resumo.ValorAbertura;
                SaldoEsperado = resumo.SaldoAtual;

                TotalDinheiro = resumo.TotalDinheiro;
                TotalCartaoCredito = resumo.TotalCartaoCredito;
                TotalCartaoDebito = resumo.TotalCartaoDebito;
                TotalPix = resumo.TotalPix;

                OnPropertyChanged(nameof(Diferenca));
                OnPropertyChanged(nameof(TotalSangria));
                OnPropertyChanged(nameof(TotalSuprimento));
            }
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

            var resultado = await _caixaService.FecharCaixa(ValorFechamento);

            if (!resultado.Sucesso)
            {
                MensagemStatus = resultado.Erro ?? "Erro ao fechar caixa";
                return;
            }

            // Imprime relatorio de fechamento na termica
            try
            {
                var caixa = new Caixa
                {
                    NumeroCaixa = _sessao.CaixaCodigo ?? 0,
                    NomeOperador = _sessao.Usuario?.Nome ?? "Operador",
                    DataAbertura = DateTime.Today,
                    ValorAbertura = ValorAbertura,
                    TotalVendas = TotalVendas,
                    TotalDinheiro = TotalDinheiro,
                    TotalCartaoCredito = TotalCartaoCredito,
                    TotalCartaoDebito = TotalCartaoDebito,
                    TotalPix = TotalPix,
                    TotalSangria = TotalSangrias,
                    TotalSuprimento = TotalSuprimentos,
                    TotalCancelamentos = TotalEstornos,
                    ValorFechamento = ValorFechamento
                };
                MensagemStatus = "Imprimindo relatorio...";
                await _impressoraService.ImprimirFechamentoCaixa(caixa);
            }
            catch
            {
                // Impressora indisponivel - nao impede o fechamento
            }

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
}
