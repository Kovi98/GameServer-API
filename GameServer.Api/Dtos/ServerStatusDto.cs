using System.ComponentModel;

namespace GameServer.Api.Dtos;

public sealed record ServerStatusDto(
    [property: Description("Displayed server name.")] string ServerName,
    [property: Description("Name of the game the server runs.")] string GameName,
    [property: Description("Current game server version.")] string Version,
    [property: Description("Currently active map name.")] string MapName,
    [property: Description("Whether the server is currently online.")] bool IsOnline,
    [property: Description("Number of players currently online.")] int OnlinePlayers,
    [property: Description("Maximum number of players the server supports.")] int MaxPlayers,
    [property: Description("Server MOTD shown to players.")] string Motd,
    [property: Description("UTC timestamp when the server started.")] DateTime StartedAt,
    [property: Description("UTC timestamp when this response was generated.")] DateTime GeneratedAt);
