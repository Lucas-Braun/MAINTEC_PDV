using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Interfaces;

namespace PDV.App.ViewModels;

public partial class AberturaCaixaViewModel : ObservableObject
{
    private readonly ICaixaService _caixaService;
    private readonly IOperadorService _operadorService;

    public AberturaCaixaViewModel(ICaixaService caixaService, IOperadorService operadorService)
    {
        _caixaService = caixaService;
        _operadorService = operadorService;
    }

    public Action? CaixaAberto { get; set; }

    [ObservableProperty]
    private int _numeroCaixa = 1;

    [ObservableProperty]
    private decimal _valorAbertura;

    [ObservableProperty]
    private string _mensagemErro = string.Empty;

    [ObservableProperty]
    private bool _processando = false;

    [RelayCommand]
    private async Task AbrirCaixa()
    {
        try
        {
            Processando = true;
            MensagemErro = string.Empty;

            var operador = _operadorService.OperadorLogado;
            if (operador == null)
            {
                MensagemErro = "Operador nao identificado";
                return;
            }

            await _caixaService.AbrirCaixa(operador.Id, NumeroCaixa, ValorAbertura);
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
