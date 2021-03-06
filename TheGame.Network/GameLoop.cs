using Microsoft.Extensions.Logging;

namespace TheGame.Network;

public class GameLoop
{
    private readonly TimeSpan _delayBetweenTicks;
    private readonly ILogger<GameLoop> _logger;

    public GameLoop(GameLoopOptions options, ILogger<GameLoop> logger)
    {
        _delayBetweenTicks = TimeSpan.FromSeconds(1) / options.tickRate;
        _logger = logger;
    }

    public void Run(Action<TimeSpan> update)
    {
        var prev = DateTime.Now - _delayBetweenTicks;

        while (true)
        {
            var next = prev + _delayBetweenTicks + _delayBetweenTicks;
            var delta = DateTime.Now - prev;
            prev += delta;

            _logger.LogDebug("Updating, delta is {0} ms", delta.TotalMilliseconds);

            update(delta);

            Delay(next);
        }
    }

    private void Delay(DateTime next)
    {
        var now = DateTime.Now;

        if (next > now)
        {
            var delay = next - now;
            _logger.LogDebug("Next update in {0} ms", delay.TotalMilliseconds);
            Thread.Sleep(delay);
        }

        _logger.LogDebug("Running next update immediately");
    }
}
