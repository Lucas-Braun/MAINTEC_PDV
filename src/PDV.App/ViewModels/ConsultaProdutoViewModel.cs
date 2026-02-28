using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using System.Collections.ObjectModel;

namespace PDV.App.ViewModels;

public partial class ConsultaProdutoViewModel : ObservableObject
{
    private readonly IProdutoService _produtoService;

    public ConsultaProdutoViewModel(IProdutoService produtoService)
    {
        _produtoService = produtoService;
        Produtos = new ObservableCollection<Produto>();
    }

    // Callbacks
    public Action<Produto>? ProdutoSelecionadoCallback { get; set; }
    public Action? Cancelado { get; set; }

    [ObservableProperty]
    private string _termoBusca = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Produto> _produtos;

    [ObservableProperty]
    private Produto? _produtoSelecionado;

    [ObservableProperty]
    private bool _processando = false;

    [RelayCommand]
    private async Task Pesquisar()
    {
        if (string.IsNullOrWhiteSpace(TermoBusca)) return;

        try
        {
            Processando = true;
            var resultado = await _produtoService.Pesquisar(TermoBusca);
            Produtos.Clear();
            foreach (var p in resultado)
                Produtos.Add(p);
        }
        catch (Exception)
        {
            // Silently handle - empty results shown
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private void Selecionar()
    {
        if (ProdutoSelecionado == null) return;
        ProdutoSelecionadoCallback?.Invoke(ProdutoSelecionado);
    }

    [RelayCommand]
    private void Voltar()
    {
        Cancelado?.Invoke();
    }
}
