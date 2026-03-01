# MEINTEC PDV — Estrutura do Projeto

## Status: API-First — .NET 8.0, 9 telas, 2 temas, ESCPOS_NET, Instalador, Icone + Splash

**Repositorio:** https://github.com/Lucas-Braun/MAINTEC_PDV.git

## Arquitetura Geral (API-First)

```
ViewModel → Service → IApiClient (API real) + ISessaoService (estado em memoria)
                         ↓
                   ErpApiClient → POST/GET /api/v1/pdv/*
                         ↓
                   ERP MEINTEC (servidor)
```

- **ISessaoService**: singleton in-memory (token, usuario, empresa, filial, config, formas pagamento, terminal)
- **IApiClient**: 17 metodos retornando objetos de dominio (nao DTOs)
- **SQLite/PdvDbContext**: permanece como cache opcional, fora do caminho critico
- **NFC-e/Estoque/Precos**: servidor cuida de tudo via `POST /venda/finalizar-direto`

```
PDV.sln
nuget.config                           Source nuget.org
installer.iss                          Script Inno Setup (gera setup.exe PT-BR)
README.md                              Documentacao da API MEINTEC (39 endpoints)
│
├── src/
│   ├── PDV.App/                        # Projeto WPF Principal (UI)
│   │   ├── App.xaml / App.xaml.cs      DI completo + ISessaoService singleton +
│   │   │                                 IApiClient via HttpClientFactory +
│   │   │                                 ApiKeepAliveService + SplashScreen +
│   │   │                                 CodePagesEncodingProvider + ThemeManager
│   │   ├── Themes/
│   │   │   ├── FioriTheme.xaml         Tema ativo (merge Colors + Controls)
│   │   │   ├── ThemeManager.cs         Troca de tema em runtime (ApplyTheme/CurrentTheme)
│   │   │   ├── DarkTheme.xaml          Tema alternativo legado
│   │   │   └── Fiori/
│   │   │       ├── Colors.xaml              Alias (importa o tema ativo)
│   │   │       ├── Colors.MorningHorizon.xaml  Tema Light (SAP Fiori Horizon)
│   │   │       ├── Colors.EveningHorizon.xaml  Tema Dark (SAP Fiori Horizon Dark)
│   │   │       └── Controls.xaml       BotaoPDV, TextBoxPDV, DataGridPDV,
│   │   │                                  RadioPDV, CardPDV, BotaoFuncao,
│   │   │                                  ScrollBar, ToolTip, ProgressBar (todos temados)
│   │   ├── Assets/
│   │   │   ├── app.ico                Icone multi-res (16/32/48/256) — "M" azul #4BA3F5
│   │   │   └── splash.png             Splash screen 600x340 — fundo #111920, logo MEINTEC
│   │   ├── Controls/
│   │   │   └── FadeContentControl.cs   Transicao animada entre telas (fade 250ms)
│   │   ├── Views/
│   │   │   ├── MainWindow.xaml/.cs     Shell fullscreen + DataTemplates (9 telas)
│   │   │   ├── LoginView.xaml/.cs      Layout split: branding MEINTEC + formulario
│   │   │   │                              (sem indicador ERP, login obrigatorio via API)
│   │   │   ├── PDVView.xaml/.cs        Tela principal + F-keys + empty state +
│   │   │   │                              loading overlay + edicao QTD inline
│   │   │   ├── AberturaCaixaView.xaml/.cs    ComboBox de terminais (ou terminal fixo) + valor
│   │   │   ├── PagamentoView.xaml/.cs        ListBox dinamico de formas (da API via sessao),
│   │   │   │                                    troco/valor recebido condicionais por PermiteTroco
│   │   │   ├── ConsultaProdutoView.xaml/.cs  Busca + DataGrid + estoque baixo em vermelho
│   │   │   ├── SangriaSuprimentoView.xaml/.cs Valor + observacao + botoes rapidos
│   │   │   ├── FechamentoCaixaView.xaml/.cs  Resumo financeiro da API + fechar
│   │   │   ├── ConfiguracoesView.xaml/.cs    Tema + impressora + URL API + info sistema
│   │   │   └── ComprovanteView.xaml/.cs     Exibicao de comprovante de venda
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs        Navegacao central + callbacks + ApiKeepAliveService
│   │   │   │                              start/stop + checa caixa via IApiClient
│   │   │   ├── LoginViewModel.cs       Auth via IOperadorService (que chama API + carrega
│   │   │   │                              config/formas/terminal em paralelo)
│   │   │   ├── PDVViewModel.cs         Busca produto via API, FinalizarVendaDireta com
│   │   │   │                              idempotency key, imprime local
│   │   │   ├── AberturaCaixaViewModel.cs     Terminais de ISessaoService + AbrirCaixa(val, ter)
│   │   │   ├── PagamentoViewModel.cs         Formas dinamicas da sessao, PermiteTroco,
│   │   │   │                                    FcbInCodigo no Pagamento
│   │   │   ├── ConsultaProdutoViewModel.cs   IProdutoService.Pesquisar (API)
│   │   │   ├── SangriaSuprimentoViewModel.cs Sangria/Suprimento via ICaixaService (API)
│   │   │   ├── FechamentoCaixaViewModel.cs   Resumo via ObterResumoCaixa (API) + FecharCaixa
│   │   │   ├── ConfiguracoesViewModel.cs     Tema + impressora + ApiUrl + info da sessao
│   │   │   └── ComprovanteViewModel.cs      NomeFormaPagamento busca na sessao
│   │   └── Converters/
│   │       └── Converters.cs           BoolToVisibility, InvertBool,
│   │                                      InvertBoolToVisibility, StringToVisibility,
│   │                                      Currency, BoolToStatusColor,
│   │                                      BoolToConnectionText, IsNegative, EstoqueBaixo
│   │
│   ├── PDV.Core/                       # Logica de Negocio (sem dependencias externas)
│   │   ├── Models/
│   │   │   ├── Sessao.cs              UsuarioSessao, EmpresaSessao, FilialSessao,
│   │   │   │                             ConfiguracaoPdv, FormaPagamentoSessao,
│   │   │   │                             ConfigTerminal, TerminalInfo
│   │   │   ├── Produto.cs              Campos fiscais (NCM, CFOP, CST, CSOSN, CEST)
│   │   │   ├── Venda.cs                SubTotal/ValorTotal computados
│   │   │   ├── ItemVenda.cs            ValorTotal/ValorICMS computados
│   │   │   ├── Pagamento.cs            FormaPagamento enum + FcbInCodigo int,
│   │   │   │                             Troco computado, suporte TEF/PIX
│   │   │   ├── Caixa.cs                SaldoEsperado/Diferenca computados
│   │   │   ├── MovimentoCaixa.cs
│   │   │   └── Operador.cs             Perfis: caixa, supervisor, admin
│   │   ├── Interfaces/
│   │   │   ├── IApiClient.cs           17 metodos: Login, RefreshToken, Ping,
│   │   │   │                             ObterConfiguracao, ObterFormasPagamento,
│   │   │   │                             ObterStatusCaixa, ObterConfigTerminal,
│   │   │   │                             AbrirCaixa, FecharCaixa, RegistrarSangria,
│   │   │   │                             RegistrarSuprimento, ObterResumoCaixa,
│   │   │   │                             BuscarProdutoPorCodigo, PesquisarProdutos,
│   │   │   │                             FinalizarVendaDireta
│   │   │   │                             + Result types (ResultadoLogin, ResultadoCaixaStatus,
│   │   │   │                             ResultadoAbrirCaixa, ResultadoFecharCaixa,
│   │   │   │                             ResultadoOperacao, ResultadoResumoCaixa,
│   │   │   │                             ResultadoVenda, ItemVendaApi, ParcelaApi)
│   │   │   ├── ISessaoService.cs       Token, Autenticado, Usuario, Empresa, Filial,
│   │   │   │                             Filiais, Configuracao, FormasPagamento,
│   │   │   │                             ConfigTerminal + DefinirSessao/AtualizarToken/Limpar
│   │   │   ├── ICaixaService.cs        AbrirCaixa(valor, terInCodigo), ObterStatusCaixa,
│   │   │   │                             RegistrarSangria/Suprimento, FecharCaixa,
│   │   │   │                             ObterResumoCaixa — retorna tipos Result
│   │   │   ├── IProdutoService.cs      BuscarPorCodigoBarras, BuscarPorCodigo,
│   │   │   │                             Pesquisar, AtualizarCacheLocal
│   │   │   ├── IVendaService.cs        SalvarVenda, CriarVenda, CancelarVenda (compat)
│   │   │   ├── IOperadorService.cs     Autenticar (via API), Logout (limpa sessao)
│   │   │   ├── INFCeService.cs         EmitirNFCe, CancelarNFCe (stub — servidor cuida)
│   │   │   ├── ITEFService.cs          ProcessarPagamento (TEF local futuro)
│   │   │   └── IImpressoraService.cs   ImprimirCupom, EnviarRaw, VerificarConexao
│   │   └── Enums/
│   │       ├── StatusVenda.cs          EmAberto, Finalizada, Cancelada, Contingencia
│   │       ├── FormaPagamento.cs       Dinheiro(1), Credito(3), Debito(4), PIX(17),
│   │       │                             ValeAlimentacao(10), Outros(99)
│   │       └── TipoMovimentoCaixa.cs   Abertura, Sangria, Suprimento, Fechamento
│   │
│   ├── PDV.Infrastructure/             # Integracoes Externas
│   │   ├── Api/
│   │   │   ├── ErpApiClient.cs         Implementa IApiClient — chamadas HTTP a
│   │   │   │                             /api/v1/pdv/*, token via ISessaoService,
│   │   │   │                             mapeamento DTO→domain
│   │   │   ├── ErpApiConfig.cs         BaseUrl, ApiVersion, ApiPrefix="/api/v1/pdv",
│   │   │   │                             TimeoutSeconds=30, PingIntervalMinutes=10,
│   │   │   │                             TokenRefreshHours=12
│   │   │   ├── ApiKeepAliveService.cs  Ping a cada 10min + refresh token a cada 12h
│   │   │   │                             Iniciar/Parar/Dispose
│   │   │   └── DTOs/
│   │   │       ├── ApiResponse.cs      ApiResponse<T> generico (Success, Message, Error, Code)
│   │   │       ├── AuthResponse.cs     LoginResponse, UsuarioDTO, EmpresaDTO, FilialDTO,
│   │   │       │                         RefreshTokenResponse, SessaoResponse
│   │   │       ├── ProdutoDTO.cs       ProdutoBuscaResponse, ProdutosPesquisaResponse,
│   │   │       │                         ProdutoApiDTO (pro_in_codigo, etc.)
│   │   │       ├── VendaDTO.cs         FinalizarVendaRequest, ItemVendaApiDTO, ParcelaApiDTO,
│   │   │       │                         FinalizarVendaResponse, ResultadoVendaDTO
│   │   │       ├── CaixaDTO.cs         CaixaStatusResponse, AbrirCaixaRequest/Response,
│   │   │       │                         FecharCaixaRequest/Response, SangriaSuprimentoReq/Resp,
│   │   │       │                         ResumoResponse, ConfigTerminalResponse, TerminalDTO
│   │   │       └── ConfigDTO.cs        ConfiguracaoResponse, ConfiguracaoPdvDTO,
│   │   │                                 FormasPagamentoResponse, FormaPagamentoApiDTO,
│   │   │                                 PingResponse
│   │   ├── Fiscal/
│   │   │   └── NFCeServiceStub.cs      Stub (servidor cuida de NFC-e via finalizar-direto)
│   │   ├── TEF/
│   │   │   └── TEFServiceStub.cs       Stub (TEF local futuro)
│   │   ├── Impressora/
│   │   │   ├── ImpressoraService.cs    Serial/Rede/Spooler + RawPrinterHelper (P/Invoke)
│   │   │   ├── CupomBuilder.cs         ESCPOS_NET emitter (EPSON + ByteSplicer)
│   │   │   └── ImpressoraConfig.cs     NomeEmpresa, CnpjEmpresa, EnderecoEmpresa
│   │   ├── Services/
│   │   │   ├── SessaoService.cs        Implementa ISessaoService, singleton in-memory
│   │   │   ├── OperadorService.cs      Autenticar via IApiClient.Login + carrega
│   │   │   │                             config/formas/terminal em paralelo via ISessaoService
│   │   │   ├── CaixaService.cs         Wrapper fino de IApiClient (todas operacoes de caixa)
│   │   │   ├── ProdutoService.cs       Delega tudo para IApiClient (busca/pesquisa)
│   │   │   ├── VendaService.cs         IVendaService (mantido para compat, acessa SQLite)
│   │   │   └── ConfiguracoesService.cs ConfiguracoesApp com ApiUrl, persistido em JSON
│   │   │                                 (%LOCALAPPDATA%\PDV\configuracoes.json)
│   │   └── LocalDb/
│   │       ├── PdvDbContext.cs          7 tabelas, EF Core SQLite (cache opcional)
│   │       └── SyncManager.cs          Pendente (fila offline → ERP)
│   │
│   └── PDV.Shared/                     # Utilitarios Compartilhados (pendente)
│
└── PDV.Tests/                          Pendente
```

