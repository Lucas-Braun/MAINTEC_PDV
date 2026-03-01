using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Enums;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using System.Collections.ObjectModel;

namespace PDV.App.ViewModels;

public partial class ComprovanteViewModel : ObservableObject
{
    private readonly IImpressoraService _impressoraService;
    private readonly ISessaoService _sessao;

    public ComprovanteViewModel(IImpressoraService impressoraService, ISessaoService sessao)
    {
        _impressoraService = impressoraService;
        _sessao = sessao;
    }

    // Callback de navegacao
    public Action? Voltar { get; set; }

    // Venda completa
    [ObservableProperty]
    private Venda _venda = new();

    // Propriedades derivadas para binding
    public string NumeroVenda => Venda.NumeroVenda;
    public string DataVenda => Venda.DataVenda.ToString("dd/MM/yyyy HH:mm");
    public ObservableCollection<ItemVenda> Itens => new(Venda.Itens);
    public decimal SubTotal => Venda.SubTotal;
    public decimal DescontoTotal => Venda.DescontoTotal;
    public decimal ValorTotal => Venda.ValorTotal;
    public ObservableCollection<Pagamento> PagamentosLista => new(Venda.Pagamentos);

    // Dados NFC-e
    public string? ChaveNFCe => Venda.ChaveNFCe;
    public int? NumeroNFCe => Venda.NumeroNFCe;
    public string? ProtocoloAutorizacao => Venda.ProtocoloAutorizacao;

    public string StatusNFCe => Venda.Status == StatusVenda.Contingencia
        ? "CONTINGENCIA"
        : "AUTORIZADA";

    public bool EmContingencia => Venda.Status == StatusVenda.Contingencia;

    public string NomeFormaPagamento(FormaPagamento forma)
    {
        // Tenta buscar o nome da forma na sessao
        var formasSessao = _sessao.FormasPagamento;
        if (formasSessao.Count > 0)
        {
            // Mapeia enum para busca aproximada
            var nome = forma switch
            {
                FormaPagamento.Dinheiro => "dinheiro",
                FormaPagamento.CartaoCredito => "credito",
                FormaPagamento.CartaoDebito => "debito",
                FormaPagamento.PIX => "pix",
                _ => ""
            };
            var match = formasSessao.FirstOrDefault(f => f.Nome.ToLower().Contains(nome));
            if (match != null) return match.Nome;
        }

        return forma switch
        {
            FormaPagamento.Dinheiro => "Dinheiro",
            FormaPagamento.CartaoCredito => "Cartao Credito",
            FormaPagamento.CartaoDebito => "Cartao Debito",
            FormaPagamento.PIX => "PIX",
            _ => "Outros"
        };
    }

    partial void OnVendaChanged(Venda value)
    {
        OnPropertyChanged(nameof(NumeroVenda));
        OnPropertyChanged(nameof(DataVenda));
        OnPropertyChanged(nameof(Itens));
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(DescontoTotal));
        OnPropertyChanged(nameof(ValorTotal));
        OnPropertyChanged(nameof(PagamentosLista));
        OnPropertyChanged(nameof(ChaveNFCe));
        OnPropertyChanged(nameof(NumeroNFCe));
        OnPropertyChanged(nameof(ProtocoloAutorizacao));
        OnPropertyChanged(nameof(StatusNFCe));
        OnPropertyChanged(nameof(EmContingencia));
    }

    [RelayCommand]
    private async Task Reimprimir()
    {
        try
        {
            await _impressoraService.ImprimirCupom(Venda);
        }
        catch
        {
            // Impressora indisponivel - nao impede o fluxo
        }
    }

    [RelayCommand]
    private void VoltarParaPDV()
    {
        Voltar?.Invoke();
    }
}
