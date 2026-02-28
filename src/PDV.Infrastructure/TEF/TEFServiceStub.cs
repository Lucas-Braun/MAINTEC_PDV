using PDV.Core.Interfaces;

namespace PDV.Infrastructure.TEF;

/// <summary>
/// Stub do servico TEF para desenvolvimento.
/// TODO: Substituir por implementacao real com SiTef/PayGo.
/// </summary>
public class TEFServiceStub : ITEFService
{
    public async Task<ResultadoTEF> ProcessarPagamento(decimal valor, string tipo, int parcelas = 1)
    {
        await Task.Delay(500); // Simula processamento

        return new ResultadoTEF
        {
            Aprovado = true,
            NSU = Random.Shared.Next(100000, 999999).ToString(),
            CodigoAutorizacao = Random.Shared.Next(100000, 999999).ToString(),
            Bandeira = tipo == "credito" ? "VISA" : "MASTERCARD",
            Mensagem = "APROVADA",
            ComprovanteLoja = $"VIA LOJA - {tipo.ToUpper()} - R$ {valor:N2}",
            ComprovanteCliente = $"VIA CLIENTE - {tipo.ToUpper()} - R$ {valor:N2}"
        };
    }

    public async Task<bool> CancelarTransacao(string nsu)
    {
        await Task.Delay(200);
        return true;
    }

    public async Task<bool> VerificarConexao()
    {
        return true;
    }
}
