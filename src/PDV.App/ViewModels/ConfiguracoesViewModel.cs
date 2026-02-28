using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.App.Themes;
using PDV.Infrastructure.Impressora;
using System.Reflection;

namespace PDV.App.ViewModels;

public partial class ConfiguracoesViewModel : ObservableObject
{
    private readonly ImpressoraConfig _impressoraConfig;

    public ConfiguracoesViewModel(ImpressoraConfig impressoraConfig)
    {
        _impressoraConfig = impressoraConfig;
        _temaAtual = ThemeManager.CurrentTheme;
    }

    // Callback de navegacao
    public Action? Voltar { get; set; }

    // Infos da empresa (read-only)
    public string NomeEmpresa => _impressoraConfig.NomeEmpresa;
    public string CnpjEmpresa => _impressoraConfig.CnpjEmpresa;
    public string EnderecoEmpresa => _impressoraConfig.EnderecoEmpresa;
    public string Impressora => $"{_impressoraConfig.TipoConexao} / {_impressoraConfig.Porta}";
    public string VersaoApp => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    [ObservableProperty]
    private string _temaAtual;

    public bool IsMorningHorizon
    {
        get => TemaAtual == "MorningHorizon";
        set
        {
            if (value) AplicarTema("MorningHorizon");
        }
    }

    public bool IsEveningHorizon
    {
        get => TemaAtual == "EveningHorizon";
        set
        {
            if (value) AplicarTema("EveningHorizon");
        }
    }

    private void AplicarTema(string tema)
    {
        ThemeManager.ApplyTheme(tema);
        TemaAtual = tema;
        OnPropertyChanged(nameof(IsMorningHorizon));
        OnPropertyChanged(nameof(IsEveningHorizon));
    }

    [RelayCommand]
    private void VoltarAoPDV()
    {
        Voltar?.Invoke();
    }
}
