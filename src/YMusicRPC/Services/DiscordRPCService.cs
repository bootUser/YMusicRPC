using Microsoft.Extensions.Options;
using YMusicRPC.Models;
using DiscordRPC;

namespace YMusicRPC;
internal class DiscordRPCService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOptionsMonitor<AppOptions> _options;
    private readonly DiscordRpcClient _client;

    public DiscordRPCService(ILogger<Worker> logger, IOptionsMonitor<AppOptions> options)
    {
        _logger = logger;
        _options = options;
        _client = new DiscordRpcClient(_options.CurrentValue.DiscordApplicationId);
    }

    public async Task<bool> Initialize()
    {
        var tcs = new TaskCompletionSource();
        DiscordRPC.Events.OnReadyEvent ev = (s,e) => tcs.SetResult();
        _client.OnReady += ev;
        var result = _client.Initialize();
        //await tcs.Task;
        _client.OnReady -= ev;
        return result;
    }
    
    public async Task Deinitialize()
    {
        var tcs = new TaskCompletionSource();
        DiscordRPC.Events.OnCloseEvent ev = (s,e) => tcs.SetResult();
        _client.OnClose += ev;
        _client.Deinitialize();
        await tcs.Task;
        _client.OnClose -= ev;
    }

    public async Task Reinitialize() => await Task.WhenAll(Deinitialize(), Task.Delay(1000), Initialize());

    public void SetPresenceForTrack(Track track)
    {
        var presence = new RichPresence
        {
            Details = track.Name,
            State = string.Join(", ", track.Artists),
            Timestamps = Timestamps.FromTimeSpan(track.Duration),
            Assets = new Assets { LargeImageKey = track.ImageUrl, LargeImageText = track.Name },
            Buttons = new Button[] {new() { Label = "Слушать", Url = track.Url} }
        };
        _client.SetPresence(presence);
        _client.Invoke();
    }
    public void SetPresenceForRadio(Radio radio)
    {
        var presence = new RichPresence
        {
            Details = "Радио",
            State = radio.Name,
            Assets = new Assets { LargeImageKey = radio.ImageUrl, LargeImageText = radio.ImageUrl }
        };
        _client.SetPresence(presence);
    }

    public void ClearPresence() => _client.ClearPresence();
}
