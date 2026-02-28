using System.Windows;
using System.Windows.Controls;

namespace PDV.App.Views;

public partial class AberturaCaixaView : UserControl
{
    public AberturaCaixaView()
    {
        InitializeComponent();
        Loaded += (_, _) => TxtValorAbertura.Focus();
    }
}
