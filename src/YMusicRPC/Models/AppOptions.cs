namespace YMusicRPC.Models;
public sealed class AppOptions
{
    public const string Position = nameof(AppOptions);
    public required string DiscordApplicationId { get; set; }
    public required string Token { get; set; }
}