## Fluxo de Navegacao

```
Login → AberturaCaixa (se nao tem caixa aberto) → PDV
                                                    ├─ F2  → Pagamento → Comprovante → PDV
                                                    ├─ F4  → ConsultaProduto → PDV (insere produto)
                                                    ├─ F5  → SangriaSuprimento(Sangria) → PDV
                                                    ├─ F6  → SangriaSuprimento(Suprimento) → PDV
                                                    ├─ F9  → Cancela venda atual
                                                    ├─ F10 → Configuracoes → PDV (ESC)
                                                    ├─ Del → Remove item selecionado
                                                    ├─ F12 → FechamentoCaixa → Login
                                                    └─ ESC → Fechar app (so em Login/PDV)
```

Sub-telas usam ESC para voltar ao PDV (sem fechar o app).

## Padrao de Navegacao (DataTemplate + Callbacks)

Toda navegacao e feita via troca de ViewModel na propriedade `MainViewModel.TelaAtual`.
O `MainWindow.xaml` tem DataTemplates que mapeiam cada ViewModel para sua View:

```csharp
// MainViewModel.cs
[ObservableProperty]
private ObservableObject? _telaAtual;

// MainWindow.xaml — 9 DataTemplates:
<DataTemplate DataType="{x:Type vm:LoginViewModel}">              → LoginView
<DataTemplate DataType="{x:Type vm:PDVViewModel}">                → PDVView
<DataTemplate DataType="{x:Type vm:AberturaCaixaViewModel}">      → AberturaCaixaView
<DataTemplate DataType="{x:Type vm:PagamentoViewModel}">          → PagamentoView
<DataTemplate DataType="{x:Type vm:ConsultaProdutoViewModel}">    → ConsultaProdutoView
<DataTemplate DataType="{x:Type vm:SangriaSuprimentoViewModel}">  → SangriaSuprimentoView
<DataTemplate DataType="{x:Type vm:FechamentoCaixaViewModel}">    → FechamentoCaixaView
<DataTemplate DataType="{x:Type vm:ConfiguracoesViewModel}">      → ConfiguracoesView
<DataTemplate DataType="{x:Type vm:ComprovanteViewModel}">        → ComprovanteView
```

