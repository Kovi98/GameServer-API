using System.ComponentModel.DataAnnotations;

namespace GameServer.Api.Options;

public sealed class PlayerOptions
{
    [Required]
    public string Nickname { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Level { get; set; }

    public bool IsBanned { get; set; }
}
