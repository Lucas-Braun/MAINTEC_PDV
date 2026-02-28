using System.Text;

namespace PDV.Infrastructure.Impressora;

public class CupomBuilder
{
    private readonly List<byte> _buffer = new();
    private readonly int _colunas;
    private readonly Encoding _encoding;

    public CupomBuilder(int colunas = 48)
    {
        _colunas = colunas;
        _encoding = Encoding.GetEncoding(850); // Code page para acentos

        // Inicializa impressora
        _buffer.AddRange(new byte[] { 0x1B, 0x40 }); // ESC @
        // Seleciona code page 850
        _buffer.AddRange(new byte[] { 0x1B, 0x74, 0x02 });
    }

    public void Centralizado() => _buffer.AddRange(new byte[] { 0x1B, 0x61, 0x01 });
    public void Esquerda() => _buffer.AddRange(new byte[] { 0x1B, 0x61, 0x00 });
    public void Direita() => _buffer.AddRange(new byte[] { 0x1B, 0x61, 0x02 });
    public void NegritoOn() => _buffer.AddRange(new byte[] { 0x1B, 0x45, 0x01 });
    public void NegritoOff() => _buffer.AddRange(new byte[] { 0x1B, 0x45, 0x00 });
    public void FonteGrande() => _buffer.AddRange(new byte[] { 0x1D, 0x21, 0x11 });
    public void FonteNormal() => _buffer.AddRange(new byte[] { 0x1D, 0x21, 0x00 });

    public void AdicionarLinha(string texto)
    {
        _buffer.AddRange(_encoding.GetBytes(texto));
        _buffer.Add(0x0A); // Line feed
    }

    public void AdicionarLinhaDireita(string texto)
    {
        var espacos = _colunas - texto.Length;
        if (espacos > 0)
            _buffer.AddRange(_encoding.GetBytes(new string(' ', espacos)));
        _buffer.AddRange(_encoding.GetBytes(texto));
        _buffer.Add(0x0A);
    }

    public void AdicionarCampo(string label, string valor)
    {
        var espacos = _colunas - label.Length - valor.Length;
        if (espacos < 1) espacos = 1;
        var linha = label + new string(' ', espacos) + valor;
        AdicionarLinha(linha);
    }

    public void LinhaTracejada()
    {
        AdicionarLinha(new string('-', _colunas));
    }

    public void AdicionarBytes(byte[] dados) => _buffer.AddRange(dados);

    public void Cortar()
    {
        _buffer.Add(0x0A);
        _buffer.Add(0x0A);
        _buffer.Add(0x0A);
        _buffer.AddRange(new byte[] { 0x1D, 0x56, 0x01 }); // Corte parcial
    }

    public byte[] Build() => _buffer.ToArray();
}
