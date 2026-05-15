namespace GameServer.Api.Dtos;

public sealed record ServerStatusDto(
    string ServerName,
    string GameName,
    string Version,
    string MapName,
    bool IsOnline,
    int OnlinePlayers,
    int MaxPlayers,
    string Motd,
    DateTime StartedAt,
    DateTime GeneratedAt);
