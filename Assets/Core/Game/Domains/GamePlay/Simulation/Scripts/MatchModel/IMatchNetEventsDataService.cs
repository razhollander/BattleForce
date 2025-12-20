using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public interface IMatchNetEventsDataService
    {
        Dictionary<ushort, List<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerPlayer { get; }
        Dictionary<ushort, List<PlayerJoinAcceptPacketS2C>> JoinAcceptNetEventsPerPlayer { get; }
        void StartSavingPlayerEvents(ushort playerId);
        void StopSavingPlayerEvents(ushort playerId);
        void AddBulletSpawnNetEvent(int onTick, ushort bulletId, ushort belongToPlayerId, Vector2 position, float bulletRadius);
        void RemoveAllEventsOlderThanTick(ushort playerId, int tick);

        void AddPlayerJoinAcceptedEvent(int onTick, PlayerStateS2C playerState, SimulationStateS2C simulationState);
    }
}