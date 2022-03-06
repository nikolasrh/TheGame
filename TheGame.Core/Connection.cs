using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace TheGame.Core;

public class Connection
{
    public Guid Id { get; } = Guid.NewGuid();
    private readonly ILogger _logger;
    private readonly TcpClient _client;

    public Connection(TcpClient client, ILogger logger)
    {
        client.NoDelay = true;
        _client = client;
        _logger = logger;
    }

    public async Task Write(byte[] bytes)
    {
        var prefix = BitConverter.GetBytes(bytes.Length);
        var bytesWithPrefix = prefix.Concat(bytes).ToArray();

        var networkStream = _client.GetStream();
        await networkStream.WriteAsync(bytesWithPrefix);
    }

    public async Task<byte[]> Read()
    {
        var networkStream = _client.GetStream();

        var prefixBuffer = new byte[4];
        await networkStream.ReadAsync(prefixBuffer, 0, prefixBuffer.Length);

        var length = BitConverter.ToInt32(prefixBuffer);
        var buffer = new byte[length];
        await networkStream.ReadAsync(buffer, 0, length);

        return buffer;
    }
}
