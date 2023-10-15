namespace YMusicRPC.Models;
internal record Track(string Id, string Name, List<string> Artists, TimeSpan Duration, string Url, string ImageUrl) : IListenable;
