using Microsoft.Extensions.Logging;

namespace TheGame.Core;

public class GameLoop
{
    private readonly TimeSpan _delayBetweenTicks;
    private readonly ILogger<GameLoop> _logger;

    public GameLoop(GameLoopOptions options, ILogger<GameLoop> logger)
    {
        _delayBetweenTicks = TimeSpan.FromSeconds(1) / options.tickRate;
        _logger = logger;
    }

    public async Task Run()
    {
        var prev = DateTime.Now;

        while (true)
        {
            _logger.LogInformation("Beginning game loop");

            var now = DateTime.Now;

            var next = prev + _delayBetweenTicks;

            var delay = next > now
                ? next - now
                : TimeSpan.Zero;

            prev = now + delay;

            _logger.LogInformation("End game loop. Starting next in {0} ms", delay.TotalMilliseconds);
            await Task.Delay(delay);
        }
    }
}
