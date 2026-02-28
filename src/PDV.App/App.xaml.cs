using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PDV.Core.Interfaces;
using PDV.Infrastructure.Api;
using PDV.Infrastructure.Fiscal;
using PDV.Infrastructure.Impressora;
using PDV.Infrastructure.LocalDb;
using PDV.Infrastructure.Services;
using PDV.Infrastructure.TEF;
using PDV.App.Themes;
using PDV.App.ViewModels;
using PDV.App.Views;
using System.IO;
using System.Text;
using System.Windows;

namespace PDV.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public static ServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var splash = new SplashScreen("Assets/splash.png");
        splash.Show(false);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var services = new ServiceCollection();

        // Pasta de dados do PDV
        var dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PDV");
        Directory.CreateDirectory(dataPath);

        // Banco SQLite local
        var dbPath = Path.Combine(dataPath, "pdv.db");
        services.AddDbContext<PdvDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"),
            ServiceLifetime.Transient);

        // Configuracoes persistidas
        var configService = new ConfiguracoesService();
        var configApp = configService.Carregar();

        var apiConfig = new ErpApiConfig
        {
            BaseUrl = "http://localhost:5000",
            TimeoutSeconds = 30
        };

        var impressoraConfig = new ImpressoraConfig
        {
            TipoConexao = configApp.TipoConexao,
            Porta = configApp.Porta,
            BaudRate = configApp.BaudRate,
            IpImpressora = configApp.IpImpressora,
            PortaRede = configApp.PortaRede,
            NomeSpooler = configApp.NomeSpooler,
            ColunasMaximas = configApp.ColunasMaximas,
            NomeEmpresa = configApp.NomeEmpresa,
            CnpjEmpresa = configApp.CnpjEmpresa,
            EnderecoEmpresa = configApp.EnderecoEmpresa
        };

        services.AddSingleton(configService);
        services.AddSingleton(apiConfig);
        services.AddSingleton(impressoraConfig);

        // HttpClient
        services.AddHttpClient<IApiClient, ErpApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiConfig.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(apiConfig.TimeoutSeconds);
        });

        // Servicos de infraestrutura
        services.AddSingleton<IImpressoraService, ImpressoraService>();
        services.AddSingleton<INFCeService, NFCeServiceStub>();
        services.AddSingleton<ITEFService, TEFServiceStub>();

        // Servicos de negocio
        services.AddSingleton<IOperadorService, OperadorService>();
        services.AddTransient<IVendaService, VendaService>();
        services.AddTransient<ICaixaService, CaixaService>();
        services.AddTransient<IProdutoService, ProdutoService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<PDVViewModel>();
        services.AddTransient<AberturaCaixaViewModel>();
        services.AddTransient<PagamentoViewModel>();
        services.AddTransient<ConsultaProdutoViewModel>();
        services.AddTransient<SangriaSuprimentoViewModel>();
        services.AddTransient<FechamentoCaixaViewModel>();
        services.AddTransient<ConfiguracoesViewModel>();
        services.AddTransient<ComprovanteViewModel>();

        // Views
        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();
        Services = _serviceProvider;

        // Cria banco e aplica migrations automaticamente
        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PdvDbContext>();
            db.Database.EnsureCreated();
        }

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        splash.Close(TimeSpan.FromMilliseconds(300));

        // Aplicar tema salvo
        ThemeManager.ApplyTheme(configApp.Tema);

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
