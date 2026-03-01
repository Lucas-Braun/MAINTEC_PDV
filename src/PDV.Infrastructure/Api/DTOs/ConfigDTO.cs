using System.Text.Json.Serialization;

namespace PDV.Infrastructure.Api.DTOs;

public class ConfiguracaoResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("configuracao")]
    public ConfiguracaoPdvDTO? Configuracao { get; set; }
}

public class ConfiguracaoPdvDTO
{
    [JsonPropertyName("pdv_bo_emitir_nfce_auto")]
    public string PdvBoEmitirNfceAuto { get; set; } = "S";

    [JsonPropertyName("pdv_bo_exigir_cpf")]
    public string PdvBoExigirCpf { get; set; } = "N";

    [JsonPropertyName("pdv_in_casas_decimais_qtd")]
    public int PdvInCasasDecimaisQtd { get; set; } = 3;

    [JsonPropertyName("pdv_in_casas_decimais_preco")]
    public int PdvInCasasDecimaisPreco { get; set; } = 2;

    [JsonPropertyName("pdv_bo_exigir_abertura")]
    public string PdvBoExigirAbertura { get; set; } = "S";

    [JsonPropertyName("pdv_bo_imprimir_cupom")]
    public string PdvBoImprimirCupom { get; set; } = "S";

    [JsonPropertyName("pdv_bo_usar_terminal_fixo")]
    public string PdvBoUsarTerminalFixo { get; set; } = "N";

    [JsonPropertyName("pdv_st_modo_entrada")]
    public string PdvStModoEntrada { get; set; } = "A";

    [JsonPropertyName("pdv_bo_usar_turno")]
    public string PdvBoUsarTurno { get; set; } = "N";

    [JsonPropertyName("pdv_bo_bloquear_fora_turno")]
    public string PdvBoBloquearForaTurno { get; set; } = "N";

    [JsonPropertyName("pdv_in_aviso_fim_turno")]
    public int PdvInAvisoFimTurno { get; set; } = 15;

    [JsonPropertyName("pdv_in_limite_horas_aberto")]
    public int PdvInLimiteHorasAberto { get; set; } = 12;
}

public class FormasPagamentoResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("formas")]
    public List<FormaPagamentoApiDTO> Formas { get; set; } = new();
}

public class FormaPagamentoApiDTO
{
    [JsonPropertyName("fcb_in_codigo")]
    public int FcbInCodigo { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("descricao")]
    public string? Descricao { get; set; }

    [JsonPropertyName("padrao")]
    public string Padrao { get; set; } = "N";

    [JsonPropertyName("permite_troco")]
    public string PermiteTroco { get; set; } = "N";
}

public class PingResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("operador")]
    public string? Operador { get; set; }

    [JsonPropertyName("emp")]
    public int? Emp { get; set; }
}
