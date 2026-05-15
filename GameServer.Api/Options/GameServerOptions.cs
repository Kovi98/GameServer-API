using System.ComponentModel.DataAnnotations;

namespace GameServer.Api.Options;

public sealed class GameServerOptions
{
    public const string SectionName = "GameServer";

    [Required]
    public string ServerName { get; set; } = string.Empty;

    [Required]
    public string GameName { get; set; } = string.Empty;

    [Required]
    public string Version { get; set; } = string.Empty;

    [Required]
    public string MapName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int MaxPlayers { get; set; }

    [Required]
    public string Motd { get; set; } = string.Empty;

    public bool IsOnline { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public List<PlayerOptions> Players { get; set; } = [];

    public RandomizationOptions Randomization { get; set; } = new();
}
