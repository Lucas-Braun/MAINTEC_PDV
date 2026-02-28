# MEINTEC PDV — Estrutura do Projeto

## Status: Solution compilando e rodando (.NET 8.0) — 9 telas, 2 temas, ESCPOS_NET, Instalador, Icone + Splash, GitHub

**Repositorio:** https://github.com/Lucas-Braun/MAINTEC_PDV.git

## Arquitetura Geral

```
PDV.sln
nuget.config                           ✅ Source nuget.org
installer.iss                          ✅ Script Inno Setup (gera setup.exe PT-BR)
│
├── src/
│   ├── PDV.App/                        # Projeto WPF Principal (UI)
│   │   ├── App.xaml / App.xaml.cs      ✅ DI completo + SQLite auto-create +
│   │   │                                 SplashScreen + CodePagesEncodingProvider +
│   │   │                                 ConfiguracoesService + ThemeManager.ApplyTheme
│   │   ├── Themes/
│   │   │   ├── FioriTheme.xaml         ✅ Tema ativo (merge Colors + Controls)
│   │   │   ├── ThemeManager.cs         ✅ Troca de tema em runtime (ApplyTheme/CurrentTheme)
│   │   │   ├── DarkTheme.xaml          ✅ Tema alternativo legado
│   │   │   └── Fiori/
│   │   │       ├── Colors.xaml              ✅ Alias (importa o tema ativo)
│   │   │       ├── Colors.MorningHorizon.xaml ✅ Tema Light (SAP Fiori Horizon)
│   │   │       ├── Colors.EveningHorizon.xaml ✅ Tema Dark (SAP Fiori Horizon Dark)
│   │   │       └── Controls.xaml       ✅ BotaoPDV, TextBoxPDV, DataGridPDV,
│   │   │                                  RadioPDV, CardPDV, BotaoFuncao,
│   │   │                                  ScrollBar, ToolTip, ProgressBar (todos temados)
│   │   ├── Assets/
│   │   │   ├── app.ico                ✅ Icone multi-res (16/32/48/256) — "M" azul #4BA3F5
│   │   │   └── splash.png             ✅ Splash screen 600x340 — fundo #111920, logo MEINTEC
│   │   ├── Controls/
│   │   │   └── FadeContentControl.cs   ✅ Transicao animada entre telas
│   │   ├── Views/
│   │   │   ├── MainWindow.xaml/.cs     ✅ Shell fullscreen + DataTemplates (9 telas)
│   │   │   ├── LoginView.xaml/.cs      ✅ Layout split: branding MEINTEC + formulario
│   │   │   ├── PDVView.xaml/.cs        ✅ Tela principal + F-keys + empty state +
│   │   │   │                              loading overlay + edicao QTD inline
│   │   │   ├── AberturaCaixaView.xaml/.cs    ✅ Card: numero caixa + valor abertura
│   │   │   ├── PagamentoView.xaml/.cs        ✅ 2 colunas: formas pagamento / lista + totais
│   │   │   ├── ConsultaProdutoView.xaml/.cs  ✅ Busca + DataGrid + estoque baixo em vermelho
│   │   │   ├── SangriaSuprimentoView.xaml/.cs ✅ Saldo caixa + valor rapido + historico movimentos
│   │   │   ├── FechamentoCaixaView.xaml/.cs  ✅ Resumo financeiro + contagem + diferenca
│   │   │   ├── ConfiguracoesView.xaml/.cs    ✅ Troca tema + impressora + infos sistema (F10)
│   │   │   └── ComprovanteView.xaml/.cs     ✅ Exibicao de comprovantes TEF
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs        ✅ Navegacao central + callbacks + caixa aberto check
│   │   │   ├── LoginViewModel.cs       ✅ Auth local + ERP async
│   │   │   ├── PDVViewModel.cs         ✅ Fluxo venda + callbacks + edicao QTD inline
│   │   │   ├── AberturaCaixaViewModel.cs     ✅ ICaixaService.AbrirCaixa
│   │   │   ├── PagamentoViewModel.cs         ✅ Split payment, troco, parcelas
│   │   │   ├── ConsultaProdutoViewModel.cs   ✅ IProdutoService.Pesquisar
│   │   │   ├── SangriaSuprimentoViewModel.cs ✅ Sangria/Suprimento + saldo + historico + valor rapido
│   │   │   ├── FechamentoCaixaViewModel.cs   ✅ Resumo + fechar + imprimir relatorio
│   │   │   ├── ConfiguracoesViewModel.cs     ✅ Troca tema + config impressora + teste impressao
│   │   │   └── ComprovanteViewModel.cs      ✅ Exibicao de comprovantes TEF
│   │   └── Converters/
│   │       └── Converters.cs           ✅ BoolToVisibility, InvertBool,
│   │                                      InvertBoolToVisibility, StringToVisibility,
│   │                                      Currency, BoolToStatusColor,
│   │                                      BoolToConnectionText, IsNegative, EstoqueBaixo
│   │
│   ├── PDV.Core/                       # Logica de Negocio (sem dependencias externas)
│   │   ├── Models/
│   │   │   ├── Produto.cs              ✅ Campos fiscais (NCM, CFOP, CST, CSOSN, CEST)
│   │   │   ├── Venda.cs                ✅ SubTotal/ValorTotal computados
│   │   │   ├── ItemVenda.cs            ✅ ValorTotal/ValorICMS computados
│   │   │   ├── Pagamento.cs            ✅ Troco computado, suporte TEF/PIX
│   │   │   ├── Caixa.cs                ✅ SaldoEsperado/Diferenca computados
│   │   │   ├── MovimentoCaixa.cs       ✅
│   │   │   └── Operador.cs             ✅ Perfis: caixa, supervisor, admin
│   │   ├── Interfaces/
│   │   │   ├── IApiClient.cs           ✅
│   │   │   ├── IVendaService.cs        ✅
│   │   │   ├── IProdutoService.cs      ✅
│   │   │   ├── ICaixaService.cs        ✅
│   │   │   ├── IOperadorService.cs     ✅
│   │   │   ├── INFCeService.cs         ✅ + ResultadoNFCe
│   │   │   ├── ITEFService.cs          ✅ + ResultadoTEF
│   │   │   └── IImpressoraService.cs   ✅ + EnviarRaw, VerificarConexao
│   │   └── Enums/
│   │       ├── StatusVenda.cs          ✅ EmAberto, Finalizada, Cancelada, Contingencia
│   │       ├── FormaPagamento.cs       ✅ Dinheiro(1), Credito(3), Debito(4), PIX(17)
│   │       └── TipoMovimentoCaixa.cs   ✅ Abertura, Sangria, Suprimento, Fechamento
│   │
│   ├── PDV.Infrastructure/             # Integracoes Externas
│   │   ├── Api/
│   │   │   ├── ErpApiClient.cs         ✅ Auth JWT, sync produtos, envio vendas
│   │   │   ├── ErpApiConfig.cs         ✅
│   │   │   └── DTOs/
│   │   │       ├── AuthResponse.cs     ✅
│   │   │       ├── ProdutoDTO.cs       ✅
│   │   │       └── VendaDTO.cs         ✅ + ItemVendaDTO, PagamentoDTO
│   │   ├── Fiscal/
│   │   │   └── NFCeServiceStub.cs      ✅ Stub dev (simula emissao autorizada)
│   │   ├── TEF/
│   │   │   └── TEFServiceStub.cs       ✅ Stub dev (simula aprovacao)
│   │   ├── Impressora/
│   │   │   ├── ImpressoraService.cs    ✅ Serial/Rede/Spooler + RawPrinterHelper (P/Invoke)
│   │   │   ├── CupomBuilder.cs         ✅ ESCPOS_NET emitter (EPSON + ByteSplicer)
│   │   │   └── ImpressoraConfig.cs     ✅ NomeEmpresa, CnpjEmpresa, EnderecoEmpresa
│   │   ├── Services/
│   │   │   ├── OperadorService.cs      ✅ Auth via SQLite (seed: admin/caixa1/caixa2, senha: 123)
│   │   │   ├── VendaService.cs         ✅ Numero sequencial YYYYMMDD-NNNN, sync status
│   │   │   ├── CaixaService.cs         ✅ Abertura/fechamento/sangria/suprimento
│   │   │   ├── ProdutoService.cs       ✅ Busca barcode/codigo/termo, upsert cache
│   │   │   └── ConfiguracoesService.cs ✅ Salvar/carregar config JSON (%LocalAppData%/PDV/config.json)
│   │   └── LocalDb/
│   │       ├── PdvDbContext.cs          ✅ 7 tabelas, Fluent API, seed operadores
│   │       └── SyncManager.cs          ⬜ Pendente (fila offline -> ERP)
│   │
│   └── PDV.Shared/                     # Utilitarios Compartilhados
│       ├── Helpers/                    ⬜ Pendente
│       ├── Constants/                  ⬜ Pendente
│       └── Extensions/                 ⬜ Pendente
│
└── PDV.Tests/                          ⬜ Pendente
```

