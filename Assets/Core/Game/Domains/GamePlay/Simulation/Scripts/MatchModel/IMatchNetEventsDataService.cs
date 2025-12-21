using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public interface IMatchNetEventsDataService
    {
        Dictionary<ushort, List<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerPlayer { get; }
        Dictionary<ushort, List<PlayerJoinAcceptPacketS2C>> JoinAcceptNetEventsPerPlayer { get; }
        Dictionary<ushort, List<BulletDestroyedNetEventS2C>> BulletDestroyedNetEventsPerPlayer { get; }
        Dictionary<ushort, List<PlayerTakeDamageNetEventS2C>> PlayerTakeDamageNetEventsPerPlayer { get; }
        void StartSavingPlayerEvents(ushort playerId);
        void StopSavingPlayerEvents(ushort playerId);
        void AddBulletSpawnNetEvent(int onTick, ushort bulletId, ushort belongToPlayerId, Vector2 position, float bulletRadius);
        void AddPlayerTakeDamageNetEvent(int onTick, ushort damagedPlayerId, int playerHealth, int hitDamage, bool isAlive);
        void AddBulletDestroyedNetEvent(int onTick, ushort bulletId, Vector2 position);
        void AddPlayerJoinAcceptedEvent(int onTick, PlayerStateS2C playerState, SimulationStateS2C simulationState);
        void RemoveAllEventsOlderThanTick(ushort playerId, int tick);
    }
}