using System.Threading.Channels;

using Microsoft.Extensions.Logging;

using TheGame.Protobuf;

namespace TheGame.DedicatedServer;

public class ServerMessageQueue
{
    private readonly Channel<ServerMessage> _channel;
    private readonly ILogger<ServerMessageQueue> _logger;

    public ServerMessageQueue(ILogger<ServerMessageQueue> logger)
    {
        _channel = Channel.CreateUnbounded<ServerMessage>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = true
        });
        _logger = logger;
    }

    public IEnumerable<ServerMessage> ReadAll()
    {
        while (_channel.Reader.TryRead(out var serverMessage))
        {
            yield return serverMessage;
        }
    }

    public void Write(ServerMessage serverMessage)
    {
        if (!_channel.Writer.TryWrite(serverMessage))
        {
            _logger.LogError("Failed to write message to channel");
        };
    }
}
