namespace ProxyNet;

public static class Utils
{
    public static (string, int) ParseProxy(string proxy)
    {
        var parts = proxy.Split(':', 2);
        if (parts.Length != 2 || !int.TryParse(parts[1], out int port))
        {
            throw new ArgumentException("Invalid proxy format. Expected format: host:port");
        }

        return (parts[0], port);
    }
}