### Callbacks entre ViewModels

Cada sub-tela expoe callbacks (Action) que o MainViewModel configura antes de navegar:

```csharp
// Sub-tela expoe callbacks
public Action? CaixaAberto { get; set; }
public Action? Cancelado { get; set; }

// MainViewModel configura e navega
var vm = _services.GetRequiredService<AberturaCaixaViewModel>();
vm.CaixaAberto = () => NavegarParaPDV();
TelaAtual = vm;
```

O PDVViewModel expoe callbacks para navegacao sem depender do MainViewModel:

```csharp
public Action<decimal>? SolicitarPagamento { get; set; }  // F2 → PagamentoView
public Action? SolicitarConsulta { get; set; }             // F4 → ConsultaProdutoView
public Action? SolicitarSangria { get; set; }              // F5 → SangriaSuprimentoView
public Action? SolicitarSuprimento { get; set; }           // F6 → SangriaSuprimentoView
public Action? SolicitarConfiguracoes { get; set; }        // F10 → ConfiguracoesView
public Action? SolicitarFechamento { get; set; }           // F12 → FechamentoCaixaView
public Action<Venda>? VendaFinalizada { get; set; }        // → ComprovanteView
```

## DI (Dependency Injection) — App.xaml.cs

```
Singleton:  ConfiguracoesService, ErpApiConfig, ImpressoraConfig,
            ISessaoService (SessaoService),
            ApiKeepAliveService,
            IOperadorService (OperadorService),
            IImpressoraService (ImpressoraService),
            INFCeService (NFCeServiceStub), ITEFService (TEFServiceStub),
            MainViewModel, MainWindow

Transient:  PdvDbContext,
            ICaixaService (CaixaService),
            IProdutoService (ProdutoService),
            IVendaService (VendaService),
            LoginViewModel, PDVViewModel,
            AberturaCaixaViewModel, PagamentoViewModel,
            ConsultaProdutoViewModel, SangriaSuprimentoViewModel,
            FechamentoCaixaViewModel, ConfiguracoesViewModel,
            ComprovanteViewModel

HttpClient: IApiClient → ErpApiClient (timeout de ErpApiConfig)
```

