using System.Collections.Specialized;
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
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is PDVViewModel vm)
        {
            vm.Itens.CollectionChanged += Itens_CollectionChanged;
        }
    }

    private void Itens_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && GridItens.Items.Count > 0)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                GridItens.ScrollIntoView(GridItens.Items[GridItens.Items.Count - 1]);
            }), DispatcherPriority.Background);
        }
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

    private void FecharBuscaAvancada_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is PDVViewModel vm)
            vm.FecharBuscaAvancadaCommand.Execute(null);
    }

    private void BuscaAvancada_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is PDVViewModel vm && vm.ProdutoBuscaSelecionado != null)
            vm.SelecionarProdutoBuscaCommand.Execute(vm.ProdutoBuscaSelecionado);
    }

    private void FecharCadastroCliente_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is PDVViewModel vm)
            vm.FecharCadastroClienteCommand.Execute(null);
    }
}
