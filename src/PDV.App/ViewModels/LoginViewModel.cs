using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Interfaces;

namespace PDV.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IOperadorService _operadorService;
    private readonly IApiClient _apiClient;

    public LoginViewModel(IOperadorService operadorService, IApiClient apiClient)
    {
        _operadorService = operadorService;
        _apiClient = apiClient;
    }

    // Callback chamado pelo MainViewModel quando login tem sucesso
    public Action<string>? LoginSucesso { get; set; }

    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _senha = string.Empty;

    [ObservableProperty]
    private string _mensagemErro = string.Empty;

    [ObservableProperty]
    private bool _processando = false;

    [ObservableProperty]
    private bool _erpConectado = false;

    [RelayCommand]
    private async Task Entrar()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Senha))
        {
            MensagemErro = "Informe login e senha";
            return;
        }

        try
        {
            Processando = true;
            MensagemErro = string.Empty;

            var operador = await _operadorService.Autenticar(Login, Senha);

            if (operador == null)
            {
                MensagemErro = "Login ou senha incorretos";
                return;
            }

            if (!operador.Ativo)
            {
                MensagemErro = "Operador inativo - contate o supervisor";
                return;
            }

            // Tenta autenticar no ERP em background (nao trava o login)
            _ = Task.Run(async () =>
            {
                try { ErpConectado = await _apiClient.Autenticar(Login, Senha); }
                catch { ErpConectado = false; }
            });

            LoginSucesso?.Invoke(operador.Nome);
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro: {ex.Message}";
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private async Task VerificarConexaoErp()
    {
        try { ErpConectado = await _apiClient.VerificarConexao(); }
        catch { ErpConectado = false; }
    }
}
