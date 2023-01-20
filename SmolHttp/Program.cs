using System.Net;
using SmolHttp;

Console.WriteLine("Listening on port 8080");
var server = new HttpServer(new IPEndPoint(IPAddress.Any, 8080));
await server.StartReceiving();
