using PDV.Core.Models;

namespace PDV.Core.Interfaces;

public interface ISessaoService
{
    // Token
    string? Token { get; }
    bool Autenticado { get; }

    // Dados da sessao
    UsuarioSessao? Usuario { get; }
    EmpresaSessao? Empresa { get; }
    FilialSessao? Filial { get; }
    List<FilialSessao> Filiais { get; }

    // Configuracoes carregadas da API
    ConfiguracaoPdv? Configuracao { get; }
    List<FormaPagamentoSessao> FormasPagamento { get; }
    ConfigTerminal? ConfigTerminal { get; }

    // Metodos
    void DefinirSessao(string token, UsuarioSessao usuario, EmpresaSessao empresa,
        FilialSessao filial, List<FilialSessao> filiais);
    void AtualizarToken(string novoToken);
    void DefinirConfiguracao(ConfiguracaoPdv config);
    void DefinirFormasPagamento(List<FormaPagamentoSessao> formas);
    void DefinirConfigTerminal(ConfigTerminal config);
    void Limpar();
}
