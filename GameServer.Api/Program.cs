using GameServer.Api.Middleware;
using GameServer.Api.Options;
using GameServer.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<GameServerOptions>()
    .Bind(builder.Configuration.GetSection(GameServerOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(static options => options.MaxPlayers > 0, "GameServer:MaxPlayers must be greater than 0.")
    .Validate(static options => options.StartedAtUtc != default, "GameServer:StartedAtUtc must be configured.")
    .Validate(static options => options.Randomization.MinOnlinePlayers >= 0, "GameServer:Randomization:MinOnlinePlayers must be >= 0.")
    .Validate(static options => options.Randomization.MaxOnlinePlayers >= 0, "GameServer:Randomization:MaxOnlinePlayers must be >= 0.")
    .Validate(static options => options.Randomization.MinPingMs >= 0, "GameServer:Randomization:MinPingMs must be >= 0.")
    .Validate(static options => options.Randomization.MaxPingMs >= 0, "GameServer:Randomization:MaxPingMs must be >= 0.")
    .Validate(static options => options.Randomization.MaxOnlinePlayers >= options.Randomization.MinOnlinePlayers,
        "GameServer:Randomization:MaxOnlinePlayers must be >= MinOnlinePlayers.")
    .Validate(static options => options.Randomization.MaxPingMs >= options.Randomization.MinPingMs,
        "GameServer:Randomization:MaxPingMs must be >= MinPingMs.")
    .ValidateOnStart();

builder.Services.AddSingleton<IGameServerFakeDataService, GameServerFakeDataService>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseWhen(static context => context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
    static appBuilder => appBuilder.UseMiddleware<ApiKeyMiddleware>());

app.MapGet("/api/server/status", (IGameServerFakeDataService fakeDataService) =>
{
    var status = fakeDataService.GetServerStatus();
    return Results.Ok(status);
})
.WithName("GetServerStatus")
.WithSummary("Gets current fake game server status.");

app.MapGet("/api/players", (IGameServerFakeDataService fakeDataService) =>
{
    var players = fakeDataService.GetPlayers();
    return Results.Ok(players);
})
.WithName("GetPlayers")
.WithSummary("Gets fake game players.");

app.Run();
