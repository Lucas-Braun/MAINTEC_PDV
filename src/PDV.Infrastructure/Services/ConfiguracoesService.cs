using System.Text.Json;

namespace PDV.Infrastructure.Services;

public class ConfiguracoesApp
{
    public string Tema { get; set; } = "EveningHorizon";

    // Impressora
    public string TipoConexao { get; set; } = "USB";
    public string Porta { get; set; } = "COM3";
    public int BaudRate { get; set; } = 9600;
    public string? IpImpressora { get; set; }
    public int PortaRede { get; set; } = 9100;
    public string? NomeSpooler { get; set; }
    public int ColunasMaximas { get; set; } = 48;

    // Empresa
    public string NomeEmpresa { get; set; } = "SUA EMPRESA LTDA";
    public string CnpjEmpresa { get; set; } = "00.000.000/0001-00";
    public string EnderecoEmpresa { get; set; } = "Rua Exemplo, 123 - Cidade/UF";

    // API
    public string ApiUrl { get; set; } = "https://meintec.com.br";
}

public class ConfiguracoesService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _caminhoArquivo;

    public ConfiguracoesService()
    {
        var pasta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PDV");
        Directory.CreateDirectory(pasta);
        _caminhoArquivo = Path.Combine(pasta, "configuracoes.json");
    }

    public ConfiguracoesApp Carregar()
    {
        if (!File.Exists(_caminhoArquivo))
            return new ConfiguracoesApp();

        try
        {
            var json = File.ReadAllText(_caminhoArquivo);
            return JsonSerializer.Deserialize<ConfiguracoesApp>(json, _jsonOptions)
                   ?? new ConfiguracoesApp();
        }
        catch
        {
            return new ConfiguracoesApp();
        }
    }

    public void Salvar(ConfiguracoesApp config)
    {
        var json = JsonSerializer.Serialize(config, _jsonOptions);
        File.WriteAllText(_caminhoArquivo, json);
    }
}
