namespace PDV.Core.Interfaces;

public interface ITEFService
{
    Task<ResultadoTEF> ProcessarPagamento(decimal valor, string tipo, int parcelas = 1);
    Task<bool> CancelarTransacao(string nsu);
    Task<bool> VerificarConexao();
}

public class ResultadoTEF
{
    public bool Aprovado { get; set; }
    public string NSU { get; set; } = string.Empty;
    public string CodigoAutorizacao { get; set; } = string.Empty;
    public string Bandeira { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string? ComprovanteLoja { get; set; }
    public string? ComprovanteCliente { get; set; }
}
