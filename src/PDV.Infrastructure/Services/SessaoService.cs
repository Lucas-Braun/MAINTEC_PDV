using PDV.Core.Interfaces;
using PDV.Core.Models;

namespace PDV.Infrastructure.Services;

public class SessaoService : ISessaoService
{
    public string? Token { get; private set; }
    public bool Autenticado => !string.IsNullOrEmpty(Token);

    public UsuarioSessao? Usuario { get; private set; }
    public EmpresaSessao? Empresa { get; private set; }
    public FilialSessao? Filial { get; private set; }
    public List<FilialSessao> Filiais { get; private set; } = new();

    public ConfiguracaoPdv? Configuracao { get; private set; }
    public List<FormaPagamentoSessao> FormasPagamento { get; private set; } = new();
    public ConfigTerminal? ConfigTerminal { get; private set; }

    public void DefinirSessao(string token, UsuarioSessao usuario, EmpresaSessao empresa,
        FilialSessao filial, List<FilialSessao> filiais)
    {
        Token = token;
        Usuario = usuario;
        Empresa = empresa;
        Filial = filial;
        Filiais = filiais ?? new();
    }

    public void AtualizarToken(string novoToken)
    {
        Token = novoToken;
    }

    public void DefinirConfiguracao(ConfiguracaoPdv config)
    {
        Configuracao = config;
    }

    public void DefinirFormasPagamento(List<FormaPagamentoSessao> formas)
    {
        FormasPagamento = formas ?? new();
    }

    public void DefinirConfigTerminal(ConfigTerminal config)
    {
        ConfigTerminal = config;
    }

    public void Limpar()
    {
        Token = null;
        Usuario = null;
        Empresa = null;
        Filial = null;
        Filiais = new();
        Configuracao = null;
        FormasPagamento = new();
        ConfigTerminal = null;
    }
}