## Fluxo Completo de Venda (API-First)

```
1. Login
   OperadorService.Autenticar → IApiClient.Login
   → Popula ISessaoService (token, usuario, empresa, filial, filiais)
   → Paralelo: ObterConfiguracao + ObterFormasPagamento + ObterConfigTerminal
   → Inicia ApiKeepAliveService

2. Abertura de Caixa
   AberturaCaixaVM carrega terminais de ISessaoService.ConfigTerminal
   → ICaixaService.AbrirCaixa(valor, terInCodigo) → API POST /caixa/abrir

3. Loop de Venda
   a. Escanear produto → IProdutoService.BuscarPorCodigoBarras → API GET /produto/buscar-codigo
   b. Adicionar ao carrinho (MEMORIA LOCAL — ObservableCollection<ItemVenda>)
   c. F2 → Pagamento → seleciona forma (dinamica da API), valor, troco

4. Finalizar Venda
   a. TEF local (se cartao)
   b. IApiClient.FinalizarVendaDireta(itens, parcelas, cpf, troco, uuid)
      → API POST /venda/finalizar-direto com X-Idempotency-Key
      → Servidor: recalcula precos, baixa estoque, emite NFC-e
   c. Imprime cupom local (se config)
   d. Navega para comprovante

5. Operacoes de Caixa
   Sangria: ICaixaService.RegistrarSangria → API POST /caixa/sangria
   Suprimento: ICaixaService.RegistrarSuprimento → API POST /caixa/suprimento

6. Fechamento de Caixa
   CarregarResumoCaixa → API GET /caixa/resumo
   FecharCaixa → API POST /caixa/fechar

7. Keep-alive (background)
   Ping: GET /ping a cada 10 min
   Refresh: POST /auth/refresh a cada 12h

8. Logout
   OperadorService.Logout → ISessaoService.Limpar + ApiKeepAliveService.Parar
```

