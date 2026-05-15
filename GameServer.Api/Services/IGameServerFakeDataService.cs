using GameServer.Api.Dtos;

namespace GameServer.Api.Services;

public interface IGameServerFakeDataService
{
    ServerStatusDto GetServerStatus();

    IReadOnlyList<PlayerDto> GetPlayers();
}
