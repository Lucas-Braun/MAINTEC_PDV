using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using System.Collections.ObjectModel;

namespace PDV.App.ViewModels;

public partial class ConsultaClienteViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    public ConsultaClienteViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        Clientes = new ObservableCollection<Cliente>();
    }

    // Callbacks
    public Action<Cliente?>? ClienteSelecionadoCallback { get; set; }
    public Action? Cancelado { get; set; }

    // =========================================
    // PROPRIEDADES - BUSCA
    // =========================================

    [ObservableProperty]
    private string _termoBusca = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Cliente> _clientes;

    [ObservableProperty]
    private Cliente? _clienteSelecionado;

    [ObservableProperty]
    private bool _processando = false;

    [ObservableProperty]
    private string _mensagemErro = string.Empty;

    [ObservableProperty]
    private string _mensagemSucesso = string.Empty;

    // =========================================
    // PROPRIEDADES - CADASTRO RAPIDO
    // =========================================

    [ObservableProperty]
    private string _nome = string.Empty;

    [ObservableProperty]
    private string _cpfCnpj = string.Empty;

    [ObservableProperty]
    private string _telefone = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    // =========================================
    // COMANDOS
    // =========================================

    [RelayCommand]
    private async Task Pesquisar()
    {
        if (string.IsNullOrWhiteSpace(TermoBusca)) return;

        try
        {
            Processando = true;
            MensagemErro = string.Empty;
            MensagemSucesso = string.Empty;

            var resultado = await _apiClient.BuscarClientes(TermoBusca.Trim());

            Clientes.Clear();
            foreach (var c in resultado)
                Clientes.Add(c);

            if (resultado.Count == 0)
                MensagemErro = "Nenhum cliente encontrado";
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro: {ex.Message}";
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private async Task Cadastrar()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            MensagemErro = "Informe o nome do cliente";
            return;
        }

        try
        {
            Processando = true;
            MensagemErro = string.Empty;
            MensagemSucesso = string.Empty;

            var resultado = await _apiClient.CadastrarCliente(
                Nome.Trim(),
                string.IsNullOrWhiteSpace(CpfCnpj) ? null : CpfCnpj.Trim(),
                string.IsNullOrWhiteSpace(Telefone) ? null : Telefone.Trim(),
                string.IsNullOrWhiteSpace(Email) ? null : Email.Trim());

            if (resultado.Duplicado && resultado.ClienteExistente != null)
            {
                MensagemErro = $"CPF/CNPJ ja cadastrado: {resultado.ClienteExistente.Nome} (Cod. {resultado.ClienteExistente.Id})";

                // Adiciona o existente na lista para facilitar selecao
                Clientes.Clear();
                Clientes.Add(resultado.ClienteExistente);
                ClienteSelecionado = resultado.ClienteExistente;
                return;
            }

            if (!resultado.Sucesso)
            {
                MensagemErro = resultado.Erro ?? "Erro ao cadastrar cliente";
                return;
            }

            if (resultado.Cliente != null)
            {
                MensagemSucesso = $"Cliente cadastrado: {resultado.Cliente.Nome} (Cod. {resultado.Cliente.Id})";

                // Seleciona automaticamente o novo cliente
                Clientes.Clear();
                Clientes.Add(resultado.Cliente);
                ClienteSelecionado = resultado.Cliente;

                // Limpa campos de cadastro
                Nome = string.Empty;
                CpfCnpj = string.Empty;
                Telefone = string.Empty;
                Email = string.Empty;
            }
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro: {ex.Message}";
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private void Selecionar()
    {
        if (ClienteSelecionado == null) return;
        ClienteSelecionadoCallback?.Invoke(ClienteSelecionado);
    }

    [RelayCommand]
    private void LimparCliente()
    {
        ClienteSelecionadoCallback?.Invoke(null);
    }

    [RelayCommand]
    private void Voltar()
    {
        Cancelado?.Invoke();
    }
}
