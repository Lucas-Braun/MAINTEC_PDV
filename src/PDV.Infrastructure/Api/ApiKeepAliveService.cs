using PDV.Core.Interfaces;

namespace PDV.Infrastructure.Api;

public class ApiKeepAliveService : IDisposable
{
    private readonly IApiClient _apiClient;
    private readonly ErpApiConfig _config;
    private Timer? _pingTimer;
    private Timer? _refreshTimer;
    private bool _disposed;

    public ApiKeepAliveService(IApiClient apiClient, ErpApiConfig config)
    {
        _apiClient = apiClient;
        _config = config;
    }

    public void Iniciar()
    {
        Parar();

        // Ping a cada N minutos
        var pingInterval = TimeSpan.FromMinutes(_config.PingIntervalMinutes);
        _pingTimer = new Timer(async _ =>
        {
            try { await _apiClient.Ping(); }
            catch { /* ignora erro de ping */ }
        }, null, pingInterval, pingInterval);

        // Refresh token a cada N horas
        var refreshInterval = TimeSpan.FromHours(_config.TokenRefreshHours);
        _refreshTimer = new Timer(async _ =>
        {
            try { await _apiClient.RefreshToken(); }
            catch { /* ignora erro de refresh */ }
        }, null, refreshInterval, refreshInterval);
    }

    public void Parar()
    {
        _pingTimer?.Dispose();
        _pingTimer = null;
        _refreshTimer?.Dispose();
        _refreshTimer = null;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Parar();
            _disposed = true;
        }
    }
}
