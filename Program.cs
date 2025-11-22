using ConsoleAppFramework;
using System.Net.Sockets;
using System.Text;

await ConsoleApp.RunAsync(args,
    static async (ProxyType proxyType, string proxy, string targetHost, int targetPort, CancellationToken cancellationToken) => {
      if (proxyType != ProxyType.Http) {
        throw new NotSupportedException("Only HTTP proxy type is supported.");
      }

      var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

      try {
        var stdin = Console.OpenStandardInput();
        var stdout = Console.OpenStandardOutput();

        var (proxyHost, proxyPort) = ProxyNet.Utils.ParseProxy(proxy);

        using var proxyClient = new TcpClient(proxyHost, proxyPort);
        using var proxyStream = proxyClient.GetStream();

        {
          var connectRequest = $"CONNECT {targetHost}:{targetPort} HTTP/1.1\r\nHost: {targetHost}:{targetPort}\r\n\r\n";
          var requestBytes = Encoding.ASCII.GetBytes(connectRequest);
          await proxyStream.WriteAsync(requestBytes, 0, requestBytes.Length, cts.Token);
        }

        {
          var reader = new StreamReader(proxyStream, Encoding.ASCII);
          var responseLine = await reader.ReadLineAsync(cts.Token);
          if (responseLine == null || !responseLine.Contains("200")) {
            throw new Exception("Proxy CONNECT failed: " + responseLine);
          }

          // Skip remaining headers
          string? line;
          while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync(cts.Token))) { }
        }

        var remoteToStdout = proxyStream.CopyToAsync(stdout, cts.Token);
        var stdinToRemote = stdin.CopyToAsync(proxyStream, cts.Token);

        await Task.WhenAll(remoteToStdout, stdinToRemote);
      } catch (TaskCanceledException) {
        // Ignore
      }
    });

public enum ProxyType {
  Http,
}
