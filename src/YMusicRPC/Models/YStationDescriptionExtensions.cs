using Yandex.Music.Api.Models.Radio;

namespace YMusicRPC.Models;
public static class YStationDescriptionExtensions
{
    public const int CoverSize = 1000;
    public static string GetCoverLink(this YStationDescription station) =>
        "https://" + station.Icon.ImageUrl.Replace("%%", $"{CoverSize}x{CoverSize}");
}
