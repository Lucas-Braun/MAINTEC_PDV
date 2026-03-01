using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Interfaces;

namespace PDV.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IOperadorService _operadorService;

    public LoginViewModel(IOperadorService operadorService)
    {
        _operadorService = operadorService;
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
}
