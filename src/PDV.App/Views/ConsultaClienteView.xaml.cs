using System.Windows.Controls;

namespace PDV.App.Views;

public partial class ConsultaClienteView : UserControl
{
    public ConsultaClienteView()
    {
        InitializeComponent();
        Loaded += (_, _) => TxtBusca.Focus();
    }
}
