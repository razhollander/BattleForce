using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Utils.CustomCollections;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public interface INetEventsDataService
    {
        void InitEntryPoint();
        void ClearPlayerNetEvents(ushort playerId);
        void StartSavingPlayerEvents(ushort playerId);
        void RemoveAllEventsOlderThanTick(int tick);
        CapacityDict<ushort, FixedUnorderedList<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>> PlayerRejoinAcceptNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>> MatchMakingPlayerJoinAcceptNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<BulletDestroyedNetEventS2C>> BulletDestroyedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>> PowerUpBallSpawnedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PowerUpBallDestroyedNetEventS2C>> PowerUpBallDestroyedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<TalentCardSpawnedNetEventS2C>> TalentCardSpawnedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<TalentCardDestroyedNetEventS2C>> TalentCardDestroyedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<TalentCardObtainedNetEventS2C>> TalentCardObtainedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PowerUpBallObtainedNetEventS2C>> PowerUpBallObtainedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<GainBoltsNetEventS2C>> GainBoltsNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<GainGemsNetEventS2C>> GainGemsNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerDiedNetEventS2C>> PlayerDiedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<TalentSwitchNetEventS2C>> TalentSwitchNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerSpinnedStartedNetEventS2C>> PlayerSpinnedStartedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerSpinnedEndedNetEventS2C>> PlayerSpinnedEndedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PerformDashPulseNetEventS2C>> PerformDashPulseNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerSwapNetEventS2C>> PlayerSwapNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<StageEndNetEventS2C>> StageEndNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PreparationPhaseEndedNetEventS2C>> PreparationPhaseEndedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerTakeDamangeNetEventS2C>> PlayerTakeDamangeNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>> CreateGrapplingHookProjectileNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>> DeactivateGrapplingHookTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>> PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>> CreateMagneticPullFieldNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<CreateSwapFieldNetEventS2C>> CreateSwapFieldNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<CreateKOProjectileNetEventS2C>> CreateKOProjectileNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>> ActivateSentryGunTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>> DeactivateSentryGunTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C>> ActivateUmbrellaTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C>> DeactivateUmbrellaTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<LayChickenEggNetEventS2C>> LayChickenEggNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<ChickenEggDestroyedNetEventS2C>> ChickenEggDestroyedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C>> ActivateYearsOfPainTalentNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerSwitchTeamNetEventS2C>> PlayerSwitchTeamNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<StartMatchCountdownNetEventS2C>> StartMatchCountdownNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<StopMatchCountdownNetEventS2C>> StopMatchCountdownNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>> StartMatchEligibleChangedNetEventsPerPlayer { get; }

        CapacityDict<ushort, FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>> PlayerMaxShootCooldownChangedNetEventsPerPlayer { get; }
        CapacityDict<ushort, FixedUnorderedList<PlayerLockOnHeartTargetsChangedNetEventS2C>> PlayerLockOnHeartTargetsChangedNetEventsPerPlayer { get; }
        void AddBulletSpawnNetEvent(int onTick, ushort bulletId, ushort belongToPlayerId, System.Numerics.Vector2 position, float radius, System.Numerics.Vector2 velocity);
        void AddBulletDestroyedNetEvent(int onTick, ushort bulletId);
        void AddPlayerJoinAcceptedEvent(int onTick, List<PlayerStateS2C> playerStates, MatchSimulationStateS2C simulationState, ushort playerId);
        void AddMatchMakingPlayerJoinAcceptedEvent(int onTick, List<MatchMakingPlayerStateS2C> playerStates, MatchMakingSimulationStateS2C simulationState, ushort playerId);
        void AddPowerUpBallSpawnedNetEvent(int onTick, ushort powerUpBallId, System.Numerics.Vector2 position);
        void AddPowerUpBallDestroyedNetEvent(int onTick, ushort powerUpBallId);
        void AddTalentCardSpawnedNetEvent(int onTick, ushort talentCardId, System.Numerics.Vector2 talentCardPosition, SharedGamePlayConfig.TalentType talentType, ushort health);
        void AddTalentCardDestroyedNetEvent(int onTick, ushort talentCardId);
        void AddTalentCardObtainedNetEvent(int onTick, ushort talentCardId, ushort playerId);
        void AddPowerUpBallObtainedNetEvent(int onTick, ushort powerUpBallId, ushort playerId);
        void AddGainBoltsNetEvent(int onTick, ushort playerId, int amountOfBolts);
        void AddGainGemsNetEvent(int onTick, ushort playerId, int amountOfGems);
        void AddPlayerDiedNetEvent(int onTick, ushort playerId, int timeToRespawnTicks);
        void AddTalentSwitchNetEvent(int onTick, ushort playerId, int switchedTalentIndex);
        void AddPlayerSpinnedStartedNetEvent(int onTick, ushort playerId);
        void AddPlayerSpinnedEndedNetEvent(int onTick, ushort playerId);
        void AddPerformDashPulseNetEvent(int onTick, ushort playerId, System.Numerics.Vector2 direction);
        void AddPlayerSwapNetEvent(int onTick, ushort swapperPlayerId, ushort targetPlayerId, System.Numerics.Vector2 swapperPlayerPosition, System.Numerics.Vector2 targetPlayerPosition);
        void AddStageEndNetEvent(int onTick, int currentStageIndex, ushort winningTeamId);
        void AddPreparationPhaseEndedNetEvent(int onTick);
        void AddPlayerTakeDamangeNetEvent(int onTick, ushort playerId, ushort attackerId, ushort damage);
        void AddCreateGrapplingHookProjectileNetEvent(int onTick, ushort playerId, ushort projectileId, System.Numerics.Vector2 initialPosition);
        void AddDeactivateGrapplingHookTalentNetEvent(int onTick, ushort playerId, ushort projectileId);
        void AddPlayerToEnvironmentTeleportGateCollisionNetEvent(int onTick, ushort playerId, System.Numerics.Vector2 positionAfterTeleportation);
        void AddCreateMagneticPullFieldNetEvent(int onTick, ushort playerId);
        void AddCreateSwapFieldNetEvent(int onTick, ushort playerId, ushort swapFieldId, int endTick, float maxRadius);
        void AddCreateKOProjectileNetEvent(int onTick, ushort projectileId, ushort casterPlayerId, float size);
        void AddActivateSentryGunTalentNetEvent(int onTick, ushort playerId, System.Numerics.Vector2 position, System.Numerics.Vector2 direction);
        void AddDeactivateSentryGunTalentNetEvent(int onTick, ushort playerId);
        void AddActivateUmbrellaTalentNetEvent(int onTick, ushort playerId);
        void AddDeactivateUmbrellaTalentNetEvent(int onTick, ushort playerId);
        void AddLayChickenEggNetEvent(int onTick, ushort playerId, ushort eggId, System.Numerics.Vector2 position);
        void AddChickenEggDestroyedNetEvent(int onTick, ushort eggId);
        void AddActivateYearsOfPainTalentNetEvent(int onTick, ushort playerId);
        void AddPlayerMaxShootCooldownChangedNetEvent(int onTick, ushort playerId, float maxCooldown);
        void AddPlayerLockOnHeartTargetsChangedNetEvent(int onTick, ushort playerId, List<ushort> targetIds);
        void AddPlayerSwitchTeamNetEvent(int onTick, ushort playerId, ushort newTeamId);
        void AddStartMatchCountdownNetEvent(int onTick, float countdownSeconds);
        void AddStopMatchCountdownNetEvent(int onTick);
        void AddStartMatchEligibleChangedNetEvent(int onTick, bool isEligible);
    }
}
