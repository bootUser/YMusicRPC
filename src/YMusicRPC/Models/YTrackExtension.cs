using Yandex.Music.Api.Models.Radio;
using Yandex.Music.Api.Models.Track;

namespace YMusicRPC.Models;
public static class YTrackExtensions
{
    public const int CoverSize = 1000;
    public static string GetCoverLink(this YTrack track) =>
        "https://" + track.CoverUri.Replace("%%", $"{CoverSize}x{CoverSize}");
}
