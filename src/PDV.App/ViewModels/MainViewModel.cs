using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PDV.Core.Enums;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using PDV.Infrastructure.Api;

namespace PDV.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _services;
    private PDVViewModel? _pdvVmAtual;

    public MainViewModel(IServiceProvider services)
    {
        _services = services;
        // Inicia na tela de login
        NavegarParaLogin();
    }

    [ObservableProperty]
    private ObservableObject? _telaAtual;

    [ObservableProperty]
    private string _nomeOperador = string.Empty;

    [ObservableProperty]
    private bool _operadorLogado = false;

    [RelayCommand]
    private void NavegarParaLogin()
    {
        // Para o keep-alive ao voltar para login
        var keepAlive = _services.GetService<ApiKeepAliveService>();
        keepAlive?.Parar();

        var loginVm = _services.GetRequiredService<LoginViewModel>();
        loginVm.LoginSucesso = OnLoginSucesso;
        TelaAtual = loginVm;
        OperadorLogado = false;
        NomeOperador = string.Empty;
        _pdvVmAtual = null;
    }

    [RelayCommand]
    private void NavegarParaPDV()
    {
        if (_pdvVmAtual == null)
        {
            _pdvVmAtual = _services.GetRequiredService<PDVViewModel>();
            ConfigurarCallbacksPDV(_pdvVmAtual);
        }
        TelaAtual = _pdvVmAtual;
    }

    private void ConfigurarCallbacksPDV(PDVViewModel pdvVm)
    {
        pdvVm.SolicitarPagamento = NavegarParaPagamento;
        pdvVm.SolicitarConsulta = NavegarParaConsultaProduto;
        pdvVm.SolicitarSangria = () => NavegarParaSangriaSuprimento(TipoMovimentoCaixa.Sangria);
        pdvVm.SolicitarSuprimento = () => NavegarParaSangriaSuprimento(TipoMovimentoCaixa.Suprimento);
        pdvVm.SolicitarFechamento = NavegarParaFechamentoCaixa;
        pdvVm.SolicitarConfiguracoes = () => NavegarParaConfiguracoes();
        pdvVm.VendaFinalizada = NavegarParaComprovante;
    }

    private async void OnLoginSucesso(string nomeOperador)
    {
        NomeOperador = nomeOperador;
        OperadorLogado = true;

        // Inicia keep-alive apos login
        var keepAlive = _services.GetService<ApiKeepAliveService>();
        keepAlive?.Iniciar();

        // Verifica se ja tem caixa aberto via API
        var apiClient = _services.GetRequiredService<IApiClient>();
        var status = await apiClient.ObterStatusCaixa();

        if (status.Sucesso && status.CaixaAberto)
        {
            NavegarParaPDV();
            return;
        }

        NavegarParaAberturaCaixa();
    }

    private void NavegarParaAberturaCaixa()
    {
        var vm = _services.GetRequiredService<AberturaCaixaViewModel>();
        vm.CaixaAberto = () => NavegarParaPDV();
        TelaAtual = vm;
    }

    private void NavegarParaPagamento(decimal valorTotal)
    {
        var vm = _services.GetRequiredService<PagamentoViewModel>();
        vm.ValorTotal = valorTotal;
        vm.ValorPagamento = valorTotal;
        vm.PagamentoConfirmado = pagamentos =>
        {
            TelaAtual = _pdvVmAtual;
            _ = _pdvVmAtual!.ProcessarPagamento(pagamentos);
        };
        vm.Cancelado = () => TelaAtual = _pdvVmAtual;
        TelaAtual = vm;
    }

    private void NavegarParaConsultaProduto()
    {
        var vm = _services.GetRequiredService<ConsultaProdutoViewModel>();
        vm.ProdutoSelecionadoCallback = produto =>
        {
            TelaAtual = _pdvVmAtual;
            _pdvVmAtual!.InserirProdutoConsultado(produto);
        };
        vm.Cancelado = () => TelaAtual = _pdvVmAtual;
        TelaAtual = vm;
    }

    private void NavegarParaSangriaSuprimento(TipoMovimentoCaixa tipo)
    {
        var vm = _services.GetRequiredService<SangriaSuprimentoViewModel>();
        vm.Tipo = tipo;
        vm.Confirmado = () =>
        {
            TelaAtual = _pdvVmAtual;
            _pdvVmAtual!.MensagemStatus = tipo == TipoMovimentoCaixa.Sangria
                ? $"Sangria de {vm.Valor:C2} realizada"
                : $"Suprimento de {vm.Valor:C2} realizado";
        };
        vm.Cancelado = () => TelaAtual = _pdvVmAtual;
        TelaAtual = vm;
    }

    private void NavegarParaFechamentoCaixa()
    {
        var vm = _services.GetRequiredService<FechamentoCaixaViewModel>();
        _ = vm.CarregarResumoCaixa();
        vm.CaixaFechado = () => NavegarParaLogin();
        vm.Cancelado = () => TelaAtual = _pdvVmAtual;
        TelaAtual = vm;
    }

    private void NavegarParaComprovante(Venda venda)
    {
        var vm = _services.GetRequiredService<ComprovanteViewModel>();
        vm.Venda = venda;
        vm.Voltar = () =>
        {
            _pdvVmAtual?.NovaVenda();
            NavegarParaPDV();
        };
        TelaAtual = vm;
    }

    private void NavegarParaConfiguracoes()
    {
        var vm = _services.GetRequiredService<ConfiguracoesViewModel>();
        vm.Voltar = () => NavegarParaPDV();
        TelaAtual = vm;
    }

    [RelayCommand]
    private void Logout()
    {
        var operadorService = _services.GetRequiredService<IOperadorService>();
        operadorService.Logout();
        NavegarParaLogin();
    }
}
