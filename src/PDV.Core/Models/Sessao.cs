namespace PDV.Core.Models;

public class UsuarioSessao
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class EmpresaSessao
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
}

public class FilialSessao
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public int OrgId { get; set; }
    public string? OrgNome { get; set; }
}

public class ConfiguracaoPdv
{
    public bool EmitirNfceAuto { get; set; } = true;
    public bool ExigirCpf { get; set; }
    public int CasasDecimaisQtd { get; set; } = 3;
    public int CasasDecimaisPreco { get; set; } = 2;
    public bool ExigirAbertura { get; set; } = true;
    public bool ImprimirCupom { get; set; } = true;
    public bool UsarTerminalFixo { get; set; }
    public string ModoEntrada { get; set; } = "A";
    public bool UsarTurno { get; set; }
    public int AvisoFimTurno { get; set; } = 15;
    public int LimiteHorasAberto { get; set; } = 12;
}

public class FormaPagamentoSessao
{
    public int FcbInCodigo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Padrao { get; set; }
    public bool PermiteTroco { get; set; }
}

public class ConfigTerminal
{
    public bool UsarTerminalFixo { get; set; }
    public List<TerminalInfo> Terminais { get; set; } = new();
    public TerminalInfo? TerminalOperador { get; set; }
}

public class TerminalInfo
{
    public int TerInCodigo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? SetorNome { get; set; }
}
