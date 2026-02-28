using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using PDV.Infrastructure.Api.DTOs;

namespace PDV.Infrastructure.Api;

public class ErpApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _token;
    private DateTime? _tokenExpiry;

    public ErpApiClient(HttpClient httpClient, ErpApiConfig config)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(config.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<bool> Autenticar(string usuario, string senha)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", new
            {
                username = usuario,
                password = senha,
                tipo = "pdv"
            });

            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
            if (result == null) return false;

            _token = result.AccessToken;
            _tokenExpiry = DateTime.Now.AddSeconds(result.ExpiresIn);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task EnsureAuthenticated()
    {
        if (_token == null || _tokenExpiry < DateTime.Now.AddMinutes(-5))
        {
            var response = await _httpClient.PostAsync("/api/v1/auth/refresh", null);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
                if (result != null)
                {
                    _token = result.AccessToken;
                    _tokenExpiry = DateTime.Now.AddSeconds(result.ExpiresIn);
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _token);
                }
            }
        }
    }

    public async Task<List<Produto>> SincronizarProdutos(DateTime? ultimaSincronizacao = null)
    {
        await EnsureAuthenticated();

        var url = "/api/v1/pdv/produtos";
        if (ultimaSincronizacao.HasValue)
            url += $"?atualizado_apos={ultimaSincronizacao.Value:yyyy-MM-ddTHH:mm:ss}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var dtos = await response.Content.ReadFromJsonAsync<List<ProdutoDTO>>(_jsonOptions);
        return dtos?.Select(MapToProduto).ToList() ?? new List<Produto>();
    }

    public async Task<Produto?> BuscarProduto(string codigoBarras)
    {
        await EnsureAuthenticated();

        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/pdv/produtos/barcode/{codigoBarras}");
            if (!response.IsSuccessStatusCode) return null;

            var dto = await response.Content.ReadFromJsonAsync<ProdutoDTO>(_jsonOptions);
            return dto != null ? MapToProduto(dto) : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> EnviarVenda(Venda venda)
    {
        await EnsureAuthenticated();

        var dto = new VendaDTO
        {
            NumeroVenda = venda.NumeroVenda,
            DataVenda = venda.DataVenda,
            ClienteCpfCnpj = venda.ClienteCpfCnpj,
            ValorTotal = venda.ValorTotal,
            DescontoTotal = venda.DescontoTotal,
            ChaveNfce = venda.ChaveNFCe,
            NumeroNfce = venda.NumeroNFCe,
            Itens = venda.Itens.Select(i => new ItemVendaDTO
            {
                ProdutoId = i.ProdutoId,
                CodigoBarras = i.CodigoBarras,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario,
                DescontoValor = i.DescontoValor,
                ValorTotal = i.ValorTotal
            }).ToList(),
            Pagamentos = venda.Pagamentos.Select(p => new PagamentoDTO
            {
                FormaPagamento = (int)p.FormaPagamento,
                Valor = p.Valor,
                Nsu = p.NSU,
                CodigoAutorizacao = p.CodigoAutorizacao,
                BandeiraCartao = p.BandeiraCartao,
                Parcelas = p.Parcelas
            }).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync("/api/v1/pdv/vendas", dto, _jsonOptions);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Venda>> ObterVendasPendentes()
    {
        // Vendas pendentes ficam no banco local
        return new List<Venda>();
    }

    public async Task<bool> VerificarConexao()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static Produto MapToProduto(ProdutoDTO dto) => new()
    {
        Id = dto.Id,
        CodigoBarras = dto.CodigoBarras ?? string.Empty,
        CodigoInterno = dto.CodigoInterno ?? string.Empty,
        Descricao = dto.Descricao,
        UnidadeMedida = dto.UnidadeMedida ?? "UN",
        PrecoVenda = dto.PrecoVenda,
        EstoqueAtual = dto.EstoqueAtual,
        NCM = dto.Ncm ?? string.Empty,
        CFOP = dto.Cfop ?? "5102",
        CST_ICMS = dto.CstIcms ?? string.Empty,
        CSOSN = dto.Csosn ?? string.Empty,
        AliquotaICMS = dto.AliquotaIcms,
        CEST = dto.Cest ?? string.Empty,
        Ativo = dto.Ativo,
        UltimaAtualizacao = dto.UltimaAtualizacao
    };
}
