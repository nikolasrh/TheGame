using System.Threading.Channels;

using Microsoft.Extensions.Logging;

using TheGame.Protobuf;

namespace TheGame.DedicatedServer;

public class ServerMessageQueue
{
    private readonly Channel<ServerMessage> _channel;
    private readonly CancellationToken _cancellationToken;
    private readonly ILogger<ServerMessageQueue> _logger;

    public ServerMessageQueue(CancellationToken cancellationToken, ILogger<ServerMessageQueue> logger)
    {
        _channel = Channel.CreateUnbounded<ServerMessage>();
        _cancellationToken = cancellationToken;
        _logger = logger;
    }

    public ValueTask<ServerMessage> Read()
    {
        return _channel.Reader.ReadAsync(_cancellationToken);
    }

    public void Write(ServerMessage serverMessage)
    {
        if (!_channel.Writer.TryWrite(serverMessage))
        {
            _logger.LogError("Failed to write message to channel");
        };
    }
}
