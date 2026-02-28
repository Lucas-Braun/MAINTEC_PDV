using System.Windows.Controls;

namespace PDV.App.Views;

public partial class ComprovanteView : UserControl
{
    public ComprovanteView()
    {
        InitializeComponent();
    }

    private void ComprovanteView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Focus();
    }
}