## Fluxo de Navegacao

```
Login → AberturaCaixa (se nao tem caixa aberto) → PDV
                                                    ├─ F2  → Pagamento → PDV (processa pagamento)
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
- **Inputs:** TextBoxPDV, PasswordBoxPDV, RadioPDV
- **DataGrid:** DataGridPDV, FioriColumnHeader, FioriDataGridRow, FioriDataGridCell
- **Texto:** LabelPDV, TituloPDV, SubtituloPDV
- **Layout:** CardPDV
- **Globais:** ScrollBar (thumb arredondado), ToolTip (fundo escuro), ProgressBar (Fiori)

## Melhorias de UX implementadas

| # | Melhoria | Onde |
|---|----------|------|
| 1 | Empty state no DataGrid — "Leia um codigo de barras para iniciar" | PDVView |
| 2 | Loading overlay com ProgressBar quando Processando=true | PDVView |
| 3 | ScrollBar dark com thumb arredondado | Controls.xaml (global) |
| 4 | ToolTip dark com fundo escuro e sombra | Controls.xaml (global) |
| 5 | Botoes de valor rapido (R$50/100/200) | SangriaSuprimentoView |
| 6 | Editar quantidade inline (duplo-clique na coluna QTD) | PDVView + PDVViewModel |
| 7 | Saldo atual do caixa visivel | SangriaSuprimentoView |
| 8 | Estoque baixo (<= 5) em vermelho | ConsultaProdutoView |
| 9 | Historico ultimas 10 movimentacoes | SangriaSuprimentoView |
| 10 | ProgressBar estilizada Fiori | Controls.xaml (global) |
| 11 | Tela de Configuracoes (F10) — tema + impressora + infos | ConfiguracoesView |
| 12 | Login redesenhado — layout split com branding MEINTEC | LoginView |
| 13 | Comprovante TEF — exibicao de comprovantes | ComprovanteView |
| 14 | Config impressora persistida em JSON | ConfiguracoesService |
| 15 | ESCPOS_NET — emitter EPSON substitui bytes hardcoded | CupomBuilder |
| 16 | Icone customizado multi-res ("M" azul) + splash screen MEINTEC | Assets, App.xaml.cs |

## Pacotes NuGet

```
PDV.App:
  ✅ CommunityToolkit.Mvvm 8.4.0
  ✅ Microsoft.Extensions.DependencyInjection
  ✅ Microsoft.Extensions.Hosting

