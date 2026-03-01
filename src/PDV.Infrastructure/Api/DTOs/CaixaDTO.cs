using System.Text.Json.Serialization;

namespace PDV.Infrastructure.Api.DTOs;

public class CaixaStatusResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("caixa_aberto")]
    public bool CaixaAberto { get; set; }

    [JsonPropertyName("caixa")]
    public CaixaApiDTO? Caixa { get; set; }
}

public class CaixaApiDTO
{
    [JsonPropertyName("cai_in_codigo")]
    public int CaiInCodigo { get; set; }

    [JsonPropertyName("cai_dt_abertura")]
    public string? CaiDtAbertura { get; set; }

    [JsonPropertyName("cai_re_vl_abertura")]
    public decimal? CaiReVlAbertura { get; set; }
}

public class AbrirCaixaRequest
{
    [JsonPropertyName("valor_abertura")]
    public decimal ValorAbertura { get; set; }

    [JsonPropertyName("ter_in_codigo")]
    public int? TerInCodigo { get; set; }
}

public class AbrirCaixaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("caixa")]
    public CaixaApiDTO? Caixa { get; set; }
}

public class FecharCaixaRequest
{
    [JsonPropertyName("valor_fechamento")]
    public decimal ValorFechamento { get; set; }
}

public class FecharCaixaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("resultado")]
    public FecharCaixaResultadoDTO? Resultado { get; set; }
}

public class FecharCaixaResultadoDTO
{
    [JsonPropertyName("cai_in_codigo")]
    public int CaiInCodigo { get; set; }

    [JsonPropertyName("valor_esperado")]
    public decimal ValorEsperado { get; set; }

    [JsonPropertyName("valor_fechamento")]
    public decimal ValorFechamento { get; set; }

    [JsonPropertyName("diferenca")]
    public decimal Diferenca { get; set; }

    [JsonPropertyName("dt_fechamento")]
    public string? DtFechamento { get; set; }
}

public class SangriaSuprimentoRequest
{
    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("observacao")]
    public string? Observacao { get; set; }
}

public class SangriaSuprimentoResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("movimento")]
    public MovimentoApiDTO? Movimento { get; set; }
}

public class MovimentoApiDTO
{
    [JsonPropertyName("mov_in_codigo")]
    public int MovInCodigo { get; set; }
}

public class ResumoResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("caixa_aberto")]
    public bool CaixaAberto { get; set; }

    [JsonPropertyName("caixa")]
    public CaixaApiDTO? Caixa { get; set; }

    [JsonPropertyName("saldo_atual")]
    public decimal SaldoAtual { get; set; }

    [JsonPropertyName("totais")]
    public ResumoTotaisDTO? Totais { get; set; }
}

public class ResumoTotaisDTO
{
    [JsonPropertyName("vendas")]
    public decimal Vendas { get; set; }

    [JsonPropertyName("sangrias")]
    public decimal Sangrias { get; set; }

    [JsonPropertyName("suprimentos")]
    public decimal Suprimentos { get; set; }

    [JsonPropertyName("estornos")]
    public decimal Estornos { get; set; }
}

public class ConfigTerminalResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("usar_terminal_fixo")]
    public bool UsarTerminalFixo { get; set; }

    [JsonPropertyName("terminais")]
    public List<TerminalDTO> Terminais { get; set; } = new();

    [JsonPropertyName("terminal_operador")]
    public TerminalDTO? TerminalOperador { get; set; }
}

public class TerminalDTO
{
    [JsonPropertyName("ter_in_codigo")]
    public int TerInCodigo { get; set; }

    [JsonPropertyName("ter_st_nome")]
    public string TerStNome { get; set; } = string.Empty;

    [JsonPropertyName("setor_nome")]
    public string? SetorNome { get; set; }
}
