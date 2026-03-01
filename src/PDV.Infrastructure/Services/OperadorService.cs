using PDV.Core.Interfaces;
using PDV.Core.Models;

namespace PDV.Infrastructure.Services;

public class OperadorService : IOperadorService
{
    private readonly IApiClient _apiClient;
    private readonly ISessaoService _sessao;

    public Operador? OperadorLogado { get; private set; }

    public OperadorService(IApiClient apiClient, ISessaoService sessao)
    {
        _apiClient = apiClient;
        _sessao = sessao;
    }

    public async Task<Operador?> Autenticar(string login, string senha)
    {
        var resultado = await _apiClient.Login(login, senha);

        if (!resultado.Sucesso || resultado.Token == null || resultado.Usuario == null)
            return null;

        // Popula sessao com dados do login
        _sessao.DefinirSessao(
            resultado.Token,
            resultado.Usuario,
            resultado.Empresa ?? new EmpresaSessao(),
            resultado.Filial ?? new FilialSessao(),
            resultado.Filiais ?? new()
        );

        // Carrega config, formas de pagamento e config terminal em paralelo
        var configTask = _apiClient.ObterConfiguracao();
        var formasTask = _apiClient.ObterFormasPagamento();
        var terminalTask = _apiClient.ObterConfigTerminal();

        await Task.WhenAll(configTask, formasTask, terminalTask);

        var config = await configTask;
        if (config != null) _sessao.DefinirConfiguracao(config);

        var formas = await formasTask;
        _sessao.DefinirFormasPagamento(formas);

        var terminal = await terminalTask;
        if (terminal != null) _sessao.DefinirConfigTerminal(terminal);

        // Cria operador local para compatibilidade
        OperadorLogado = new Operador
        {
            Id = resultado.Usuario.Id,
            Nome = resultado.Usuario.Nome,
            Login = login,
            Perfil = "caixa",
            Ativo = true
        };

        return OperadorLogado;
    }

    public void Logout()
    {
        OperadorLogado = null;
        _sessao.Limpar();
    }
}
