using System.Windows;
using System.Windows.Input;
using PDV.App.ViewModels;

namespace PDV.App.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void BtnFechar_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Deseja fechar o PDV?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            Application.Current.Shutdown();
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            // Escape so fecha o app nas telas principais (Login/PDV)
            if (DataContext is MainViewModel vm &&
                (vm.TelaAtual is LoginViewModel || vm.TelaAtual is PDVViewModel))
            {
                BtnFechar_Click(sender, e);
            }
        }
    }
}
