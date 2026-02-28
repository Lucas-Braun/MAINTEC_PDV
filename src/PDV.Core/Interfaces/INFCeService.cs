using PDV.Core.Models;

namespace PDV.Core.Interfaces;

public interface INFCeService
{
    Task<ResultadoNFCe> EmitirNFCe(Venda venda);
    Task<bool> CancelarNFCe(string chaveAcesso, string justificativa);
    Task<bool> InutilizarNumeracao(int serieNFCe, int numeroInicial, int numeroFinal, string justificativa);
    Task<List<Venda>> ReenviarContingencia();
}

public class ResultadoNFCe
{
    public bool Autorizada { get; set; }
    public string ChaveAcesso { get; set; } = string.Empty;
    public int NumeroNFCe { get; set; }
    public string Protocolo { get; set; } = string.Empty;
    public string? MotivoRejeicao { get; set; }
    public string XmlAutorizado { get; set; } = string.Empty;
}
