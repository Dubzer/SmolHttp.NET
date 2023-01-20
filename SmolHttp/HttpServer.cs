using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SmolHttp;

public class HttpServer
{
    private readonly TcpListener _server;
    private readonly FileExtensionContentTypeProvider _fileExtensionProvider = new();
    private readonly RotatableIndex _rotatableIndex = new();
    private const string BadRequestResponse = "HTTP/1.1 400 Bad Request\r\nContent-Length: 0\r\n\r\n";
    public HttpServer(IPEndPoint endPoint)
    {
        _server = new TcpListener(endPoint);
    }

    public async Task StartReceiving()
    {
        _server.Start();

        while (true)
        {
            var client = await _server.AcceptTcpClientAsync();

            var connectionIndex = _rotatableIndex.GetIndex();
            Console.WriteLine("New client connected: " + connectionIndex);

            var stream = client.GetStream();
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessRequest(client, stream, connectionIndex);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Environment.Exit(-1);
                }
            });
        }
    }

    async Task ProcessRequest(TcpClient client, NetworkStream stream, uint index)
    {
        using var streamReader = new StreamReader(stream);
        try
        {
            uint delayedTimes = 0;
            while (client.Client.Available == 0)
            {
                if (++delayedTimes == 100)
                    throw new TimeoutException("timeout connection");

                await Task.Delay(100).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            await stream.DisposeAsync();
            Console.WriteLine($"Closed connection {index}: " + e.Message);
            return;
        }

        var rentedArray = ArrayPool<char>.Shared.Rent(1024);
        Memory<char> buffer = rentedArray;
        await streamReader.ReadAsync(buffer);

        var requestHeader = buffer[..buffer.Span.IndexOf('\r')].ToString();
        ArrayPool<char>.Shared.Return(rentedArray);

        var requestUri = requestHeader.Split(' ')[1][1..];
        if (!UrlValidator.IsValidUrl(requestUri))
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes(BadRequestResponse));
            await stream.DisposeAsync();
            Console.WriteLine($"Closed connection {index}: bad request");
            return;
        }
        if(string.IsNullOrEmpty(requestUri) || !_fileExtensionProvider.TryGetContentType(
               requestUri
                   .Split('/', StringSplitOptions.RemoveEmptyEntries)
                   .Last(), out var contentType))
            contentType = "text/plain";

        var filePath = Path.Combine(Environment.CurrentDirectory, requestUri);
        await using var file = File.OpenRead(filePath);
        var responseHeaders = $"HTTP/1.1 200 OK\n" +
                              $"Date: {DateTime.Now.ToUniversalTime():R}\n" +
                              $"Content-Length: {file.Length}\n" +
                              $"Content-Type: {contentType};charset=utf-8\n\n";

        await stream.WriteAsync(Encoding.UTF8.GetBytes(responseHeaders));
        await file.CopyToAsync(stream);

        Console.WriteLine("Sent response: " + index);
        await Task.Run(() => ProcessRequest(client, stream, index));
    }
}