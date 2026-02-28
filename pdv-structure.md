# PDV WPF - Estrutura do Projeto

## Status: Solution compilando e rodando (.NET 8.0) — Todas as telas implementadas

## Arquitetura Geral

```
PDV.sln
nuget.config                           ✅ Source nuget.org
│
├── src/
│   ├── PDV.App/                        # Projeto WPF Principal (UI)
│   │   ├── App.xaml / App.xaml.cs      ✅ DI completo + SQLite auto-create
│   │   ├── Themes/
│   │   │   ├── FioriTheme.xaml         ✅ Tema ativo (merge Colors + Controls)
│   │   │   ├── DarkTheme.xaml          ✅ Tema alternativo
│   │   │   └── Fiori/
│   │   │       ├── Colors.xaml         ✅ Paleta SAP Fiori Horizon Dark
│   │   │       └── Controls.xaml       ✅ BotaoPDV, TextBoxPDV, DataGridPDV,
│   │   │                                  RadioPDV, CardPDV, BotaoFuncao, etc.
│   │   ├── Views/
│   │   │   ├── MainWindow.xaml/.cs     ✅ Shell fullscreen + DataTemplates (7 telas)
│   │   │   ├── LoginView.xaml/.cs      ✅ Card login + status ERP
│   │   │   ├── PDVView.xaml/.cs        ✅ Tela principal + F-keys (F2/F4/F5/F6/F9/F12)
│   │   │   ├── AberturaCaixaView.xaml/.cs    ✅ Card: numero caixa + valor abertura
│   │   │   ├── PagamentoView.xaml/.cs        ✅ 2 colunas: formas pagamento / lista + totais
│   │   │   ├── ConsultaProdutoView.xaml/.cs  ✅ Busca + DataGrid produtos + selecionar
│   │   │   ├── SangriaSuprimentoView.xaml/.cs ✅ Card: valor + observacao (sangria E suprimento)
│   │   │   └── FechamentoCaixaView.xaml/.cs  ✅ Resumo financeiro + contagem + diferenca
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs        ✅ Navegacao central + callbacks + caixa aberto check
│   │   │   ├── LoginViewModel.cs       ✅ Auth local + ERP async
│   │   │   ├── PDVViewModel.cs         ✅ Fluxo venda + callbacks navegacao
│   │   │   ├── AberturaCaixaViewModel.cs     ✅ ICaixaService.AbrirCaixa
│   │   │   ├── PagamentoViewModel.cs         ✅ Split payment, troco, parcelas
│   │   │   ├── ConsultaProdutoViewModel.cs   ✅ IProdutoService.Pesquisar
│   │   │   ├── SangriaSuprimentoViewModel.cs ✅ Sangria/Suprimento (mesma VM, tipo define acao)
│   │   │   └── FechamentoCaixaViewModel.cs   ✅ Resumo + fechar + imprimir relatorio
│   │   └── Converters/
│   │       └── Converters.cs           ✅ BoolToVisibility, InvertBool,
│   │                                      StringToVisibility, Currency,
│   │                                      BoolToStatusColor, BoolToConnectionText,
│   │                                      IsNegative
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
│   │   │   └── IImpressoraService.cs   ✅
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
│   │   │   ├── ImpressoraService.cs    ✅ Serial/Rede/Spooler
│   │   │   ├── CupomBuilder.cs         ✅ ESC/POS completo
│   │   │   └── ImpressoraConfig.cs     ✅
│   │   ├── Services/
│   │   │   ├── OperadorService.cs      ✅ Auth via SQLite (seed: admin/caixa1/caixa2, senha: 123)
│   │   │   ├── VendaService.cs         ✅ Numero sequencial YYYYMMDD-NNNN, sync status
│   │   │   ├── CaixaService.cs         ✅ Abertura/fechamento/sangria/suprimento
│   │   │   └── ProdutoService.cs       ✅ Busca barcode/codigo/termo, upsert cache
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

// MainWindow.xaml — 7 DataTemplates:
<DataTemplate DataType="{x:Type vm:LoginViewModel}">         → LoginView
<DataTemplate DataType="{x:Type vm:PDVViewModel}">           → PDVView
<DataTemplate DataType="{x:Type vm:AberturaCaixaViewModel}"> → AberturaCaixaView
<DataTemplate DataType="{x:Type vm:PagamentoViewModel}">     → PagamentoView
<DataTemplate DataType="{x:Type vm:ConsultaProdutoViewModel}"> → ConsultaProdutoView
<DataTemplate DataType="{x:Type vm:SangriaSuprimentoViewModel}"> → SangriaSuprimentoView
<DataTemplate DataType="{x:Type vm:FechamentoCaixaViewModel}"> → FechamentoCaixaView
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
public Action? SolicitarFechamento { get; set; }           // F12 → FechamentoCaixaView
```

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
Singleton:  ApiConfig, ImpressoraConfig, MainViewModel,
            IOperadorService, IImpressoraService,
            INFCeService (stub), ITEFService (stub)

