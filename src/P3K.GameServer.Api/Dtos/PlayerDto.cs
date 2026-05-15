using System.ComponentModel;

namespace GameServer.Api.Dtos;

public sealed record PlayerDto(
    [property: Description("Unique player identifier.")] Guid Id,
    [property: Description("Player nickname displayed in the game.")] string Nickname,
    [property: Description("Whether the player is currently online.")] bool IsOnline,
    [property: Description("Whether the player is banned from the server.")] bool IsBanned,
    [property: Description("Current player level.")] int Level,
    [property: Description("Current ping in milliseconds.")] int PingMs,
    [property: Description("UTC timestamp of the last time the player was seen offline; null when online.")] DateTime? LastSeenAt);
