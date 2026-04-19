using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
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
        CapacityDict<ushort, FixedUnorderedList<PlayerDiedNetEventS2C>> PlayerDiedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayersSwapNetEventS2C>> PlayerSwapNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedClassUnorderedList<TalentCardObtainedNetEventS2C>> TalentCardObtainedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<TalentCardHitNetEventS2C>> TalentCardHitNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerSpinnedStartedNetEventS2C>> PlayerSpinnedStartedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerSpinnedEndedNetEventS2C>> PlayerSpinnedEndedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>> PowerUpBallSpawnedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PowerUpBallObtainedNetEventS2C>> PowerUpBallObtainedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerSwitchTeamNetEventS2C>> PlayerSwitchTeamNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<StartMatchCountdownNetEventS2C>> StartMatchCountdownNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<StopMatchCountdownNetEventS2C>> StopMatchCountdownNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>> StartMatchEligibleChangedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedClassUnorderedList<StageEndNetEventS2C>> StageEndNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<TeamLostNetEventS2C>> TeamLostNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<TalentSwitchNetEventS2C>> TalentSwitchNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>> EnvironmentSpringPlayerCollisionNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<GainBoltsNetEventS2C>> GainBoltsNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>> PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PreparationPhaseEndedNetEventS2C>> PreparationPhaseEndedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<CreateSwapFieldNetEventS2C>> CreateSwapFieldNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> DeactivateSwapTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<CreateKOProjectileNetEventS2C>> CreateKOProjectileNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<KOProjectHitPlayerNetEventS2C>> KOProjectHitPlayerNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<DeactivateKOTalentNetEventS2C>> DeactivateKOTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PerformDashPulseNetEventS2C>> PerformDashPulseNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>> ActivateSentryGunTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>> DeactivateSentryGunTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>> UpdatePlayerTalentStocksNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>> PlayerMaxShootCooldownChangedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>> CreateGrapplingHookProjectileNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<GrapplingHookHitWallNetEventS2C>> GrapplingHookHitWallNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>> DeactivateGrapplingHookTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C>> ActivateUmbrellaTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C>> DeactivateUmbrellaTalentNetEventsPerPlayer { get; }
        void StartSavingPlayerEvents(ushort playerId);
        void StopSavingPlayerEvents(ushort playerId);
        void AddBulletSpawnNetEvent(int onTick, ushort bulletId, ushort belongToPlayerId, Vector2 position, float bulletRadius);
        void AddPlayerTakeDamageNetEvent(int onTick, ushort damagedPlayerId, ushort playerHealth, ushort hitDamage, bool isAlive);
        void AddPlayerDiedNetEvent(int onTick, ushort playerId);
        void AddBulletDestroyedNetEvent(int onTick, ushort bulletId, Vector2 position);
        void AddPlayerJoinAcceptedEvent(int onTick, PlayerStateS2C playerState, MatchSimulationStateS2C simulationState);
        void AddMatchMakingPlayerJoinAcceptedEvent(int onTick, MatchMakingPlayerStateS2C playerState, MatchMakingSimulationStateS2C simulationState);
        void AddPlayersSwapEvent(int onTick, ushort casterPlayerId, ushort otherPlayerId, Vector2 casterPlayerPosition, Vector2 otherPlayerPosition, Vector2 casterPlayerDirection, Vector2 otherPlayerDirection);
        void AddTalentCardObtainedNetEvent(int onTick, ushort cardId, ushort obtainedByPlayerId, FixedOrderedList<TalentStateS2C> playerTalents, bool didReplaceTalent);
        void RemoveAllEventsOlderThanTick(ushort playerId, int tick);
        void AddTalentCardHitNetEvent(int processedTick, ushort talentCardId, ushort cardHealth);
        void AddPlayerSpinnedStartedNetEvent(int onTick, ushort playerId);
        void AddPlayerSpinnedEndedNetEvent(int onTick, ushort playerId);
        void AddPowerUpSpawnedNetEvent(int onTick, ushort powerUpBallId, Vector2 position);
        void AddPowerUpObtainedNetEvent(int onTick, ushort powerUpBallId, ushort byPlayerId);
        void AddPlayerSwitchTeamNetEvent(int onTick, ushort playerId, ushort teamId);
        void AddStartMatchCountdownNetEvent(int onTick, ushort seconds);
        void AddStopMatchCountdownNetEvent(int onTick);
        void AddStartMatchEligibleChangedNetEvent(int onTick, bool isEligible);
        void AddStageEndNetEvent(int onTick, ushort winningTeamId, Dictionary<ushort, int> jemsWon, Dictionary<ushort, int> totalJems);
        void AddTeamLostNetEvent(int onTick, ushort losingTeamId, Dictionary<ushort, int> totalGemsPerTeam, Dictionary<ushort, int> gemsGainedPerTeam);
        void AddTalentSwitchNetEvent(int onTick, ushort playerId, int newTalentIndex);
        void AddEnvironmentSpringPlayerCollisionNetEvent(int onTick, ushort springId, ushort playerId, Vector2 newPlayerDirection);
        void AddGainBoltsNetEvent(int onTick, ushort playerId, int gainedAmount, int totalTeamBolts);
        void AddPlayerToEnvironmentTeleportGateCollisionNetEvent(int onTick, ushort teleportPairId, Vector2 enterPoint, Vector2 exitPoint, ushort playerId);
        void AddPreparationPhaseEndedNetEvent(int onTick);
        void AddCreateSwapFieldNetEvent(int onTick, ushort swapFieldId, ushort casterPlayerId, int fieldEndTick, float maxRadius);
        void AddDeactivateSwapTalentNetEvent(int onTick, ushort casterPlayerId, ushort swapFieldId, int talentCooldownEndTick);
        void AddCreateKOProjectileNetEvent(int onTick, ushort projectileId, ushort playerCasterId, Vector2 position, Vector2 velocity, float size);
        void AddKOProjectHitPlayerNetEvent(int onTick, ushort projectileId, ushort hitPlayerId, Vector2 hitPosition);
        void AddPerformDashPulseNetEvent(int onTick, ushort casterPlayerId);
        void AddDeactivateKOTalentNetEvent(int onTick, ushort casterPlayerId, ushort projectileId, int talentCooldownEndTick);
        void AddActivateSentryGunTalentNetEvent(int onTick, ushort casterPlayerId);
        void AddDeactivateSentryGunTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick);
        void AddUpdatePlayerTalentStocksNetEventS2C(int onTick, ushort casterPlayerId, TalentType talentType, int currentStocksAmount, int recieveNextStockOnTick);
        void AddPlayerMaxShootCooldownChangedNetEvent(int onTick, ushort playerId, float maxShootCooldown, float shootCooldownSecondsLeft);
        void AddCreateGrapplingHookProjectileNetEvent(int onTick, ushort projectileId, ushort playerCasterId, Vector2 position);
        void AddGrapplingHookHitWallNetEvent(int onTick, ushort projectileId, ushort hitWallId, Vector2 hitPosition);
        void AddDeactivateGrapplingHookTalentNetEvent(int onTick, ushort casterPlayerId, ushort projectileId, int talentCooldownEndTick);
        void AddActivateUmbrellaTalentNetEvent(int onTick, ushort casterPlayerId);
        void AddDeactivateUmbrellaTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick);
    }
}