using System.Security.Cryptography;
using System.Text;
using GameServer.Api.Dtos;
using GameServer.Api.Options;
using Microsoft.Extensions.Options;

namespace GameServer.Api.Services;

public sealed class GameServerFakeDataService : IGameServerFakeDataService
{
    private readonly GameServerOptions _options;
    private readonly IReadOnlyList<SeedPlayer> _seedPlayers;

    public GameServerFakeDataService(IOptions<GameServerOptions> options)
    {
        _options = options.Value;
        _seedPlayers = _options.Players
            .Select(static player => new SeedPlayer(
                CreateDeterministicGuid(player.Nickname),
                player.Nickname,
                player.Level,
                player.IsBanned))
            .ToList();
    }

    public ServerStatusDto GetServerStatus()
    {
        var generatedAt = DateTime.UtcNow;

        if (!_options.IsOnline)
        {
            return new ServerStatusDto(
                _options.ServerName,
                _options.GameName,
                _options.Version,
                _options.MapName,
                false,
                0,
                _options.MaxPlayers,
                _options.Motd,
                _options.StartedAtUtc,
                generatedAt);
        }

        var notBannedCount = _seedPlayers.Count(static player => !player.IsBanned);
        var maxOnlineByConfig = Math.Min(_options.MaxPlayers, notBannedCount);

        int onlinePlayers;
        if (_options.Randomization.Enabled)
        {
            var minOnline = Math.Min(_options.Randomization.MinOnlinePlayers, maxOnlineByConfig);
            var maxOnline = Math.Min(_options.Randomization.MaxOnlinePlayers, maxOnlineByConfig);
            if (maxOnline < minOnline)
            {
                maxOnline = minOnline;
            }

            onlinePlayers = Random.Shared.Next(minOnline, maxOnline + 1);
        }
        else
        {
            onlinePlayers = maxOnlineByConfig;
        }

        return new ServerStatusDto(
            _options.ServerName,
            _options.GameName,
            _options.Version,
            _options.MapName,
            true,
            onlinePlayers,
            _options.MaxPlayers,
            _options.Motd,
            _options.StartedAtUtc,
            generatedAt);
    }

    public IReadOnlyList<PlayerDto> GetPlayers()
    {
        if (_seedPlayers.Count == 0)
        {
            return [];
        }

        if (!_options.IsOnline)
        {
            return _seedPlayers
                .Select(player => new PlayerDto(
                    player.Id,
                    player.Nickname,
                    false,
                    player.IsBanned,
                    player.Level,
                    0,
                    GetRandomPastDateTimeUtc()))
                .ToList();
        }

        var onlineIds = GetOnlinePlayerIds();

        return _seedPlayers
            .Select(player =>
            {
                var isOnline = !player.IsBanned && onlineIds.Contains(player.Id);
                var pingMs = isOnline
                    ? Random.Shared.Next(_options.Randomization.MinPingMs, _options.Randomization.MaxPingMs + 1)
                    : 0;
                var lastSeenAt = isOnline ? (DateTime?)null : GetRandomPastDateTimeUtc();

                return new PlayerDto(
                    player.Id,
                    player.Nickname,
                    isOnline,
                    player.IsBanned,
                    player.Level,
                    pingMs,
                    lastSeenAt);
            })
            .ToList();
    }

    private HashSet<Guid> GetOnlinePlayerIds()
    {
        var availablePlayers = _seedPlayers
            .Where(static player => !player.IsBanned)
            .ToList();

        if (availablePlayers.Count == 0)
        {
            return [];
        }

        if (!_options.Randomization.Enabled)
        {
            return availablePlayers
                .Take(_options.MaxPlayers)
                .Select(static player => player.Id)
                .ToHashSet();
        }

        var maxOnlineByConfig = Math.Min(_options.MaxPlayers, availablePlayers.Count);
        var minOnline = Math.Min(_options.Randomization.MinOnlinePlayers, maxOnlineByConfig);
        var maxOnline = Math.Min(_options.Randomization.MaxOnlinePlayers, maxOnlineByConfig);
        if (maxOnline < minOnline)
        {
            maxOnline = minOnline;
        }

        var onlineCount = Random.Shared.Next(minOnline, maxOnline + 1);

        return availablePlayers
            .OrderBy(static _ => Random.Shared.Next())
            .Take(onlineCount)
            .Select(static player => player.Id)
            .ToHashSet();
    }

    private static DateTime GetRandomPastDateTimeUtc()
    {
        var hoursAgo = Random.Shared.Next(1, 72);
        var minutesAgo = Random.Shared.Next(0, 60);
        return DateTime.UtcNow.AddHours(-hoursAgo).AddMinutes(-minutesAgo);
    }

    private static Guid CreateDeterministicGuid(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return new Guid(hash[..16]);
    }

    private sealed record SeedPlayer(Guid Id, string Nickname, int Level, bool IsBanned);
}