PDV.Core:
  ✅ CommunityToolkit.Mvvm 8.4.0
  ✅ Microsoft.Extensions.DependencyInjection.Abstractions

PDV.Infrastructure:
  ✅ Microsoft.EntityFrameworkCore.Sqlite 8.0.x
  ✅ Microsoft.EntityFrameworkCore.Design 8.0.x
  ✅ ESCPOS_NET 3.0.0
  ✅ Polly
  ✅ System.IO.Ports
  ✅ Microsoft.Extensions.Http
```

## Referencias entre Projetos

```
PDV.App → PDV.Core, PDV.Infrastructure, PDV.Shared
PDV.Infrastructure → PDV.Core, PDV.Shared
PDV.Core → PDV.Shared
```

## DI (Dependency Injection) — App.xaml.cs

```
Singleton:  ConfiguracoesService, ApiConfig, ImpressoraConfig,
            MainViewModel, IOperadorService, IImpressoraService,
            INFCeService (stub), ITEFService (stub)

Transient:  PdvDbContext, LoginViewModel, PDVViewModel,
            AberturaCaixaViewModel, PagamentoViewModel,
            ConsultaProdutoViewModel, SangriaSuprimentoViewModel,
            FechamentoCaixaViewModel, ConfiguracoesViewModel,
            ComprovanteViewModel,
            IVendaService, ICaixaService, IProdutoService

