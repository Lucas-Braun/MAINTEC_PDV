using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using PDV.App.ViewModels;

namespace PDV.App.Views;

public partial class PDVView : UserControl
{
    public PDVView()
    {
        InitializeComponent();
    }

    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (DataContext is PDVViewModel vm)
                vm.AtualizarAposEdicaoQuantidade();
        }), DispatcherPriority.ApplicationIdle);
    }
}
