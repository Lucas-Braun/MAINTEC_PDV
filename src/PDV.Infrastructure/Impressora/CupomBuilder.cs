using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;

namespace PDV.Infrastructure.Impressora;

public class CupomBuilder
{
    private readonly List<byte[]> _commands = new();
    private readonly int _colunas;
    private readonly EPSON _e = new();

    public CupomBuilder(int colunas = 48)
    {
        _colunas = colunas;
        _commands.Add(_e.Initialize());
    }

    public void Centralizado() => _commands.Add(_e.CenterAlign());
    public void Esquerda() => _commands.Add(_e.LeftAlign());
    public void Direita() => _commands.Add(_e.RightAlign());
    public void NegritoOn() => _commands.Add(_e.SetStyles(PrintStyle.Bold));
    public void NegritoOff() => _commands.Add(_e.SetStyles(PrintStyle.None));

    public void FonteGrande() =>
        _commands.Add(_e.SetStyles(PrintStyle.Bold | PrintStyle.DoubleWidth | PrintStyle.DoubleHeight));

    public void FonteNormal() => _commands.Add(_e.SetStyles(PrintStyle.None));

    public void AdicionarLinha(string texto) => _commands.Add(_e.PrintLine(texto));

    public void AdicionarLinhaDireita(string texto)
    {
        var espacos = _colunas - texto.Length;
        if (espacos > 0)
            _commands.Add(_e.Print(new string(' ', espacos)));
        _commands.Add(_e.PrintLine(texto));
    }

    public void AdicionarCampo(string label, string valor)
    {
        var espacos = _colunas - label.Length - valor.Length;
        if (espacos < 1) espacos = 1;
        var linha = label + new string(' ', espacos) + valor;
        AdicionarLinha(linha);
    }

    public void LinhaTracejada() => AdicionarLinha(new string('-', _colunas));

    public void AdicionarBytes(byte[] dados) => _commands.Add(dados);

    public void Cortar() => _commands.Add(_e.PartialCutAfterFeed(3));

    public byte[] Build() => ByteSplicer.Combine(_commands.ToArray());
}
