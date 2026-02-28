using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using PDV.Core.Enums;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace PDV.Infrastructure.Impressora;

public class ImpressoraService : IImpressoraService
{
    private readonly ImpressoraConfig _config;

    public ImpressoraService(ImpressoraConfig config)
    {
        _config = config;
    }

    public async Task ImprimirCupom(Venda venda)
    {
        var builder = new CupomBuilder(_config.ColunasMaximas);

        // Cabecalho da empresa
        builder.Centralizado();
        builder.NegritoOn();
        builder.AdicionarLinha(_config.NomeEmpresa);
        builder.NegritoOff();
        builder.AdicionarLinha($"CNPJ: {_config.CnpjEmpresa}");
        builder.AdicionarLinha(_config.EnderecoEmpresa);
        builder.LinhaTracejada();

        // Dados da NFC-e
        builder.Centralizado();
        builder.AdicionarLinha("DANFE NFC-e - Documento Auxiliar");
        builder.AdicionarLinha("da Nota Fiscal Eletronica p/ Consumidor");
        builder.LinhaTracejada();

        // Itens
        builder.Esquerda();
        builder.AdicionarLinha("COD   DESCRICAO");
        builder.AdicionarLinha("QTD    UN   VL.UNIT   VL.TOTAL");
        builder.LinhaTracejada();

        foreach (var item in venda.Itens)
        {
            builder.AdicionarLinha($"{item.CodigoBarras}  {TruncateString(item.DescricaoProduto, 24)}");
            builder.AdicionarLinhaDireita(
                $"{item.Quantidade:N2}  {item.UnidadeMedida}  {item.PrecoUnitario:N2}  {item.ValorTotal:N2}");
        }

        builder.LinhaTracejada();

        // Totais
        builder.AdicionarCampo("SUBTOTAL", venda.SubTotal.ToString("N2"));
        if (venda.DescontoTotal > 0)
            builder.AdicionarCampo("DESCONTO", $"-{venda.DescontoTotal:N2}");
        builder.NegritoOn();
        builder.FonteGrande();
        builder.AdicionarCampo("TOTAL", venda.ValorTotal.ToString("N2"));
        builder.FonteNormal();
        builder.NegritoOff();

        builder.LinhaTracejada();

        // Pagamentos
        builder.AdicionarLinha("FORMA DE PAGAMENTO");
        foreach (var pag in venda.Pagamentos)
        {
            var nome = pag.FormaPagamento switch
            {
                FormaPagamento.Dinheiro => "Dinheiro",
                FormaPagamento.CartaoCredito => "Cartao Credito",
                FormaPagamento.CartaoDebito => "Cartao Debito",
                FormaPagamento.PIX => "PIX",
                _ => "Outros"
            };
            builder.AdicionarCampo(nome, pag.Valor.ToString("N2"));

            if (pag.Troco.HasValue && pag.Troco.Value > 0)
                builder.AdicionarCampo("TROCO", pag.Troco.Value.ToString("N2"));
        }

        builder.LinhaTracejada();

        // Dados fiscais
        builder.Centralizado();
        builder.AdicionarLinha($"NFC-e n {venda.NumeroNFCe}");
        builder.AdicionarLinha($"Chave: {venda.ChaveNFCe}");
        builder.AdicionarLinha($"Protocolo: {venda.ProtocoloAutorizacao}");

        if (!string.IsNullOrEmpty(venda.ClienteCpfCnpj))
            builder.AdicionarLinha($"CPF/CNPJ: {venda.ClienteCpfCnpj}");

        builder.LinhaTracejada();
        builder.AdicionarLinha(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

        builder.Cortar();

        await EnviarParaImpressora(builder.Build());
    }

    public async Task ImprimirComprovanteTEF(string comprovante)
    {
        var e = new EPSON();
        var dados = ByteSplicer.Combine(
            e.Initialize(),
            e.Print(comprovante),
            e.PartialCutAfterFeed(3)
        );
        await EnviarParaImpressora(dados);
    }

    public async Task ImprimirFechamentoCaixa(Caixa caixa)
    {
        var builder = new CupomBuilder(_config.ColunasMaximas);

        builder.Centralizado();
        builder.NegritoOn();
        builder.AdicionarLinha("FECHAMENTO DE CAIXA");
        builder.NegritoOff();
        builder.LinhaTracejada();

        builder.Esquerda();
        builder.AdicionarCampo("Caixa", caixa.NumeroCaixa.ToString());
        builder.AdicionarCampo("Operador", caixa.NomeOperador);
        builder.AdicionarCampo("Abertura", caixa.DataAbertura.ToString("dd/MM/yyyy HH:mm"));
        builder.AdicionarCampo("Fechamento", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        builder.LinhaTracejada();

        builder.AdicionarCampo("Valor Abertura", caixa.ValorAbertura.ToString("N2"));
        builder.AdicionarCampo("Total Vendas", caixa.TotalVendas.ToString("N2"));
        builder.AdicionarCampo("Dinheiro", caixa.TotalDinheiro.ToString("N2"));
        builder.AdicionarCampo("Cartao Credito", caixa.TotalCartaoCredito.ToString("N2"));
        builder.AdicionarCampo("Cartao Debito", caixa.TotalCartaoDebito.ToString("N2"));
        builder.AdicionarCampo("PIX", caixa.TotalPix.ToString("N2"));
        builder.AdicionarCampo("Sangrias", $"-{caixa.TotalSangria:N2}");
        builder.AdicionarCampo("Suprimentos", caixa.TotalSuprimento.ToString("N2"));
        builder.AdicionarCampo("Cancelamentos", $"-{caixa.TotalCancelamentos:N2}");
        builder.LinhaTracejada();

        builder.NegritoOn();
        builder.AdicionarCampo("SALDO ESPERADO", caixa.SaldoEsperado.ToString("N2"));
        builder.AdicionarCampo("VALOR CONTADO", caixa.ValorFechamento?.ToString("N2") ?? "0,00");
        builder.AdicionarCampo("DIFERENCA", caixa.Diferenca?.ToString("N2") ?? "0,00");
        builder.NegritoOff();

        builder.Cortar();
        await EnviarParaImpressora(builder.Build());
    }

    public async Task AbrirGaveta()
    {
        var e = new EPSON();
        await EnviarParaImpressora(e.CashDrawerOpenPin2());
    }

    public async Task CortarPapel()
    {
        var e = new EPSON();
        await EnviarParaImpressora(e.PartialCut());
    }

    public async Task EnviarRaw(byte[] dados)
    {
        switch (_config.TipoConexao)
        {
            case "USB":
            case "Serial":
                await EnviarSerial(dados);
                break;
            case "Rede":
                await EnviarRede(dados);
                break;
            case "Windows":
                await EnviarSpooler(dados);
                break;
            default:
                throw new Exception($"Tipo de conexao desconhecido: {_config.TipoConexao}");
        }
    }

    public bool VerificarConexao()
    {
        try
        {
            if (_config.TipoConexao is "USB" or "Serial")
            {
                using var port = new SerialPort(_config.Porta);
                port.Open();
                port.Close();
                return true;
            }
            if (_config.TipoConexao is "Windows")
            {
                return !string.IsNullOrEmpty(_config.NomeSpooler)
                    && RawPrinterHelper.PrinterExists(_config.NomeSpooler);
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task EnviarParaImpressora(byte[] dados)
    {
        try
        {
            switch (_config.TipoConexao)
            {
                case "USB":
                case "Serial":
                    await EnviarSerial(dados);
                    break;
                case "Rede":
                    await EnviarRede(dados);
                    break;
                case "Windows":
                    await EnviarSpooler(dados);
                    break;
            }
        }
        catch (Exception)
        {
            // Impressora indisponivel - ignora silenciosamente
        }
    }

    private async Task EnviarSerial(byte[] dados)
    {
        using var port = new SerialPort(_config.Porta, _config.BaudRate);
        port.Open();
        await port.BaseStream.WriteAsync(dados, 0, dados.Length);
        port.Close();
    }

    private async Task EnviarRede(byte[] dados)
    {
        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync(_config.IpImpressora!, _config.PortaRede);
        var stream = client.GetStream();
        await stream.WriteAsync(dados, 0, dados.Length);
        stream.Close();
    }

    private async Task EnviarSpooler(byte[] dados)
    {
        await Task.Run(() => RawPrinterHelper.SendBytesToPrinter(_config.NomeSpooler!, dados));
    }

    public static string[] ListarImpressorasWindows()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine
                .OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Print\Printers");
            if (key == null) return [];
            return key.GetSubKeyNames().OrderBy(n => n).ToArray();
        }
        catch
        {
            return [];
        }
    }

    private static string TruncateString(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}

internal static class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DOCINFOW
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string pDocName;
        [MarshalAs(UnmanagedType.LPWStr)] public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPWStr)] public string pDataType;
    }

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DOCINFOW di);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    public static bool PrinterExists(string printerName)
    {
        if (!OpenPrinter(printerName, out var hPrinter, IntPtr.Zero))
            return false;
        ClosePrinter(hPrinter);
        return true;
    }

    public static void SendBytesToPrinter(string printerName, byte[] dados)
    {
        if (!OpenPrinter(printerName, out var hPrinter, IntPtr.Zero))
            throw new Exception($"Nao foi possivel abrir impressora: {printerName}");

        var di = new DOCINFOW { pDocName = "PDV Cupom", pDataType = "RAW" };
        try
        {
            StartDocPrinter(hPrinter, 1, ref di);
            StartPagePrinter(hPrinter);

            var pBytes = Marshal.AllocCoTaskMem(dados.Length);
            try
            {
                Marshal.Copy(dados, 0, pBytes, dados.Length);
                WritePrinter(hPrinter, pBytes, dados.Length, out _);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pBytes);
            }

            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }
}
