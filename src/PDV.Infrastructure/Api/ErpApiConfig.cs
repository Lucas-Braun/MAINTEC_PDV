namespace PDV.Infrastructure.Api;

public class ErpApiConfig
{
    public string BaseUrl { get; set; } = "http://localhost:5000";
    public string ApiVersion { get; set; } = "v1";
    public int TimeoutSeconds { get; set; } = 30;
}
