using Microsoft.Extensions.Options;
using Yandex.Music.Client;
using YMusicRPC.Models;
using Yandex.Music.Api.Models.Radio;
using Yandex.Music.Api.Models.Queue;
using Yandex.Music.Client.Extensions;

namespace YMusicRPC;
internal class YMusicService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOptions<AppOptions> _options;
    private readonly YandexMusicClientAsync _client = new();

    public YMusicService(ILogger<Worker> logger, IOptions<AppOptions> options) =>
        (_logger, _options) = (logger, options);

    public async Task Authorize() => await _client.Authorize(_options.Value.Token);

    private async Task<Track> GetTrackAsync(YQueueItem queueItem)
    {
        var queue = await _client.GetQueue(queueItem.Id);
        var track = await _client.GetTrack(queue.Tracks[queue.CurrentIndex!.Value].TrackId);
        return new Track(track.Id, track.Title, track.Artists.Select(a => a.Name).ToList(), TimeSpan.FromMilliseconds(track.DurationMs),
            await track.GetLinkAsync(), track.GetCoverLink());
    }
    private async Task<Radio> GetRadioAsync(YQueueItem queueItem)
    {
        var radioIdParts = queueItem.Context.Id.Split(':');
        var radioId = new YStationId { Tag = radioIdParts[1], Type = radioIdParts[0] };
        var radio = (await _client.GetRadioStation(radioId)).Station;
        return new Radio(radio.IdForFrom, radio.Name, radio.GetCoverLink());
    }

    public async Task<IListenable?> GetListenableAsync()
    {
        var queueItem = (await _client.QueuesList()).Queues.FirstOrDefault();
        if (queueItem == null)
            return null;
        return queueItem.Context.Type == "radio" ? await GetRadioAsync(queueItem) : await GetTrackAsync(queueItem);
    }
}
