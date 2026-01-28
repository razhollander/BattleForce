using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public interface INetEventsDataService
    {
        CapacityDict<ushort, FixedUnorderedList<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>> PlayerRejoinAcceptNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>> MatchMakingPlayerJoinAcceptNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<BulletDestroyedNetEventS2C>> BulletDestroyedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerTakeDamageNetEventS2C>> PlayerTakeDamageNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayersSwapNetEventS2C>> PlayerSwapNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<TalentCardObtainedNetEventS2C>> TalentCardObtainedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<TalentCardHitNetEventS2C>> TalentCardHitNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>> PowerUpBallSpawnedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PowerUpBallObtainedNetEventS2C>> PowerUpBallObtainedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerSwitchTeamNetEventS2C>> PlayerSwitchTeamNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<StartMatchCountdownNetEventS2C>> StartMatchCountdownNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<StopMatchCountdownNetEventS2C>> StopMatchCountdownNetEventsPerPlayer { get; }

        void StartSavingPlayerEvents(ushort playerId);
        void StopSavingPlayerEvents(ushort playerId);
        void AddBulletSpawnNetEvent(int onTick, ushort bulletId, ushort belongToPlayerId, Vector2 position, float bulletRadius);
        void AddPlayerTakeDamageNetEvent(int onTick, ushort damagedPlayerId, ushort playerHealth, ushort hitDamage, bool isAlive);
        void AddBulletDestroyedNetEvent(int onTick, ushort bulletId, Vector2 position);
        void AddPlayerRejoinAcceptedEvent(int onTick, PlayerStateS2C playerState, MatchSimulationStateS2C simulationState);
        void AddMatchMakingPlayerJoinAcceptedEvent(int onTick, MatchMakingPlayerStateS2C playerState, MatchMakingSimulationStateS2C simulationState);
        void AddPlayersSwapEvent(int onTick, ushort casterPlayerId, ushort otherPlayerId, Vector2 casterPlayerPosition, Vector2 otherPlayerPosition, Vector2 casterPlayerDirection, Vector2 otherPlayerDirection);
        void AddTalentCardObtainedNetEvent(int onTick, ushort cardId, ushort obtainedByPlayerId);
        void RemoveAllEventsOlderThanTick(ushort playerId, int tick);
        void AddTalentCardHitNetEvent(int processedTick, ushort talentCardId, ushort cardHealth);
        void AddPowerUpSpawnedNetEvent(int onTick, ushort powerUpBallId, Vector2 position);
        void AddPowerUpObtainedNetEvent(int onTick, ushort powerUpBallId, ushort byPlayerId);
        void AddPlayerSwitchTeamNetEvent(int onTick, ushort playerId, ushort teamId);
        void AddStartMatchCountdownNetEvent(int onTick, ushort seconds);
        void AddStopMatchCountdownNetEvent(int onTick);
    }
}