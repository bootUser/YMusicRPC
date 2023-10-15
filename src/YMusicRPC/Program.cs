using YMusicRPC;
using YMusicRPC.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<YMusicService>();
builder.Services.AddSingleton<DiscordRPCService>();
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.Position));
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
IHost host = builder.Build();
host.Run();
