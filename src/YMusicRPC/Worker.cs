using YMusicRPC.Models;
using Timer = System.Timers.Timer;

namespace YMusicRPC;

internal class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly YMusicService _yandexMusic;
    private readonly DiscordRPCService _discordRPC;

    private IListenable? _last;
    private Timer? _timer;

    public Worker(ILogger<Worker> logger, YMusicService yandexMusic, DiscordRPCService discordRPC)
    {
        (_logger, _yandexMusic, _discordRPC) = (logger, yandexMusic, discordRPC);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _yandexMusic.Authorize();
            await _discordRPC.Initialize();
            await DoWorkAsync(stoppingToken);
        }
        catch(Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);
            Environment.Exit(0);
        }
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
            try
            {
                if (!_timer?.Enabled ?? false && _last is Track)
                {
                    _discordRPC.ClearPresence();
                    _timer = null;
                    _logger.LogInformation("Presence cleared. Timer for track elapsed.");
                }

                var listenable = await _yandexMusic.GetListenableAsync();
                if(listenable?.Id != _last?.Id)
                {
                    _last = listenable;
                    switch (listenable)
                    {
                        case Track track:
                            _discordRPC.SetPresenceForTrack(track);
                            _timer = new Timer(track.Duration) { AutoReset = false, Enabled = true };
                            _logger.LogInformation("Listening: {1}", track);
                            break;
                        case Radio radio:
                            _discordRPC.SetPresenceForRadio(radio);
                            _logger.LogInformation("Listening: {1}", radio);
                        break;
                        default:
                            _discordRPC.ClearPresence();
                            _logger.LogInformation("Presence cleared. Queue is empty.");
                            break;
                    }
                }
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                _logger.LogInformation("Reinitializing services");
                _last = null;
                _timer = null;
                _discordRPC.Deinitialize();
                await Task.Delay(5000, stoppingToken);
                _discordRPC.Initialize();
                _yandexMusic.Authorize();
            }
        }
    }
}