HttpClient: IApiClient → ErpApiClient
```

## Banco de Dados (SQLite)

```
Arquivo: %LocalAppData%/PDV/pdv.db
Auto-criado no startup via EnsureCreated()

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

## Fluxo Principal

```
1. Operador faz login (admin/caixa1/caixa2, senha: 123)
2. Se nao tem caixa aberto → Tela de Abertura (numero + valor inicial)
   Se ja tem caixa aberto → Vai direto pro PDV
3. Loop de vendas:
   a. Le codigo de barras / busca produto (ou F4 para consulta)
   b. Adiciona item a venda (com quantidade, editavel inline)
   c. Finaliza venda (F2) → Tela de Pagamento (split payment)
   d. Processa pagamento (Dinheiro com troco / Cartao via TEF / PIX)
   e. Emite NFC-e → SEFAZ (stub em dev)
   f. Imprime cupom na termica
   g. Sincroniza com ERP via API (background, nao trava)
4. Sangria (F5) / Suprimento (F6) — com saldo visivel e historico
5. Configuracoes (F10) — troca de tema, infos do sistema
6. Fechamento de caixa (F12) → Resumo financeiro + contagem → Volta ao Login
```

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

## Proximos Passos

1. ✅ Criar solution .NET com 4 projetos
2. ✅ LoginView + LoginViewModel + navegacao
3. ✅ LocalDbContext + Services (Venda, Caixa, Produto, Operador)
4. ✅ Stubs NFCe/TEF para desenvolvimento
5. ✅ PDVView + PDVViewModel (tela principal do caixa)
6. ✅ Tema Fiori (Morning Horizon light + Evening Horizon dark)
7. ✅ AberturaCaixaView (abertura de caixa apos login)
8. ✅ PagamentoView (split payment, troco, parcelas)
9. ✅ ConsultaProdutoView (busca + selecao + estoque baixo)
10. ✅ SangriaSuprimentoView (saldo + valor rapido + historico)
11. ✅ FechamentoCaixaView (resumo financeiro + contagem)
12. ✅ Navegacao completa com callbacks (F2/F4/F5/F6/F10/F12/ESC)
13. ✅ ConfiguracoesView (troca de tema + infos sistema)
14. ✅ Melhorias UX (empty state, loading overlay, inline edit, etc.)
15. ✅ Redesign tela de login (layout split com branding MEINTEC)
16. ✅ Controles globais temados (ScrollBar, ToolTip, ProgressBar)
17. ✅ ConfiguracoesService — persistencia JSON de config
18. ✅ ComprovanteView — exibicao de comprovantes TEF
19. ✅ ESCPOS_NET — emitter EPSON no CupomBuilder e ImpressoraService
20. ✅ Instalador Inno Setup — setup.exe self-contained PT-BR (~51 MB)
21. ✅ Icone customizado + splash screen + branding MEINTEC
22. ⬜ Fiscal: NFCeService real (ACBrLib)
21. ⬜ TEF: TEFService real (SiTef/PayGo)
22. ⬜ SyncManager (fila offline → ERP)
23. ⬜ Testes

## Ambiente

- .NET SDK: 8.0.418 (WSL2) / 9.0.101 (Windows - compativel)
- Target: net8.0-windows (WPF)
- OS Dev: Ubuntu 24.04 WSL2 (EnableWindowsTargeting=true)
- OS Run: Windows (PowerShell)
- Build: dotnet build PDV.sln ✅ 0 erros
- Repo: https://github.com/Lucas-Braun/MAINTEC_PDV.git
