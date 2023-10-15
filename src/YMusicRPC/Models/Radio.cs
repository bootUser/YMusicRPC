namespace YMusicRPC.Models;
internal record Radio(string Id, string Name, string ImageUrl) : IListenable;
