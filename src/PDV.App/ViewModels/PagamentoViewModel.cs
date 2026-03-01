using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Enums;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using System.Collections.ObjectModel;

namespace PDV.App.ViewModels;

public partial class PagamentoViewModel : ObservableObject
{
    private readonly ITEFService _tefService;
    private readonly ISessaoService _sessao;

    public PagamentoViewModel(ITEFService tefService, ISessaoService sessao)
    {
        _tefService = tefService;
        _sessao = sessao;
        Pagamentos = new ObservableCollection<Pagamento>();

        // Carrega formas de pagamento da sessao
        CarregarFormasPagamento();
    }

    // Callbacks
    public Action<List<Pagamento>>? PagamentoConfirmado { get; set; }
    public Action? Cancelado { get; set; }

    // Valor total da venda (setado pelo PDV antes de navegar)
    [ObservableProperty]
    private decimal _valorTotal;

    [ObservableProperty]
    private FormaPagamentoSessao? _formaSelecionadaSessao;

    [ObservableProperty]
    private FormaPagamento _formaSelecionada;

    [ObservableProperty]
    private decimal _valorPagamento;

    [ObservableProperty]
    private decimal _valorRecebido;

    [ObservableProperty]
    private int _parcelas = 1;

    [ObservableProperty]
    private ObservableCollection<Pagamento> _pagamentos;

    [ObservableProperty]
    private string _mensagemStatus = string.Empty;

    [ObservableProperty]
    private bool _processando = false;

    [ObservableProperty]
    private ObservableCollection<FormaPagamentoSessao> _formasPagamentoDisponiveis = new();

    // Propriedades calculadas
    public decimal TotalPago => Pagamentos.Sum(p => p.Valor);
    public decimal ValorRestante => ValorTotal - TotalPago;
    public decimal Troco => PermiteTroco && ValorRecebido > ValorPagamento
        ? ValorRecebido - ValorPagamento : 0;
    public bool PodeConfirmar => ValorRestante <= 0;
    public bool PermiteTroco => FormaSelecionadaSessao?.PermiteTroco ?? false;

    // Formas de pagamento disponiveis para binding (compatibilidade)
    public FormaPagamento[] FormasPagamento => new[]
    {
        FormaPagamento.Dinheiro,
        FormaPagamento.CartaoCredito,
        FormaPagamento.CartaoDebito,
        FormaPagamento.PIX
    };

    private void CarregarFormasPagamento()
    {
        var formas = _sessao.FormasPagamento;
        if (formas.Count > 0)
        {
            FormasPagamentoDisponiveis = new ObservableCollection<FormaPagamentoSessao>(formas);
            var padrao = formas.FirstOrDefault(f => f.Padrao) ?? formas[0];
            FormaSelecionadaSessao = padrao;
            MapearFormaPagamento(padrao);
        }
        else
        {
            // Fallback se API nao retornou formas
            FormaSelecionada = FormaPagamento.Dinheiro;
        }
    }

    private void MapearFormaPagamento(FormaPagamentoSessao forma)
    {
        var nomeLower = forma.Nome.ToLower();
        if (nomeLower.Contains("dinheiro"))
            FormaSelecionada = FormaPagamento.Dinheiro;
        else if (nomeLower.Contains("credito"))
            FormaSelecionada = FormaPagamento.CartaoCredito;
        else if (nomeLower.Contains("debito"))
            FormaSelecionada = FormaPagamento.CartaoDebito;
        else if (nomeLower.Contains("pix"))
            FormaSelecionada = FormaPagamento.PIX;
        else
            FormaSelecionada = FormaPagamento.Outros;
    }

    partial void OnFormaSelecionadaSessaoChanged(FormaPagamentoSessao? value)
    {
        if (value != null)
            MapearFormaPagamento(value);

        ValorPagamento = ValorRestante > 0 ? ValorRestante : 0;
        ValorRecebido = 0;
        Parcelas = 1;
        AtualizarCalculos();
    }

    partial void OnFormaSelecionadaChanged(FormaPagamento value)
    {
        AtualizarCalculos();
    }

    partial void OnValorPagamentoChanged(decimal value)
    {
        AtualizarCalculos();
    }

    partial void OnValorRecebidoChanged(decimal value)
    {
        AtualizarCalculos();
    }

    [RelayCommand]
    private void AdicionarPagamento()
    {
        if (ValorPagamento <= 0)
        {
            MensagemStatus = "Informe o valor do pagamento";
            return;
        }

        if (ValorRestante <= 0)
        {
            MensagemStatus = "Valor total ja atingido";
            return;
        }

        var valorEfetivo = Math.Min(ValorPagamento, ValorRestante);

        var pagamento = new Pagamento
        {
            FormaPagamento = FormaSelecionada,
            FcbInCodigo = FormaSelecionadaSessao?.FcbInCodigo ?? 0,
            Valor = valorEfetivo,
            Parcelas = FormaSelecionada == FormaPagamento.CartaoCredito ? Parcelas : null,
            DataPagamento = DateTime.Now
        };

        if (PermiteTroco)
        {
            pagamento.ValorRecebido = ValorRecebido > 0 ? ValorRecebido : valorEfetivo;
        }

        Pagamentos.Add(pagamento);
        AtualizarCalculos();

        // Prepara para proximo pagamento
        ValorPagamento = ValorRestante > 0 ? ValorRestante : 0;
        ValorRecebido = 0;

        var nomeForma = FormaSelecionadaSessao?.Nome ?? FormaSelecionada.ToString();
        MensagemStatus = $"{nomeForma} - {valorEfetivo:C2} adicionado";
    }

    [RelayCommand]
    private void RemoverPagamento(Pagamento? pagamento)
    {
        if (pagamento == null) return;

        Pagamentos.Remove(pagamento);
        AtualizarCalculos();
        ValorPagamento = ValorRestante > 0 ? ValorRestante : 0;
        MensagemStatus = "Pagamento removido";
    }

    [RelayCommand]
    private void Confirmar()
    {
        if (!PodeConfirmar)
        {
            MensagemStatus = $"Faltam {ValorRestante:C2} para completar o pagamento";
            return;
        }

        PagamentoConfirmado?.Invoke(Pagamentos.ToList());
    }

    [RelayCommand]
    private void Voltar()
    {
        Cancelado?.Invoke();
    }

    private void AtualizarCalculos()
    {
        OnPropertyChanged(nameof(TotalPago));
        OnPropertyChanged(nameof(ValorRestante));
        OnPropertyChanged(nameof(Troco));
        OnPropertyChanged(nameof(PodeConfirmar));
        OnPropertyChanged(nameof(PermiteTroco));
    }
}