## Temas Visuais — SAP Fiori Horizon

O sistema suporta 2 temas, trocaveis em runtime via `ThemeManager.ApplyTheme()`:

| Tema | Arquivo | Tipo |
|------|---------|------|
| Morning Horizon | `Colors.MorningHorizon.xaml` | Light |
| Evening Horizon | `Colors.EveningHorizon.xaml` | Dark |

Troca feita pela tela de Configuracoes (F10) via RadioButtons.

### Paleta Evening Horizon (Dark)

| Cor | Hex | Uso |
|-----|-----|-----|
| Background | `#111920` | Fundo geral |
| Shell | `#1A2733` | Header/footer |
| Surface | `#243342` | Cards |
| SurfaceLight | `#2C3E50` | Inputs, hover |
| Primary | `#4BA3F5` | Acoes principais |
| Positive | `#4CAF50` | Confirmar, sucesso |
| Negative | `#EF5350` | Cancelar, erro |
| Critical | `#FF9800` | Atencao (sangria) |
| Text | `#ECF0F4` | Texto principal |
| TextSecondary | `#7E91A5` | Labels, hints |

### Estilos reutilizaveis (Controls.xaml)

- **Botoes:** BotaoPDV, BotaoPositive, BotaoNegative, BotaoCritical, BotaoNeutral, BotaoGhost, BotaoFuncao
- **Inputs:** TextBoxPDV, PasswordBoxPDV, RadioPDV, ComboBoxPDV
- **DataGrid:** DataGridPDV, FioriColumnHeader, FioriDataGridRow, FioriDataGridCell
- **Texto:** LabelPDV, TituloPDV, SubtituloPDV
- **Layout:** CardPDV
- **Globais:** ScrollBar (thumb arredondado), ToolTip (fundo escuro), ProgressBar (Fiori)

