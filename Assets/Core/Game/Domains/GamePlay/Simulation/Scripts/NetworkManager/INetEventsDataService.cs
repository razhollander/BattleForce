using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Utils.CustomCollections;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public interface INetEventsDataService
    {
        CapacityDict<long, FixedUnorderedList<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerClient { get; }
        CapacityDict<long, FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>> PlayerRejoinAcceptNetEventsPerClient { get; }
        CapacityDict<long, FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>> MatchMakingPlayerJoinAcceptNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<BulletDestroyedNetEventS2C>> BulletDestroyedNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PlayerTakeDamageNetEventS2C>> PlayerTakeDamageNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PlayerDiedNetEventS2C>> PlayerDiedNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PlayersSwapNetEventS2C>> PlayerSwapNetEventsPerClient { get; }
        CapacityDict<long, FixedClassUnorderedList<TalentCardObtainedNetEventS2C>> TalentCardObtainedNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<TalentCardHitNetEventS2C>> TalentCardHitNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PlayerSpinnedStartedNetEventS2C>> PlayerSpinnedStartedNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PlayerSpinnedEndedNetEventS2C>> PlayerSpinnedEndedNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>> PowerUpBallSpawnedNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PowerUpBallObtainedNetEventS2C>> PowerUpBallObtainedNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PlayerSwitchTeamNetEventS2C>> PlayerSwitchTeamNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<StartMatchCountdownNetEventS2C>> StartMatchCountdownNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<StopMatchCountdownNetEventS2C>> StopMatchCountdownNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>> StartMatchEligibleChangedNetEventsPerClient { get; }
        CapacityDict<long, FixedClassUnorderedList<StageEndNetEventS2C>> StageEndNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<TeamLostNetEventS2C>> TeamLostNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<TalentSwitchNetEventS2C>> TalentSwitchNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>> EnvironmentSpringPlayerCollisionNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<GainBoltsNetEventS2C>> GainBoltsNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>> PlayerToEnvironmentTeleportGateCollisionNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PreparationPhaseEndedNetEventS2C>> PreparationPhaseEndedNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<CreateSwapFieldNetEventS2C>> CreateSwapFieldNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> DeactivateSwapTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<CreateKOProjectileNetEventS2C>> CreateKOProjectileNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<KOProjectHitPlayerNetEventS2C>> KOProjectHitPlayerNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DeactivateKOTalentNetEventS2C>> DeactivateKOTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PerformDashPulseNetEventS2C>> PerformDashPulseNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>> ActivateSentryGunTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>> DeactivateSentryGunTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>> UpdatePlayerTalentStocksNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>> PlayerMaxShootCooldownChangedNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>> CreateGrapplingHookProjectileNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<GrapplingHookHitWallNetEventS2C>> GrapplingHookHitWallNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>> DeactivateGrapplingHookTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<CreateFishingRodProjectileNetEventS2C>> CreateFishingRodProjectileNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<FishingRodCaughtEnemyNetEventS2C>> FishingRodCaughtEnemyNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<FishingRodTipHitWallNetEventS2C>> FishingRodTipHitWallNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<FishingRodThrowNetEventS2C>> FishingRodThrowNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DeactivateFishingRodTalentNetEventS2C>> DeactivateFishingRodTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<CreateSoulGhostNetEventS2C>> CreateSoulGhostNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DeactivateSoulTalentNetEventS2C>> DeactivateSoulTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C>> ActivateUmbrellaTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C>> DeactivateUmbrellaTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<ActivateWaterGunTalentNetEventS2C>> ActivateWaterGunTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DeactivateWaterGunTalentNetEventS2C>> DeactivateWaterGunTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<ActivateHeadbuttChargingNetEventS2C>> ActivateHeadbuttChargingNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PerformHeadbuttDashNetEventS2C>> PerformHeadbuttDashNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<HeadbuttHitEnemyNetEventS2C>> HeadbuttHitEnemyNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DeactivateHeadbuttTalentNetEventS2C>> DeactivateHeadbuttTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<LayChickenEggNetEventS2C>> LayChickenEggNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<ChickenEggHitNetEventS2C>> ChickenEggHitNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>> CreateMagneticPullFieldNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C>> ActivateYearsOfPainTalentNetEventsPerClient { get; }
        CapacityDict<long, FixedClassUnorderedList<PlayerLockOnTargetsChangedNetEventS2C>> PlayerLockOnTargetsChangedNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C>> PlayerLockedOnTargetHitNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PlayerPowerUpChangedNetEventS2C>> PlayerPowerUpChangedNetEventsPerClient { get; }
        CapacityDict<long, FixedClassUnorderedList<ActivateSonicSlapNetEventS2C>> ActivateSonicSlapNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<EnvironmentSpikePlayerCollisionNetEventS2C>> EnvironmentSpikePlayerCollisionNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<PerformGalacticPullNetEventS2C>> PerformGalacticPullNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DeactivateGalacticForceFieldNetEventS2C>> DeactivateGalacticForceFieldNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<ActivateNukePowerUpNetEventS2C>> ActivateNukePowerUpNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DeactivateShufflePowerUpNetEventS2C>> DeactivateShufflePowerUpNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<ShuffleSwapPlayerPositionNetEventS2C>> ShuffleSwapPlayerPositionNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<ActivateShuffleNetEventS2C>> ActivateShuffleNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<StartPowerUpGrantingPhaseNetEventS2C>> StartPowerUpGrantingPhaseNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<EndPowerUpGrantingPhaseNetEventS2C>> EndPowerUpGrantingPhaseNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<ShootFrigidBlockNetEventS2C>> ShootFrigidBlockNetEventsPerClient { get; }
        CapacityDict<long, FixedUnorderedList<DestroyFrigidBlockNetEventS2C>> DestroyFrigidBlockNetEventsPerClient { get; }

        void StartSavingClientEvents(long clientId);
        void StopSavingClientEvents(long clientId);
        void AddBulletSpawnNetEvent(int onTick, ushort bulletId, ushort belongToPlayerId, Vector2 position, float bulletRadius, Vector2 velocity);
        void AddPlayerTakeDamageNetEvent(int onTick, ushort damagedPlayerId, ushort playerHealth, ushort hitDamage, bool isAlive);
        void AddPlayerDiedNetEvent(int onTick, ushort playerId);
        void AddBulletDestroyedNetEvent(int onTick, ushort bulletId, Vector2 position);
        void AddClientJoinAcceptedEvent(int onTick, List<PlayerStateS2C> playerStates, MatchSimulationStateS2C simulationState, long clientId);
        void AddMatchMakingClientJoinAcceptedEvent(int onTick, List<MatchMakingPlayerStateS2C> playerStates, MatchMakingSimulationStateS2C simulationState, long clientId);
        void AddPlayersSwapEvent(int onTick, ushort casterPlayerId, ushort otherPlayerId, Vector2 casterPlayerPosition, Vector2 otherPlayerPosition, Vector2 casterPlayerDirection, Vector2 otherPlayerDirection);
        void AddTalentCardObtainedNetEvent(int onTick, ushort cardId, ushort obtainedByPlayerId, FixedOrderedList<TalentStateS2C> playerTalents, bool didReplaceTalent);
        void RemoveAllEventsOlderThanTick(long clientId, int tick);
        void AddTalentCardHitNetEvent(int processedTick, ushort talentCardId, ushort cardHealth);
        void AddPlayerSpinnedStartedNetEvent(int onTick, ushort playerId);
        void AddPlayerSpinnedEndedNetEvent(int onTick, ushort playerId);
        void AddPowerUpSpawnedNetEvent(int onTick, ushort powerUpBallId, Vector2 position);
        void AddPowerUpObtainedNetEvent(int onTick, ushort powerUpBallId, ushort byPlayerId);
        void AddPlayerSwitchTeamNetEvent(int onTick, ushort playerId, ushort teamId);
        void AddStartMatchCountdownNetEvent(int onTick, ushort seconds);
        void AddStopMatchCountdownNetEvent(int onTick);
        void AddStartMatchEligibleChangedNetEvent(int onTick, bool isEligible);
        void AddStageEndNetEvent(int onTick, ushort winningTeamId, Dictionary<ushort, int> jemsWon, Dictionary<ushort, int> totalJems, ushort playerIdDoingWinningBlow);
        void AddTeamLostNetEvent(int onTick, ushort losingTeamId, Dictionary<ushort, int> totalGemsPerTeam, Dictionary<ushort, int> gemsGainedPerTeam);
        void AddTalentSwitchNetEvent(int onTick, ushort playerId, int newTalentIndex);
        void AddEnvironmentSpringPlayerCollisionNetEvent(int onTick, ushort springId, ushort playerId, Vector2 newPlayerDirection);
        void AddEnvironmentSpikePlayerCollisionNetEvent(int processedTick, ushort spikeId, ushort playerId);
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
        void AddCreateFishingRodProjectileNetEvent(int onTick, ushort projectileId, ushort playerCasterId, Vector2 position);
        void AddFishingRodCaughtEnemyNetEvent(int onTick, ushort projectileId, ushort casterPlayerId, ushort caughtEnemyId);
        void AddFishingRodTipHitWallNetEvent(int onTick, ushort projectileId, Vector2 hitPosition);
        void AddFishingRodThrowNetEvent(int onTick, ushort casterPlayerId, ushort thrownEnemyId, Vector2 throwDirection);
        void AddDeactivateFishingRodTalentNetEvent(int onTick, ushort casterPlayerId, ushort projectileId, int talentCooldownEndTick);
        void AddCreateSoulGhostNetEvent(int onTick, ushort ghostId, ushort playerCasterId, Vector2 position, Vector2 direction);
        void AddDeactivateSoulTalentNetEvent(int onTick, ushort ghostId, ushort casterPlayerId, int talentCooldownEndTick, bool didTeleport, Vector2 teleportPosition, Vector2 teleportDirection);
        void AddActivateUmbrellaTalentNetEvent(int onTick, ushort casterPlayerId);
        void AddDeactivateUmbrellaTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick);
        void AddActivateWaterGunTalentNetEvent(int onTick, ushort casterPlayerId);
        void AddDeactivateWaterGunTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick);
        void AddCreateMagneticPullFieldNetEventS2C(int onTick, ushort casterPlayerId, Vector2 position, Vector2 direction, int talentCooldownEndTick, bool hasHit, ushort hitEnemyId);
        void AddLayChickenEggNetEventS2C(int tick, ushort casterId, ushort eggId, Vector2 position);
        void AddChickenEggHitNetEventS2C(int tick, ushort eggId);
        void AddActivateYearsOfPainTalentNetEventS2C(int tick, ushort casterPlayerId, Vector2 direction, int cooldownEndTick, bool didHitEnemy, ushort hitEnemyId);
        void AddPlayerLockOnTargetsChangedNetEvent(int onTick, ushort playerId, FixedUnorderedList<ObjectLockedOnTargetS2C> playerIdsLockedOnTarget);
        void AddPlayerLockedOnTargetHitNetEvent(int onTick, ushort casterPlayId, ushort hitPlayerId);
        void AddPlayerPowerUpChangedNetEvent(int onTick, ushort playerId, PowerUpType powerUp);
        void AddActivateSonicSlapNetEvent(int onTick, ushort casterPlayerId, FixedUnorderedList<ushort> affectedPlayerIds);
        void AddPerformGalacticPullNetEvent(int onTick, ushort fieldId, ushort casterPlayerId, ushort casterTeamId);
        void AddDeactivateGalacticForceFieldNetEvent(int onTick, ushort galacticForceFieldId);
        void AddActivateNukePowerUpNetEvent(int onTick, ushort casterPlayerId, Vector2 casterPosition);
        void AddDectivateShufflePowerUpNetEvent(int onTick, ushort casterPlayerId);
        void AddShuffleSwapPlayerPositionNetEvent(int onTick);
        void AddActivateShuffleNetEvent(int onTick, ushort casterPlayerId);
        void AddStartPowerUpGrantingPhaseNetEvent(int onTick, ushort playerId);
        void AddEndPowerUpGrantingPhaseNetEvent(int onTick, ushort playerId, PowerUpType grantedPowerUp);
        void AddShootFrigidBlockNetEvent(int onTick, TalentFrigidBlockStateS2C frigidBlock, int cooldownEndTick);
        void AddDestroyFrigidBlockNetEvent(int onTick, ushort blockId);
        void AddActivateHeadbuttChargingNetEvent(int onTick, ushort casterPlayerId);
        void AddPerformHeadbuttDashNetEvent(int onTick, ushort casterPlayerId);
        void AddHeadbuttHitEnemyNetEvent(int onTick, ushort casterPlayerId, ushort enemyPlayerId);
        void AddDeactivateHeadbuttTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick);
    }
}