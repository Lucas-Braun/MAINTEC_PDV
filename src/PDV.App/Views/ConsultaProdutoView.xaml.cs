using System.Windows;
using System.Windows.Controls;

namespace PDV.App.Views;

public partial class ConsultaProdutoView : UserControl
{
    public ConsultaProdutoView()
    {
        InitializeComponent();
        Loaded += (_, _) => TxtBusca.Focus();
    }
}
