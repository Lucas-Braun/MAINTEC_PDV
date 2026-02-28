using System.Windows;
using System.Windows.Controls;
using PDV.App.ViewModels;

namespace PDV.App.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Foco no campo de login ao abrir
        TxtLogin.Focus();

        // PasswordBox nao suporta Binding direto, entao usamos evento
        TxtSenha.PasswordChanged += (s, _) =>
        {
            if (DataContext is LoginViewModel vm)
                vm.Senha = TxtSenha.Password;
        };

        // Enter no campo senha aciona login
        TxtSenha.KeyDown += (s, args) =>
        {
            if (args.Key == System.Windows.Input.Key.Return && DataContext is LoginViewModel vm)
                vm.EntrarCommand.Execute(null);
        };
    }
}
