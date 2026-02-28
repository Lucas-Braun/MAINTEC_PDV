using System.Windows;
using System.Windows.Controls;

namespace PDV.App.Views;

public partial class FechamentoCaixaView : UserControl
{
    public FechamentoCaixaView()
    {
        InitializeComponent();
        Loaded += (_, _) => TxtValorFechamento.Focus();
    }
}