## Configuracoes Persistidas (configuracoes.json)

Arquivo: `%LOCALAPPDATA%\PDV\configuracoes.json`

```
Tema: "EveningHorizon"
TipoConexao: "USB"
Porta: "COM3"
BaudRate: 9600
IpImpressora: null
PortaRede: 9100
NomeSpooler: null
ColunasMaximas: 48
NomeEmpresa: "SUA EMPRESA LTDA"
CnpjEmpresa: "00.000.000/0001-00"
EnderecoEmpresa: "Rua Exemplo, 123 - Cidade/UF"
ApiUrl: "http://localhost:5000"
```

## Banco de Dados (SQLite — cache opcional)

```
Arquivo: %LocalAppData%/PDV/pdv.db
Auto-criado no startup via EnsureCreated()
Fora do caminho critico (API e a fonte primaria)

Tabelas:
  - produtos        (indice: codigo_barras, codigo_interno)
  - vendas          (indice: numero_venda unique, sincronizado_erp)
  - itens_venda     (FK: venda_id cascade)
  - pagamentos      (FK: venda_id cascade)
  - caixas
  - movimentos_caixa (FK: caixa_id cascade)
  - operadores      (indice: login unique, seed: 3 operadores)

Propriedades computadas ignoradas no mapeamento:
  Venda.SubTotal, Venda.ValorTotal, ItemVenda.ValorTotal,
  ItemVenda.ValorICMS, Pagamento.Troco, Caixa.SaldoEsperado,
  Caixa.Diferenca, Caixa.Aberto
```

## Pacotes NuGet

