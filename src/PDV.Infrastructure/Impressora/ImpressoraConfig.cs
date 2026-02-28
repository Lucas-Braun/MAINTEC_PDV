namespace PDV.Infrastructure.Impressora;

public class ImpressoraConfig
{
    public string TipoConexao { get; set; } = "USB";      // USB, Serial, Rede, Windows
    public string Porta { get; set; } = "COM3";
    public int BaudRate { get; set; } = 9600;
    public string? IpImpressora { get; set; }
    public int PortaRede { get; set; } = 9100;
    public string? NomeSpooler { get; set; }               // Nome no Windows
    public int ColunasMaximas { get; set; } = 48;          // 48 ou 40 cols
    public string NomeEmpresa { get; set; } = string.Empty;
    public string CnpjEmpresa { get; set; } = string.Empty;
    public string EnderecoEmpresa { get; set; } = string.Empty;
}
