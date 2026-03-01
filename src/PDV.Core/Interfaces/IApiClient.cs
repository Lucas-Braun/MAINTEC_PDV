using PDV.Core.Models;

namespace PDV.Core.Interfaces;

public interface IApiClient
{
    // Auth
    Task<ResultadoLogin> Login(string login, string senha);
    Task<bool> RefreshToken();
    Task<bool> Ping();

    // Config
    Task<ConfiguracaoPdv?> ObterConfiguracao();
    Task<List<FormaPagamentoSessao>> ObterFormasPagamento();

    // Caixa
    Task<ResultadoCaixaStatus> ObterStatusCaixa();
    Task<ConfigTerminal?> ObterConfigTerminal();
    Task<ResultadoAbrirCaixa> AbrirCaixa(decimal valorAbertura, int? terInCodigo);
    Task<ResultadoFecharCaixa> FecharCaixa(decimal valorFechamento);
    Task<ResultadoOperacao> RegistrarSangria(decimal valor, string? observacao);
    Task<ResultadoOperacao> RegistrarSuprimento(decimal valor, string? observacao);
    Task<ResultadoResumoCaixa> ObterResumoCaixa();

    // Produto
    Task<Produto?> BuscarProdutoPorCodigo(string codigo);
    Task<List<Produto>> PesquisarProdutos(string termo, int limite = 20);

    // Venda
    Task<ResultadoVenda> FinalizarVendaDireta(List<ItemVendaApi> itens, List<ParcelaApi> parcelas,
        string? cpfNota, decimal? troco, string idempotencyKey);

    // Cliente
    Task<List<Cliente>> BuscarClientes(string termo);
    Task<Cliente?> BuscarClientePorDocumento(string cpfCnpj);
    Task<ResultadoCadastroCliente> CadastrarCliente(string nome, string? cpfCnpj, string? telefone, string? email);
}

// ============ Result Types ============

public class ResultadoLogin
{
    public bool Sucesso { get; set; }
    public string? Token { get; set; }
    public UsuarioSessao? Usuario { get; set; }
    public EmpresaSessao? Empresa { get; set; }
    public FilialSessao? Filial { get; set; }
    public List<FilialSessao>? Filiais { get; set; }
    public string? Erro { get; set; }
    public string? Codigo { get; set; }
    public int? TentativasRestantes { get; set; }
    public int? MinutosRestantes { get; set; }
}

public class ResultadoCaixaStatus
{
    public bool Sucesso { get; set; }
    public bool CaixaAberto { get; set; }
    public int? CaixaCodigo { get; set; }
    public string? Erro { get; set; }
}

public class ResultadoAbrirCaixa
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public int? CaixaCodigo { get; set; }
    public string? Erro { get; set; }
}

public class ResultadoFecharCaixa
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public decimal ValorEsperado { get; set; }
    public decimal ValorFechamento { get; set; }
    public decimal Diferenca { get; set; }
    public string? Erro { get; set; }
}

public class ResultadoOperacao
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public string? Erro { get; set; }
}

public class ResultadoResumoCaixa
{
    public bool Sucesso { get; set; }
    public bool CaixaAberto { get; set; }
    public decimal SaldoAtual { get; set; }
    public decimal TotalVendas { get; set; }
    public decimal TotalSangrias { get; set; }
    public decimal TotalSuprimentos { get; set; }
    public decimal TotalEstornos { get; set; }
    public decimal ValorAbertura { get; set; }
    public string? Erro { get; set; }
}

public class ResultadoVenda
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public int? PedidoCodigo { get; set; }
    public int? NfCodigo { get; set; }
    public decimal? ValorVenda { get; set; }
    public decimal? Troco { get; set; }
    public string? NfceStatus { get; set; }
    public string? NfceChave { get; set; }
    public bool FromCache { get; set; }
    public string? Erro { get; set; }
}

public class ItemVendaApi
{
    public int ProInCodigo { get; set; }
    public decimal Quantidade { get; set; }
    public decimal? PrecoUnitario { get; set; }
    public decimal? DescontoPerc { get; set; }
}

public class ParcelaApi
{
    public int FcbInCodigo { get; set; }
    public decimal Valor { get; set; }
    public string? Vencimento { get; set; }
}

public class ResultadoCadastroCliente
{
    public bool Sucesso { get; set; }
    public Cliente? Cliente { get; set; }
    public string? Erro { get; set; }
    public bool Duplicado { get; set; }
    public Cliente? ClienteExistente { get; set; }
}
