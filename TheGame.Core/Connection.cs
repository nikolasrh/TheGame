using System.IO.Pipelines;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace TheGame.Core;

public class Connection
{
    public Guid Id { get; } = Guid.NewGuid();
    private readonly Pipe _outgoing = new Pipe();
    private readonly ILogger _logger;
    private readonly TcpClient _client;

    public Connection(TcpClient client, ILogger logger)
    {
        client.NoDelay = true;
        _client = client;
        _logger = logger;
    }

    public async Task StartWriting()
    {
        var reader = _outgoing.Reader;
        var networkStream = _client.GetStream();

        while (true)
        {
            var result = await reader.ReadAsync();

            foreach (var memory in result.Buffer)
            {
                await networkStream.WriteAsync(memory);
            }

            reader.AdvanceTo(result.Buffer.End);
        }
    }

    // TODO: Consider changing onRead to EventHandler
    public async Task StartReading(Func<Connection, byte[], Task> onRead)
    {
        // TODO: Consider using pipe for incoming data.
        // Buffer memory can be reused across incoming messages.
        var networkStream = _client.GetStream();
        var prefixBuffer = new byte[4];

        while (true)
        {
            await networkStream.ReadAsync(prefixBuffer, 0, prefixBuffer.Length);

            var length = BitConverter.ToInt32(prefixBuffer);
            var buffer = new byte[length];

            await networkStream.ReadAsync(buffer, 0, length);

            await onRead(this, buffer);
        }
    }

    public async Task Write(byte[] bytes)
    {
        var prefix = BitConverter.GetBytes(bytes.Length);
        var bytesWithPrefix = prefix.Concat(bytes).ToArray();

        await _outgoing.Writer.WriteAsync(bytesWithPrefix);
    }
}
