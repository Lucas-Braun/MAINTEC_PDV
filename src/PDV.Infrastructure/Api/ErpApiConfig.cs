namespace PDV.Infrastructure.Api;

public class ErpApiConfig
{
    public string BaseUrl { get; set; } = "http://localhost:5000";
    public string ApiVersion { get; set; } = "v1";
    public string ApiPrefix { get; set; } = "/api/v1/pdv";
    public int TimeoutSeconds { get; set; } = 30;
    public int PingIntervalMinutes { get; set; } = 10;
    public int TokenRefreshHours { get; set; } = 12;
}
