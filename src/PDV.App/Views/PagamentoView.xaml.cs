using System.Windows;
using System.Windows.Controls;
using PDV.App.ViewModels;
using PDV.Core.Enums;

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
        AtualizarVisibilidade();
    }

    private void FormaPagamento_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && DataContext is PagamentoViewModel vm && rb.Tag is string tag)
        {
            if (Enum.TryParse<FormaPagamento>(tag, out var forma))
            {
                vm.FormaSelecionada = forma;
                AtualizarVisibilidade();
            }
        }
    }

    private void AtualizarVisibilidade()
    {
        if (DataContext is not PagamentoViewModel vm) return;

        var isDinheiro = vm.FormaSelecionada == FormaPagamento.Dinheiro;
        var isCredito = vm.FormaSelecionada == FormaPagamento.CartaoCredito;

        if (PnlValorRecebido != null)
            PnlValorRecebido.Visibility = isDinheiro ? Visibility.Visible : Visibility.Collapsed;
        if (PnlParcelas != null)
            PnlParcelas.Visibility = isCredito ? Visibility.Visible : Visibility.Collapsed;
        if (PnlTroco != null)
            PnlTroco.Visibility = isDinheiro ? Visibility.Visible : Visibility.Collapsed;
    }
}
