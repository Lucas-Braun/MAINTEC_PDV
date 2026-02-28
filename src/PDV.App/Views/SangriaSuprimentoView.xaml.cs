using System.Windows;
using System.Windows.Controls;

namespace PDV.App.Views;

public partial class SangriaSuprimentoView : UserControl
{
    public SangriaSuprimentoView()
    {
        InitializeComponent();
        Loaded += (_, _) => TxtValor.Focus();
    }
}
