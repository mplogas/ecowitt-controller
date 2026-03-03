namespace Ecowitt.Controller.Model.Message.Config;

public class HttpConfig
{
    public List<HttpHost> Hosts { get; set; } = new List<HttpHost>();
    public int PollingInterval { get; set; } = 5;
    public bool AutoDiscovery { get; set; }
}

public class HttpHost
{
    public HttpHost()
    {
        
    }
    public HttpHost(string host)
    {
        Host = host;
    }

    public HttpHost(string host, string user, string password)
    {
        Host = host;
        User = user;
        Password = password;
    }

    public string Host { get; set; }
    public int Port { get; set; } = 80;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Protocol { get; set; } = "http"; // or "https"
    public string BaseUrl => (Protocol == "http" && Port == 80) || (Protocol == "https" && Port == 443)
        ? $"{Protocol}://{Host}"
        : $"{Protocol}://{Host}:{Port}";
}