Transient:  PdvDbContext, LoginViewModel, PDVViewModel,
            AberturaCaixaViewModel, PagamentoViewModel,
            ConsultaProdutoViewModel, SangriaSuprimentoViewModel,
            FechamentoCaixaViewModel,
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

## Tema Visual — SAP Fiori Horizon Dark

Definido em `Themes/Fiori/Colors.xaml`:

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

Estilos reutilizaveis em `Controls.xaml`:
- **Botoes:** BotaoPDV, BotaoPositive, BotaoNegative, BotaoCritical, BotaoNeutral, BotaoGhost, BotaoFuncao
- **Inputs:** TextBoxPDV, PasswordBoxPDV, RadioPDV
- **DataGrid:** DataGridPDV, FioriColumnHeader, FioriDataGridRow, FioriDataGridCell
- **Texto:** LabelPDV, TituloPDV, SubtituloPDV
- **Layout:** CardPDV

## Fluxo Principal

```
1. Operador faz login (admin/caixa1/caixa2, senha: 123)
2. Se nao tem caixa aberto → Tela de Abertura (numero + valor inicial)
   Se ja tem caixa aberto → Vai direto pro PDV
3. Loop de vendas:
   a. Le codigo de barras / busca produto (ou F4 para consulta)
   b. Adiciona item a venda (com quantidade)
   c. Finaliza venda (F2) → Tela de Pagamento (split payment)
   d. Processa pagamento (Dinheiro com troco / Cartao via TEF / PIX)
   e. Emite NFC-e → SEFAZ (stub em dev)
   f. Imprime cupom na termica
   g. Sincroniza com ERP via API (background, nao trava)
4. Sangria (F5) / Suprimento (F6) durante o dia
5. Fechamento de caixa (F12) → Resumo financeiro + contagem → Volta ao Login
```

## Como Rodar

```bash
cd C:\pdv
dotnet build PDV.sln
dotnet run --project src\PDV.App
```

## Proximos Passos

1. ✅ Criar solution .NET com 4 projetos
2. ✅ LoginView + LoginViewModel + navegacao
3. ✅ LocalDbContext + Services (Venda, Caixa, Produto, Operador)
4. ✅ Stubs NFCe/TEF para desenvolvimento
5. ✅ PDVView + PDVViewModel (tela principal do caixa)
6. ✅ Tema Fiori Dark (Colors + Controls)
7. ✅ AberturaCaixaView (abertura de caixa apos login)
8. ✅ PagamentoView (split payment, troco, parcelas)
9. ✅ ConsultaProdutoView (busca + selecao de produto)
10. ✅ SangriaSuprimentoView (sangria e suprimento, mesma view)
11. ✅ FechamentoCaixaView (resumo financeiro + contagem)
12. ✅ Navegacao completa com callbacks (F2/F4/F5/F6/F12/ESC)
13. ⬜ Fiscal: NFCeService real (ACBrLib)
14. ⬜ TEF: TEFService real (SiTef/PayGo)
15. ⬜ SyncManager (fila offline → ERP)
16. ⬜ Testes

## Ambiente

- .NET SDK: 8.0.418 (WSL2) / 9.0.101 (Windows - compativel)
- Target: net8.0-windows (WPF)
- OS Dev: Ubuntu 24.04 WSL2 (EnableWindowsTargeting=true)
- OS Run: Windows (PowerShell)
- Build: dotnet build PDV.sln ✅ 0 erros, 0 warnings
