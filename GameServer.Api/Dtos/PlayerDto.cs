namespace GameServer.Api.Dtos;

public sealed record PlayerDto(
    Guid Id,
    string Nickname,
    bool IsOnline,
    bool IsBanned,
    int Level,
    int PingMs,
    DateTime? LastSeenAt);
