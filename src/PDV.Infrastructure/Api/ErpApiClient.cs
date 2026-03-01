using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PDV.Core.Interfaces;
using PDV.Core.Models;
using PDV.Infrastructure.Api.DTOs;

namespace PDV.Infrastructure.Api;

public class ErpApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ISessaoService _sessao;
    private readonly string _prefix;
    private readonly JsonSerializerOptions _jsonOptions;

    public ErpApiClient(HttpClient httpClient, ErpApiConfig config, ISessaoService sessao)
    {
        _httpClient = httpClient;
        _sessao = sessao;
        _prefix = config.ApiPrefix;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    // ======================== AUTH ========================

    public async Task<ResultadoLogin> Login(string login, string senha)
    {
        try
        {
            var body = new { login, senha };
            var response = await _httpClient.PostAsJsonAsync($"{_prefix}/auth/login", body);

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<LoginResponse>(json, _jsonOptions);

            if (dto == null)
                return new ResultadoLogin { Sucesso = false, Erro = "Resposta invalida do servidor" };

            if (!dto.Success)
            {
                return new ResultadoLogin
                {
                    Sucesso = false,
                    Erro = dto.Error ?? "Credenciais invalidas",
                    Codigo = dto.Code,
                    TentativasRestantes = dto.TentativasRestantes,
                    MinutosRestantes = dto.MinutosRestantes
                };
            }

            var resultado = new ResultadoLogin
            {
                Sucesso = true,
                Token = dto.Token
            };

            if (dto.Usuario != null)
            {
                resultado.Usuario = new UsuarioSessao
                {
                    Id = dto.Usuario.Id,
                    Nome = dto.Usuario.Nome,
                    Email = dto.Usuario.Email
                };
            }

            if (dto.Empresa != null)
            {
                resultado.Empresa = new EmpresaSessao
                {
                    Id = dto.Empresa.Id,
                    Nome = dto.Empresa.Nome,
                    Cnpj = dto.Empresa.Cnpj
                };
            }

            if (dto.Filial != null)
            {
                resultado.Filial = new FilialSessao
                {
                    Id = dto.Filial.Id,
                    Nome = dto.Filial.Nome,
                    Cnpj = dto.Filial.Cnpj,
                    OrgId = dto.Filial.OrgId,
                    OrgNome = dto.Filial.OrgNome
                };
            }

            resultado.Filiais = dto.Filiais?.Select(f => new FilialSessao
            {
                Id = f.Id,
                Nome = f.Nome,
                Cnpj = f.Cnpj,
                OrgId = f.OrgId,
                OrgNome = f.OrgNome
            }).ToList();

            return resultado;
        }
        catch (Exception ex)
        {
            return new ResultadoLogin { Sucesso = false, Erro = $"Erro de conexao: {ex.Message}" };
        }
    }

    public async Task<bool> RefreshToken()
    {
        try
        {
            using var request = CriarRequest(HttpMethod.Post, $"{_prefix}/auth/refresh");
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<RefreshTokenResponse>(json, _jsonOptions);

            if (dto?.Success == true && !string.IsNullOrEmpty(dto.Token))
            {
                _sessao.AtualizarToken(dto.Token);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> Ping()
    {
        try
        {
            using var request = CriarRequest(HttpMethod.Get, $"{_prefix}/ping");
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // ======================== CONFIG ========================

    public async Task<ConfiguracaoPdv?> ObterConfiguracao()
    {
        try
        {
            using var request = CriarRequest(HttpMethod.Get, $"{_prefix}/configuracao");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<ConfiguracaoResponse>(json, _jsonOptions);

            if (dto?.Configuracao == null) return null;

            var c = dto.Configuracao;
            return new ConfiguracaoPdv
            {
                EmitirNfceAuto = c.PdvBoEmitirNfceAuto == "S",
                ExigirCpf = c.PdvBoExigirCpf == "S",
                CasasDecimaisQtd = c.PdvInCasasDecimaisQtd,
                CasasDecimaisPreco = c.PdvInCasasDecimaisPreco,
                ExigirAbertura = c.PdvBoExigirAbertura == "S",
                ImprimirCupom = c.PdvBoImprimirCupom == "S",
                UsarTerminalFixo = c.PdvBoUsarTerminalFixo == "S",
                ModoEntrada = c.PdvStModoEntrada,
                UsarTurno = c.PdvBoUsarTurno == "S",
                AvisoFimTurno = c.PdvInAvisoFimTurno,
                LimiteHorasAberto = c.PdvInLimiteHorasAberto
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<FormaPagamentoSessao>> ObterFormasPagamento()
    {
        try
        {
            using var request = CriarRequest(HttpMethod.Get, $"{_prefix}/formas-pagamento");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<FormasPagamentoResponse>(json, _jsonOptions);

            return dto?.Formas?.Select(f => new FormaPagamentoSessao
            {
                FcbInCodigo = f.FcbInCodigo,
                Nome = f.Nome,
                Descricao = f.Descricao,
                Padrao = f.Padrao == "S",
                PermiteTroco = f.PermiteTroco == "S"
            }).ToList() ?? new();
        }
        catch
        {
            return new();
        }
    }

    // ======================== CAIXA ========================

    public async Task<ResultadoCaixaStatus> ObterStatusCaixa()
    {
        try
        {
            using var request = CriarRequest(HttpMethod.Get, $"{_prefix}/caixa/status");
            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[PDV] ObterStatusCaixa HTTP {(int)response.StatusCode}: {json}");

            // Parse manual para robustez — a API pode variar o formato
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var sucesso = root.TryGetProperty("success", out var sp) && sp.GetBoolean();
            var caixaAberto = root.TryGetProperty("caixa_aberto", out var ca) && ca.GetBoolean();
            int? caixaCodigo = null;

            if (root.TryGetProperty("caixa", out var caixaEl) && caixaEl.ValueKind == JsonValueKind.Object)
            {
                if (caixaEl.TryGetProperty("cai_in_codigo", out var codEl))
                    caixaCodigo = codEl.GetInt32();
            }

            return new ResultadoCaixaStatus
            {
                Sucesso = sucesso,
                CaixaAberto = caixaAberto,
                CaixaCodigo = caixaCodigo
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PDV] ObterStatusCaixa ERRO: {ex.Message}");
            return new ResultadoCaixaStatus { Sucesso = false, Erro = ex.Message };
        }
    }

    public async Task<ConfigTerminal?> ObterConfigTerminal()
    {
        try
        {
            using var request = CriarRequest(HttpMethod.Get, $"{_prefix}/caixa/config-terminal");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<ConfigTerminalResponse>(json, _jsonOptions);

            if (dto == null) return null;

            return new ConfigTerminal
            {
                UsarTerminalFixo = dto.UsarTerminalFixo,
                Terminais = dto.Terminais.Select(t => new TerminalInfo
                {
                    TerInCodigo = t.TerInCodigo,
                    Nome = t.TerStNome,
                    SetorNome = t.SetorNome
                }).ToList(),
                TerminalOperador = dto.TerminalOperador != null
                    ? new TerminalInfo
                    {
                        TerInCodigo = dto.TerminalOperador.TerInCodigo,
                        Nome = dto.TerminalOperador.TerStNome,
                        SetorNome = dto.TerminalOperador.SetorNome
                    }
                    : null
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<ResultadoAbrirCaixa> AbrirCaixa(decimal valorAbertura, int? terInCodigo)
    {
        try
        {
            var body = new AbrirCaixaRequest
            {
                ValorAbertura = valorAbertura,
                TerInCodigo = terInCodigo
            };

            using var request = CriarRequest(HttpMethod.Post, $"{_prefix}/caixa/abrir", body);
            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<AbrirCaixaResponse>(json, _jsonOptions);

            if (dto == null)
                return new ResultadoAbrirCaixa { Sucesso = false, Erro = "Resposta invalida" };

            return new ResultadoAbrirCaixa
            {
                Sucesso = dto.Success,
                Mensagem = dto.Message,
                CaixaCodigo = dto.Caixa?.CaiInCodigo,
                Erro = dto.Error
            };
        }
        catch (Exception ex)
        {
            return new ResultadoAbrirCaixa { Sucesso = false, Erro = ex.Message };
        }
    }

    public async Task<ResultadoFecharCaixa> FecharCaixa(decimal valorFechamento)
    {
        try
        {
            var body = new FecharCaixaRequest { ValorFechamento = valorFechamento };

            using var request = CriarRequest(HttpMethod.Post, $"{_prefix}/caixa/fechar", body);
            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<FecharCaixaResponse>(json, _jsonOptions);

            if (dto == null)
                return new ResultadoFecharCaixa { Sucesso = false, Erro = "Resposta invalida" };

            return new ResultadoFecharCaixa
            {
                Sucesso = dto.Success,
                Mensagem = dto.Message,
                ValorEsperado = dto.Resultado?.ValorEsperado ?? 0,
                ValorFechamento = dto.Resultado?.ValorFechamento ?? 0,
                Diferenca = dto.Resultado?.Diferenca ?? 0,
                Erro = dto.Error
            };
        }
        catch (Exception ex)
        {
            return new ResultadoFecharCaixa { Sucesso = false, Erro = ex.Message };
        }
    }

    public async Task<ResultadoOperacao> RegistrarSangria(decimal valor, string? observacao)
    {
        return await RegistrarMovimento("sangria", valor, observacao);
    }

    public async Task<ResultadoOperacao> RegistrarSuprimento(decimal valor, string? observacao)
    {
        return await RegistrarMovimento("suprimento", valor, observacao);
    }

    private async Task<ResultadoOperacao> RegistrarMovimento(string tipo, decimal valor, string? observacao)
    {
        try
        {
            var body = new SangriaSuprimentoRequest { Valor = valor, Observacao = observacao };

            using var request = CriarRequest(HttpMethod.Post, $"{_prefix}/caixa/{tipo}", body);
            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<SangriaSuprimentoResponse>(json, _jsonOptions);

            return new ResultadoOperacao
            {
                Sucesso = dto?.Success ?? false,
                Mensagem = dto?.Message,
                Erro = dto?.Error
            };
        }
        catch (Exception ex)
        {
            return new ResultadoOperacao { Sucesso = false, Erro = ex.Message };
        }
    }

    public async Task<ResultadoResumoCaixa> ObterResumoCaixa()
    {
        try
        {
            using var request = CriarRequest(HttpMethod.Get, $"{_prefix}/caixa/resumo");
            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<ResumoResponse>(json, _jsonOptions);

            if (dto == null)
                return new ResultadoResumoCaixa { Sucesso = false, Erro = "Resposta invalida" };

            return new ResultadoResumoCaixa
            {
                Sucesso = dto.Success,
                CaixaAberto = dto.CaixaAberto,
                SaldoAtual = dto.SaldoAtual,
                TotalVendas = dto.Totais?.Vendas ?? 0,
                TotalSangrias = dto.Totais?.Sangrias ?? 0,
                TotalSuprimentos = dto.Totais?.Suprimentos ?? 0,
                TotalEstornos = dto.Totais?.Estornos ?? 0,
                ValorAbertura = dto.Caixa?.CaiReVlAbertura ?? 0
            };
        }
        catch (Exception ex)
        {
            return new ResultadoResumoCaixa { Sucesso = false, Erro = ex.Message };
        }
    }

    // ======================== PRODUTO ========================

    public async Task<Produto?> BuscarProdutoPorCodigo(string codigo)
    {
        try
        {
            using var request = CriarRequest(HttpMethod.Get,
                $"{_prefix}/produto/buscar-codigo?codigo={Uri.EscapeDataString(codigo)}");
            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            var dto = JsonSerializer.Deserialize<ProdutoBuscaResponse>(json, _jsonOptions);

            if (dto?.Success != true || dto.Produto == null) return null;

            return MapProduto(dto.Produto);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<Produto>> PesquisarProdutos(string termo, int limite = 20)
    {
        try
        {
            using var request = CriarRequest(HttpMethod.Get,
                $"{_prefix}/produto/buscar?q={Uri.EscapeDataString(termo)}&limit={limite}");
            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API retornou {(int)response.StatusCode}: {json}");

            var dto = JsonSerializer.Deserialize<ProdutosPesquisaResponse>(json, _jsonOptions);

            return dto?.Produtos?.Select(MapProduto).ToList() ?? new();
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao pesquisar produtos: {ex.Message}");
        }
    }

    // ======================== VENDA ========================

    public async Task<ResultadoVenda> FinalizarVendaDireta(List<ItemVendaApi> itens,
        List<ParcelaApi> parcelas, string? cpfNota, decimal? troco, string idempotencyKey)
    {
        try
        {
            var body = new FinalizarVendaRequest
            {
                Itens = itens.Select(i => new ItemVendaApiDTO
                {
                    ProInCodigo = i.ProInCodigo,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario,
                    DescontoPerc = i.DescontoPerc
                }).ToList(),
                Parcelas = parcelas.Select(p => new ParcelaApiDTO
                {
                    FcbInCodigo = p.FcbInCodigo,
                    Valor = p.Valor,
                    Vencimento = p.Vencimento
                }).ToList(),
                CpfNota = cpfNota,
                Troco = troco
            };

            using var request = CriarRequest(HttpMethod.Post, $"{_prefix}/venda/finalizar-direto", body);
            request.Headers.Add("X-Idempotency-Key", idempotencyKey);

            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<FinalizarVendaResponse>(json, _jsonOptions);

            if (dto == null)
                return new ResultadoVenda { Sucesso = false, Erro = "Resposta invalida" };

            return new ResultadoVenda
            {
                Sucesso = dto.Success,
                Mensagem = dto.Message,
                PedidoCodigo = dto.Resultado?.PedInCodigo,
                NfCodigo = dto.Resultado?.NfInCodigo,
                ValorVenda = dto.Resultado?.ValorVenda,
                Troco = dto.Resultado?.Troco,
                NfceStatus = dto.Resultado?.NfceStatus,
                NfceChave = dto.Resultado?.NfceChave,
                FromCache = dto.FromCache ?? false,
                Erro = dto.Error
            };
        }
        catch (Exception ex)
        {
            return new ResultadoVenda { Sucesso = false, Erro = ex.Message };
        }
    }

    // ======================== CLIENTE ========================

    public async Task<List<Cliente>> BuscarClientes(string termo)
    {
        try
        {
            using var request = CriarRequest(HttpMethod.Get,
                $"{_prefix}/cliente/buscar?q={Uri.EscapeDataString(termo)}");
            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API retornou {(int)response.StatusCode}: {json}");

            var dto = JsonSerializer.Deserialize<ClienteBuscaResponse>(json, _jsonOptions);

            return dto?.Clientes?.Select(MapCliente).ToList() ?? new();
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao buscar clientes: {ex.Message}");
        }
    }

    public async Task<Cliente?> BuscarClientePorDocumento(string cpfCnpj)
    {
        try
        {
            using var request = CriarRequest(HttpMethod.Get,
                $"{_prefix}/cliente/buscar?doc={Uri.EscapeDataString(cpfCnpj)}");
            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            var dto = JsonSerializer.Deserialize<ClienteBuscaDocResponse>(json, _jsonOptions);

            if (dto?.Success != true || dto.Cliente == null) return null;

            return MapCliente(dto.Cliente);
        }
        catch
        {
            return null;
        }
    }

    public async Task<ResultadoCadastroCliente> CadastrarCliente(string nome, string? cpfCnpj, string? telefone, string? email)
    {
        try
        {
            var body = new CadastrarClienteRequest
            {
                Nome = nome,
                CpfCnpj = cpfCnpj,
                Telefone = telefone,
                Email = email
            };

            using var request = CriarRequest(HttpMethod.Post, $"{_prefix}/cliente/cadastrar", body);
            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<CadastrarClienteResponse>(json, _jsonOptions);

            if (dto == null)
                return new ResultadoCadastroCliente { Sucesso = false, Erro = "Resposta invalida" };

            var resultado = new ResultadoCadastroCliente
            {
                Sucesso = dto.Success,
                Erro = dto.Error ?? dto.Message,
                Duplicado = dto.Duplicado ?? false
            };

            if (dto.Cliente != null)
                resultado.Cliente = MapCliente(dto.Cliente);

            if (dto.ClienteExistente != null)
                resultado.ClienteExistente = MapCliente(dto.ClienteExistente);

            return resultado;
        }
        catch (Exception ex)
        {
            return new ResultadoCadastroCliente { Sucesso = false, Erro = ex.Message };
        }
    }

    private static Cliente MapCliente(ClienteApiDTO dto) => new()
    {
        Id = dto.AgnInCodigo,
        Nome = dto.AgnStNome,
        CpfCnpj = dto.AgnStCnpjCpf,
        Email = dto.AgnStEmail,
        Telefone = dto.AgnStTelefone
    };

    // ======================== HELPERS ========================

    private HttpRequestMessage CriarRequest(HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);

        if (_sessao.Token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sessao.Token);
        }

        if (body != null)
        {
            var jsonStr = JsonSerializer.Serialize(body, _jsonOptions);
            request.Content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
        }

        return request;
    }

    private static Produto MapProduto(ProdutoApiDTO dto) => new()
    {
        Id = dto.ProInCodigo,
        CodigoBarras = dto.Ean ?? string.Empty,
        CodigoInterno = string.IsNullOrWhiteSpace(dto.Codigo) ? dto.ProInCodigo.ToString() : dto.Codigo,
        Descricao = dto.Descricao,
        UnidadeMedida = dto.Unidade,
        PrecoVenda = dto.Preco,
        EstoqueAtual = dto.Estoque,
        NCM = dto.Ncm ?? string.Empty,
        CFOP = dto.Cfop ?? "5102",
        CST_ICMS = dto.CstIcms ?? string.Empty,
        CSOSN = dto.Csosn ?? string.Empty,
        AliquotaICMS = dto.AliquotaIcms,
        CEST = dto.Cest ?? string.Empty,
        FotoUrl = dto.FotoUrl,
        Ativo = true,
        UltimaAtualizacao = DateTime.Now
    };
}
