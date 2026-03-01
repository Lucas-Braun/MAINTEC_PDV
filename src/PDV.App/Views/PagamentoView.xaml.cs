using System.Windows;
using System.Windows.Controls;

namespace PDV.App.Views;

public partial class PagamentoView : UserControl
{
    public PagamentoView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TxtValorPagamento.Focus();
    }
}
