namespace GameServer.Api.Options;

public sealed class RandomizationOptions
{
    public bool Enabled { get; set; }

    public int MinOnlinePlayers { get; set; }

    public int MaxOnlinePlayers { get; set; }

    public int MinPingMs { get; set; }

    public int MaxPingMs { get; set; }
}