```
PDV.App:
  CommunityToolkit.Mvvm 8.4.0
  Microsoft.Extensions.DependencyInjection
  Microsoft.Extensions.Hosting

PDV.Core:
  CommunityToolkit.Mvvm 8.4.0
  Microsoft.Extensions.DependencyInjection.Abstractions

PDV.Infrastructure:
  Microsoft.EntityFrameworkCore.Sqlite 8.0.x
  Microsoft.EntityFrameworkCore.Design 8.0.x
  ESCPOS_NET 3.0.0
  Polly
  System.IO.Ports
  Microsoft.Extensions.Http
```

## Referencias entre Projetos

```
PDV.App → PDV.Core, PDV.Infrastructure, PDV.Shared
PDV.Infrastructure → PDV.Core, PDV.Shared
PDV.Core → PDV.Shared
```

## Melhorias de UX implementadas

| # | Melhoria | Onde |
|---|----------|------|
| 1 | Empty state no DataGrid — "Leia um codigo de barras para iniciar" | PDVView |
| 2 | Loading overlay com ProgressBar quando Processando=true | PDVView |
| 3 | ScrollBar dark com thumb arredondado | Controls.xaml (global) |
| 4 | ToolTip dark com fundo escuro e sombra | Controls.xaml (global) |
| 5 | Botoes de valor rapido (R$50/100/200) | SangriaSuprimentoView |
| 6 | Editar quantidade inline (duplo-clique na coluna QTD) | PDVView + PDVViewModel |
| 7 | Estoque baixo (<= 5) em vermelho | ConsultaProdutoView |
| 8 | ProgressBar estilizada Fiori | Controls.xaml (global) |
| 9 | Tela de Configuracoes (F10) — tema + impressora + API URL + infos | ConfiguracoesView |
| 10 | Login redesenhado — layout split com branding MEINTEC | LoginView |
| 11 | Comprovante de venda | ComprovanteView |
| 12 | Config impressora persistida em JSON | ConfiguracoesService |
| 13 | ESCPOS_NET — emitter EPSON substitui bytes hardcoded | CupomBuilder |
| 14 | Icone customizado multi-res ("M" azul) + splash screen MEINTEC | Assets, App.xaml.cs |
| 15 | Formas de pagamento dinamicas da API | PagamentoView |
| 16 | Troco em tempo real por forma (PermiteTroco) | PagamentoView |
| 17 | Selecao de terminal na abertura de caixa | AberturaCaixaView |
| 18 | FadeContentControl — transicao fade 250ms entre telas | Controls |
| 19 | Relogio ao vivo no header (DispatcherTimer 1s, fonte Consolas) | PDVView |
| 20 | Total grande 42px com "R$" separado + contagem itens | PDVView |
| 21 | StatusDotOnline — bolinhas pulsando (opacity 1→0.3 em loop 2s) | PDVView |
| 22 | Overlay de sucesso verde semi-transparente apos venda | PDVView |
| 23 | BotaoFuncao com sombra + ScaleTransform 1→1.06 animado (150ms) | Controls.xaml |
| 24 | Ultimo item com destaque (card + valor 28px azul bold) | PDVView |
| 25 | Tooltips nos botoes F2-F12 | PDVView |

## Como Rodar

```bash
cd C:\pdv
dotnet build PDV.sln
dotnet run --project src\PDV.App
```

## Como Gerar o Instalador

```bash
# 1. Publicar self-contained
dotnet publish src/PDV.App/PDV.App.csproj -c Release -r win-x64 --self-contained -o ./publish

# 2. Compilar installer.iss (requer Inno Setup instalado)
"C:\InnoSetup\ISCC.exe" installer.iss

# Saida: installer_output/MEINTEC_PDV_Setup_1.0.0.exe (~51 MB)
```

## Ambiente

- .NET SDK: 8.0.418 (WSL2) / 9.0.101 (Windows - compativel)
- Target: net8.0-windows (WPF)
- OS Dev: Ubuntu 24.04 WSL2 (EnableWindowsTargeting=true)
- OS Run: Windows (PowerShell)
- Build: dotnet build PDV.sln — 0 erros
- Repo: https://github.com/Lucas-Braun/MAINTEC_PDV.git
