using PDV.Core.Interfaces;
using PDV.Core.Models;

namespace PDV.Infrastructure.Fiscal;

/// <summary>
/// Stub do servico NFC-e para desenvolvimento.
/// TODO: Substituir por implementacao real com ACBrLib.
/// </summary>
public class NFCeServiceStub : INFCeService
{
    public async Task<ResultadoNFCe> EmitirNFCe(Venda venda)
    {
        await Task.Delay(100);

        // Simula emissao autorizada
        return new ResultadoNFCe
        {
            Autorizada = true,
            ChaveAcesso = $"NFCe{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(100000, 999999)}",
            NumeroNFCe = Random.Shared.Next(1, 99999),
            Protocolo = $"PROT{Random.Shared.Next(100000000, 999999999)}",
            XmlAutorizado = "<nfeProc>...</nfeProc>"
        };
    }

    public async Task<bool> CancelarNFCe(string chaveAcesso, string justificativa)
    {
        await Task.Delay(100);
        return true;
    }

    public async Task<bool> InutilizarNumeracao(int serieNFCe, int numeroInicial, int numeroFinal, string justificativa)
    {
        await Task.Delay(100);
        return true;
    }

    public async Task<List<Venda>> ReenviarContingencia()
    {
        await Task.Delay(100);
        return new List<Venda>();
    }
}
