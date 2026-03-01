using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.App.Themes;
using PDV.Core.Interfaces;
using PDV.Infrastructure.Impressora;
using PDV.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Reflection;
using System.Text;

namespace PDV.App.ViewModels;

public partial class ConfiguracoesViewModel : ObservableObject
{
    private readonly ImpressoraConfig _impressoraConfig;
    private readonly ConfiguracoesService _configService;
    private readonly IImpressoraService _impressoraService;
    private readonly ISessaoService _sessao;

    public ConfiguracoesViewModel(
        ImpressoraConfig impressoraConfig,
        ConfiguracoesService configService,
        IImpressoraService impressoraService,
        ISessaoService sessao)
    {
        _impressoraConfig = impressoraConfig;
        _configService = configService;
        _impressoraService = impressoraService;
        _sessao = sessao;

        _temaAtual = ThemeManager.CurrentTheme;

        // Carregar URL da API
        var config = _configService.Carregar();
        _apiUrl = config.ApiUrl;

        // Carregar valores atuais (converter tipo interno -> UI)
        _tipoConexao = _impressoraConfig.TipoConexao switch
        {
            "USB" or "Serial" => "USB/Serial",
            "Rede" => "Rede",
            "Windows" => "Windows Spooler",
            _ => "USB/Serial"
        };
        _portaSelecionada = _impressoraConfig.Porta;
        _ipImpressora = _impressoraConfig.IpImpressora ?? "";
        _portaRede = _impressoraConfig.PortaRede;
        _colunasMaximas = _impressoraConfig.ColunasMaximas;
        _impressoraSelecionada = _impressoraConfig.NomeSpooler ?? "";

        AtualizarPortas();
        AtualizarImpressoras();
    }

    // Callback de navegacao
    public Action? Voltar { get; set; }

    // Infos da empresa (da sessao)
    public string NomeEmpresa => _sessao.Empresa?.Nome ?? _impressoraConfig.NomeEmpresa;
    public string CnpjEmpresa => _sessao.Empresa?.Cnpj ?? _impressoraConfig.CnpjEmpresa;
    public string EnderecoEmpresa => _impressoraConfig.EnderecoEmpresa;
    public string VersaoApp => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    // ====== API ======

    [ObservableProperty]
    private string _apiUrl = string.Empty;

    // ====== TEMA ======

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

    // ====== IMPRESSORA ======

    public static string[] TiposConexao => ["USB/Serial", "Rede", "Windows Spooler"];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConexaoSerial))]
    [NotifyPropertyChangedFor(nameof(IsConexaoRede))]
    [NotifyPropertyChangedFor(nameof(IsConexaoSpooler))]
    private string _tipoConexao;

    public bool IsConexaoSerial => TipoConexao == "USB/Serial";
    public bool IsConexaoRede => TipoConexao == "Rede";
    public bool IsConexaoSpooler => TipoConexao == "Windows Spooler";

    public ObservableCollection<string> PortasDisponiveis { get; } = [];
    public ObservableCollection<string> ImpressorasWindows { get; } = [];

    [ObservableProperty]
    private string _portaSelecionada;

    [ObservableProperty]
    private string _impressoraSelecionada = "";

    [ObservableProperty]
    private string _ipImpressora;

    [ObservableProperty]
    private int _portaRede;

    [ObservableProperty]
    private int _colunasMaximas;

    public static int[] OpcoesColuna => [48, 40];

    // Resultado do teste
    [ObservableProperty]
    private string _resultadoTeste = "";

    [ObservableProperty]
    private bool _testeConectado;

    [ObservableProperty]
    private bool _testeVisivel;

    [RelayCommand]
    private void AtualizarPortas()
    {
        PortasDisponiveis.Clear();
        try
        {
            foreach (var porta in SerialPort.GetPortNames())
                PortasDisponiveis.Add(porta);
        }
        catch
        {
            // Sem portas disponiveis
        }

        if (PortasDisponiveis.Count > 0 && !PortasDisponiveis.Contains(PortaSelecionada))
            PortaSelecionada = PortasDisponiveis[0];

        AtualizarImpressoras();
    }

    private void AtualizarImpressoras()
    {
        ImpressorasWindows.Clear();
        foreach (var nome in ImpressoraService.ListarImpressorasWindows())
            ImpressorasWindows.Add(nome);

        if (ImpressorasWindows.Count > 0 && !ImpressorasWindows.Contains(ImpressoraSelecionada))
            ImpressoraSelecionada = ImpressorasWindows[0];
    }

    [RelayCommand]
    private void TestarConexao()
    {
        // Atualizar config temporariamente para testar
        AplicarConfigNoSingleton();

        var conectado = _impressoraService.VerificarConexao();
        TesteConectado = conectado;
        ResultadoTeste = conectado ? "Conectada" : "Sem conexao";
        TesteVisivel = true;
    }

    [RelayCommand]
    private async Task ImprimirTeste()
    {
        AplicarConfigNoSingleton();
        try
        {
            var texto =
                "^XA\r\n" +
                "^CF0,30\r\n" +
                "^FO50,50^FD================================^FS\r\n" +
                "^FO50,90^FD    TESTE DE IMPRESSAO         ^FS\r\n" +
                "^FO50,130^FD================================^FS\r\n" +
                $"^FO50,180^FD  {DateTime.Now:dd/MM/yyyy HH:mm:ss}^FS\r\n" +
                $"^FO50,220^FD  Tipo: {_impressoraConfig.TipoConexao}^FS\r\n" +
                "^FO50,260^FD================================^FS\r\n" +
                "^FO50,300^FD  Impressora funcionando!      ^FS\r\n" +
                "^FO50,340^FD================================^FS\r\n" +
                "^XZ\r\n";

            var dados = Encoding.UTF8.GetBytes(texto);
            await _impressoraService.EnviarRaw(dados);

            ResultadoTeste = "Teste impresso!";
            TesteConectado = true;
        }
        catch (Exception ex)
        {
            ResultadoTeste = $"Erro: {ex.Message}";
            TesteConectado = false;
        }
        TesteVisivel = true;
    }

    [RelayCommand]
    private void Salvar()
    {
        // Atualizar singleton em memoria
        AplicarConfigNoSingleton();

        // Persistir no JSON
        var config = _configService.Carregar();
        config.Tema = TemaAtual;
        config.TipoConexao = TipoConexao;
        config.Porta = PortaSelecionada;
        config.IpImpressora = IpImpressora;
        config.PortaRede = PortaRede;
        config.ColunasMaximas = ColunasMaximas;
        config.NomeSpooler = ImpressoraSelecionada;
        config.ApiUrl = ApiUrl;
        _configService.Salvar(config);

        ResultadoTeste = "Salvo!";
        TesteConectado = true;
        TesteVisivel = true;
    }

    private void AplicarConfigNoSingleton()
    {
        switch (TipoConexao)
        {
            case "USB/Serial":
                _impressoraConfig.TipoConexao = "USB";
                _impressoraConfig.Porta = PortaSelecionada;
                break;
            case "Rede":
                _impressoraConfig.TipoConexao = "Rede";
                _impressoraConfig.IpImpressora = IpImpressora;
                _impressoraConfig.PortaRede = PortaRede;
                break;
            case "Windows Spooler":
                _impressoraConfig.TipoConexao = "Windows";
                _impressoraConfig.NomeSpooler = ImpressoraSelecionada;
                break;
        }
        _impressoraConfig.ColunasMaximas = ColunasMaximas;
    }

    [RelayCommand]
    private void VoltarAoPDV()
    {
        Voltar?.Invoke();
    }
}
