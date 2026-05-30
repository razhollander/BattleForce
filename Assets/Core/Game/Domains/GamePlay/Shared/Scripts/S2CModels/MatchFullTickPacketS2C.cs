using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class MatchFullTickPacketS2C : INetSerializable
    {
        public int Tick;
        //public SimulationStateS2C PreviousSimulationState; // not sure if gonna need this
        public MatchSimulationStateS2C CurrentSimulationState;
        public FixedUnorderedList<BulletSpawnNetEventS2C> BulletSpawnNetEvents; // todo: remove events related to bullet when bullet id destroyed
        public FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C> PlayerJoinAcceptNetEvents;
        public FixedUnorderedList<PlayerTakeDamageNetEventS2C> PlayerTakeDamageNetEvents;
        public FixedUnorderedList<PlayerDiedNetEventS2C> PlayerDiedNetEvents;
        public FixedUnorderedList<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents;
        public FixedUnorderedList<PlayersSwapNetEventS2C> PlayerSwapNetEvents;
        public FixedClassUnorderedList<TalentCardObtainedNetEventS2C> TalentCardObtainedNetEvents; // todo: remove events related to card when bullet id destroyed
        public FixedUnorderedList<TalentCardHitNetEventS2C> TalentCardHitNetEvents;
        public FixedUnorderedList<PlayerSpinnedStartedNetEventS2C> PlayerSpinnedStartedNetEvents;
        public FixedUnorderedList<PlayerSpinnedEndedNetEventS2C> PlayerSpinnedEndedNetEvents;
        public FixedUnorderedList<PowerUpBallSpawnedNetEventS2C> PowerUpSpawnedNetEvents; // todo: remove events related to power up when bullet id destroyed
        public FixedUnorderedList<PowerUpBallObtainedNetEventS2C> PowerUpObtainedNetEvents;
        public FixedClassUnorderedList<StageEndNetEventS2C> StageEndNetEvents;
        public FixedUnorderedList<TeamLostNetEventS2C> TeamLostNetEvents;
        public FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C> EnvironmentSpringPlayerCollisionNetEvents;
        public FixedUnorderedList<TalentSwitchNetEventS2C> TalentSwitchNetEvents;
        public FixedUnorderedList<GainBoltsNetEventS2C> GainBoltsNetEvents;
        public FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> PlayerToEnvironmentTeleportGateCollisionNetEvents;
        public FixedUnorderedList<PreparationPhaseEndedNetEventS2C> PreparationPhaseEndedNetEvents;
        public FixedUnorderedList<CreateSwapFieldNetEventS2C> CreateSwapFieldNetEvents;
        public FixedUnorderedList<CreateKOProjectileNetEventS2C> CreateKOProjectileNetEvents;
        public FixedUnorderedList<KOProjectHitPlayerNetEventS2C> KOProjectHitPlayerNetEvents;
        public FixedUnorderedList<DeactivateKOTalentNetEventS2C> DeactivateKOTalentNetEvents;
        public FixedUnorderedList<PerformDashPulseNetEventS2C> PerformDashPulseNetEvents;
        public FixedUnorderedList<ActivateSentryGunTalentNetEventS2C> ActivateSentryGunTalentNetEvents;
        public FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C> DeactivateSentryGunTalentNetEvents;
        public FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C> UpdatePlayerTalentStocksNetEvents;
        public FixedUnorderedList<DeactivateSwapTalentNetEventS2C> DestroySwapFieldNetEvents;
        public FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C> PlayerMaxShootCooldownChangedNetEvents;
        public FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C> CreateGrapplingHookProjectileNetEvents;
        public FixedUnorderedList<GrapplingHookHitWallNetEventS2C> GrapplingHookHitWallNetEvents;
        public FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C> DeactivateGrapplingHookTalentNetEvents;
        public FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C> ActivateUmbrellaTalentNetEvents;
        public FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C> DeactivateUmbrellaTalentNetEvents;
        public FixedUnorderedList<CreateMagneticPullFieldNetEventS2C> CreateMagneticPullFieldNetEvents;
        public FixedUnorderedList<LayChickenEggNetEventS2C> LayChickenEggNetEvents;
        public FixedUnorderedList<ChickenEggHitNetEventS2C> ChickenEggHitNetEvents;
        public FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C> ActivateYearsOfPainTalentNetEvents;
        public FixedClassUnorderedList<PlayerLockOnHeartTargetsChangedNetEventS2C> PlayerLockOnHeartTargetsChangedNetEvents;
        public FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C> PlayerLockedOnTargetHitNetEvents;
        
        public MatchFullTickPacketS2C()
        {
            // use this from the server?
        }

        public MatchFullTickPacketS2C(MaxCap maxCap, SharedGamePlayConfig sharedGamePlayConfig)
        {
            CurrentSimulationState = new MatchSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer,
                maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUpBalls, sharedGamePlayConfig.MaxTeamsAmount, maxCap.ConcurrentChickenEggs);

            BulletSpawnNetEvents = new FixedUnorderedList<BulletSpawnNetEventS2C>(maxCap.BulletSpawnNetEvents);

            PlayerJoinAcceptNetEvents = new FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>(maxCap.PlayerJoinAcceptNetEvents,
                () => new PlayerRejoinAcceptPacketS2C(maxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, sharedGamePlayConfig.MaxTeamsAmount));

            PlayerTakeDamageNetEvents = new FixedUnorderedList<PlayerTakeDamageNetEventS2C>(maxCap.PlayerTakeDamageNetEvents);
            PlayerDiedNetEvents = new FixedUnorderedList<PlayerDiedNetEventS2C>(maxCap.PlayerDiedNetEvents);
            BulletDestroyedNetEvents = new FixedUnorderedList<BulletDestroyedNetEventS2C>(maxCap.BulletDestroyedNetEvents);
            PlayerSwapNetEvents = new FixedUnorderedList<PlayersSwapNetEventS2C>(maxCap.PlayerSwapNetEvents);

            TalentCardObtainedNetEvents = new FixedClassUnorderedList<TalentCardObtainedNetEventS2C>(maxCap.TalentCardObtainedNetEvent,
                () => new TalentCardObtainedNetEventS2C(sharedGamePlayConfig.MaxConcurrentTalentsForPlayer));

            TalentCardHitNetEvents = new FixedUnorderedList<TalentCardHitNetEventS2C>(maxCap.TalentCardHitNetEvents);
            PlayerSpinnedStartedNetEvents = new FixedUnorderedList<PlayerSpinnedStartedNetEventS2C>(maxCap.PlayerSpinnedStartedNetEvents);
            PlayerSpinnedEndedNetEvents = new FixedUnorderedList<PlayerSpinnedEndedNetEventS2C>(maxCap.PlayerSpinnedEndedNetEvents);
            PowerUpSpawnedNetEvents = new FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>(maxCap.PowerUpSpawnedNetEvents);
            PowerUpObtainedNetEvents = new FixedUnorderedList<PowerUpBallObtainedNetEventS2C>(maxCap.PowerUpObtainedNetEvents);
            StageEndNetEvents = new FixedClassUnorderedList<StageEndNetEventS2C>(maxCap.StageEndNetEvents, () => new StageEndNetEventS2C(sharedGamePlayConfig.MaxTeamsAmount));
            TeamLostNetEvents = new FixedUnorderedList<TeamLostNetEventS2C>(sharedGamePlayConfig.MaxTeamsAmount);
            TalentSwitchNetEvents = new FixedUnorderedList<TalentSwitchNetEventS2C>(maxCap.TalentSwitchNetEvents);
            EnvironmentSpringPlayerCollisionNetEvents = new FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>(maxCap.EnvironmentSpringPlayerCollisionNetEvents);
            GainBoltsNetEvents = new FixedUnorderedList<GainBoltsNetEventS2C>(maxCap.GainBoltsNetEvents);

            PlayerToEnvironmentTeleportGateCollisionNetEvents =
                new FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>(maxCap.PlayerToEnvironmentTeleportGateCollisionNetEvents);

            PreparationPhaseEndedNetEvents = new FixedUnorderedList<PreparationPhaseEndedNetEventS2C>(maxCap.PreparationPhaseEndedNetEvents);
            CreateSwapFieldNetEvents = new FixedUnorderedList<CreateSwapFieldNetEventS2C>(maxCap.CreateSwapFieldNetEvents);
            DestroySwapFieldNetEvents = new FixedUnorderedList<DeactivateSwapTalentNetEventS2C>(maxCap.DestroySwapFieldNetEvents);
            KOProjectHitPlayerNetEvents = new FixedUnorderedList<KOProjectHitPlayerNetEventS2C>(maxCap.KOProjectHitPlayerNetEvents);
            CreateKOProjectileNetEvents = new FixedUnorderedList<CreateKOProjectileNetEventS2C>(maxCap.CreateKOProjectileNetEvents);
            DeactivateKOTalentNetEvents = new FixedUnorderedList<DeactivateKOTalentNetEventS2C>(maxCap.DeactivateKOTalentNetEvents);
            PerformDashPulseNetEvents = new FixedUnorderedList<PerformDashPulseNetEventS2C>(maxCap.PerformDashPulseNetEvents);
            ActivateSentryGunTalentNetEvents = new FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>(maxCap.ActivateSentryGunTalentNetEvents);
            DeactivateSentryGunTalentNetEvents = new FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>(maxCap.DeactivateSentryGunTalentNetEvents);
            UpdatePlayerTalentStocksNetEvents = new FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>(maxCap.UpdatePlayerTalentStocksNetEvents);
            PlayerMaxShootCooldownChangedNetEvents = new FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>(maxCap.PlayerMaxShootCooldownChangedNetEvents);
            ActivateSentryGunTalentNetEvents = new FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>(maxCap.ActivateSentryGunTalentNetEvents);
            DeactivateSentryGunTalentNetEvents = new FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>(maxCap.DeactivateSentryGunTalentNetEvents);
            CreateGrapplingHookProjectileNetEvents = new FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>(maxCap.CreateGrapplingHookProjectileNetEvents);
            GrapplingHookHitWallNetEvents = new FixedUnorderedList<GrapplingHookHitWallNetEventS2C>(maxCap.GrapplingHookHitWallNetEvents);
            DeactivateGrapplingHookTalentNetEvents = new FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>(maxCap.DeactivateGrapplingHookTalentNetEvents);
            CreateMagneticPullFieldNetEvents = new FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>(maxCap.CreateMagneticPullFieldNetEvents);
            ActivateYearsOfPainTalentNetEvents = new FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C>(maxCap.ActivateYearsOfPainTalentNetEvents);
            ActivateUmbrellaTalentNetEvents = new FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C>(maxCap.ActivateUmbrellaTalentNetEvents);
            DeactivateUmbrellaTalentNetEvents = new FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C>(maxCap.DeactivateUmbrellaTalentNetEvents);
            LayChickenEggNetEvents = new FixedUnorderedList<LayChickenEggNetEventS2C>(maxCap.LayChickenEggNetEvents);
            ChickenEggHitNetEvents = new FixedUnorderedList<ChickenEggHitNetEventS2C>(maxCap.ChickenEggHitNetEvents);
            PlayerLockOnHeartTargetsChangedNetEvents = new FixedClassUnorderedList<PlayerLockOnHeartTargetsChangedNetEventS2C>(maxCap.PlayerLockOnHeartTargetsChangedNetEvents, () => new PlayerLockOnHeartTargetsChangedNetEventS2C(maxCap.ConcurrentEnemyPlayers));
            PlayerLockedOnTargetHitNetEvents = new FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C>(maxCap.ConcurrentPlayers);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            CurrentSimulationState.SerializeDeltas(writer);
            SerializedPlayerJoinedEvents(writer);
            SerializedBulletSpawnedEvents(writer);
            SerializedPlayerTakeDamageEvents(writer);
            SerializedPlayerDiedEvents(writer);
            SerializedPlayerLockOnHeartTargetsChangedNetEvents(writer);
            SerializedPlayerLockedOnTargetHitNetEvents(writer);
            SerializedBulletDestroyedEvents(writer);
            SerializedPlayerSwapEvents(writer);
            SerializedTalentCardObtainedEvents(writer);
            SerializedTalentCardHitEvents(writer);
            SerializedPowerUpSpawnedEvents(writer);
            SerializedPowerUpObtainedEvents(writer);
            SerializedStageEndEvents(writer);
            SerializedTeamLostEvents(writer);
            SerializedTalentSwitchEvents(writer);
            SerializedEnvironmentSpringPlayerCollisionEvents(writer);
            SerializedGainBoltsEvents(writer);
            SerializedPlayerToEnvironmentTeleportGateCollisionEvents(writer);
            SerializedPreparationPhaseEndedEvents(writer);
            SerializedCreateSwapFieldNetEvents(writer);
            SerializedCreateKOProjectileNetEvents(writer);
            SerializedKOProjectHitPlayerNetEvents(writer);
            SerializedDeactivateKOTalentNetEvents(writer);
            SerializedPerformDashPulseNetEvents(writer);
            SerializedActivateSentryGunTalentNetEvents(writer);
            SerializedDeactivateSentryGunTalentNetEvents(writer);
            SerializedUpdatePlayerTalentStocksNetEvents(writer);
            SerializedPlayerSpinnedStartedEvents(writer);
            SerializedPlayerSpinnedEndedEvents(writer);
            SerializedDestroySwapFieldNetEvents(writer);
            SerializedPlayerMaxShootCooldownChangedNetEvents(writer);
            SerializedCreateGrapplingHookProjectileNetEvents(writer);
            SerializedGrapplingHookHitWallNetEvents(writer);
            SerializedDeactivateGrapplingHookTalentNetEvents(writer);
            SerializedCreateMagneticPullFieldNetEvents(writer);
            SerializedActivateUmbrellaTalentNetEvents(writer);
            SerializedDeactivateUmbrellaTalentNetEvents(writer);
            SerializedLayChickenEggNetEvents(writer);
            SerializedChickenEggHitNetEvents(writer);
            SerializedActivateYearsOfPainTalentNetEvents(writer);
        }

        private void SerializedKOProjectHitPlayerNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)KOProjectHitPlayerNetEvents.Count);
            foreach (var netEvent in KOProjectHitPlayerNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedCreateKOProjectileNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateKOProjectileNetEvents.Count);
            foreach (var netEvent in CreateKOProjectileNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedDeactivateKOTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateKOTalentNetEvents.Count);
            foreach (var netEvent in DeactivateKOTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }
        
        private void SerializedCreateSwapFieldNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateSwapFieldNetEvents.Count);
            foreach (var netEvent in CreateSwapFieldNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedDestroySwapFieldNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DestroySwapFieldNetEvents.Count);
            foreach (var netEvent in DestroySwapFieldNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedPreparationPhaseEndedEvents(NetDataWriter writer)
        {
            writer.Put((byte)PreparationPhaseEndedNetEvents.Count);
            foreach (var netEvent in PreparationPhaseEndedNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedGainBoltsEvents(NetDataWriter writer)
        {
            writer.Put((byte) GainBoltsNetEvents.Count);
            foreach (var netEvent in GainBoltsNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedPlayerToEnvironmentTeleportGateCollisionEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerToEnvironmentTeleportGateCollisionNetEvents.Count);
            foreach (var netEvent in PlayerToEnvironmentTeleportGateCollisionNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedTalentSwitchEvents(NetDataWriter writer)
        {
            writer.Put((byte) TalentSwitchNetEvents.Count);
            foreach (var netEvent in TalentSwitchNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedEnvironmentSpringPlayerCollisionEvents(NetDataWriter writer)
        {
            writer.Put((byte)EnvironmentSpringPlayerCollisionNetEvents.Count);
            foreach (var netEvent in EnvironmentSpringPlayerCollisionNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedTeamLostEvents(NetDataWriter writer)
        {
            writer.Put((byte) TeamLostNetEvents.Count);
            foreach (var netEvent in TeamLostNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedStageEndEvents(NetDataWriter writer)
        {
            writer.Put((byte) StageEndNetEvents.Count);
            foreach (var netEvent in StageEndNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedPowerUpObtainedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PowerUpObtainedNetEvents.Count);
            foreach (var netEvent in PowerUpObtainedNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedPowerUpSpawnedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PowerUpSpawnedNetEvents.Count);
            foreach (var netEvent in PowerUpSpawnedNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedTalentCardHitEvents(NetDataWriter writer)
        {
            writer.Put((byte) TalentCardHitNetEvents.Count);
            foreach (var talentCardHitNetEvent in TalentCardHitNetEvents.AsSpan())
            {
                talentCardHitNetEvent.Serialize(writer);
            }
        }

        private void SerializedPlayerSwapEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerSwapNetEvents.Count);
            foreach (var playerSwapNetEvent in PlayerSwapNetEvents.AsSpan())
            {
                playerSwapNetEvent.Serialize(writer);
            }
        }

        private void SerializedTalentCardObtainedEvents(NetDataWriter writer)
        {
            writer.Put((byte) TalentCardObtainedNetEvents.Count);
            foreach (var talentCardObtained in TalentCardObtainedNetEvents.AsSpan())
            {
                talentCardObtained.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            CurrentSimulationState.DeserializeTransforms(reader);
            DeserializedPlayerJoinedEvents(reader);
            DeserializedBulletSpawnedEvents(reader);
            DeserializedPlayerTakeDamageEvents(reader);
            DeserializedPlayerDiedEvents(reader);
            DeserializedPlayerLockOnHeartTargetsChangedNetEvents(reader);
            DeserializedPlayerLockedOnTargetHitNetEvents(reader);
            DeserializedBulletDestroyedEvents(reader);
            DeserializedPlayerSwapEvents(reader);
            DeserializedTalentCardObtainedEvents(reader);
            DeserializedTalentCardHitEvents(reader);
            DeserializedPowerUpSpawnedEvents(reader);
            DeserializedPowerUpObtainedEvents(reader);
            DeserializedStageEndEvents(reader);
            DeserializedTeamLostEvents(reader);
            DeserializedTalentSwitchEvents(reader);
            DeserializedEnvironmentSpringPlayerCollisionEvents(reader);
            DeserializedGainBoltsEvents(reader);
            DeserializedPlayerToEnvironmentTeleportGateCollisionEvents(reader);
            DeserializedPreparationPhaseEndedEvents(reader);
            DeserializedCreateSwapFieldNetEvents(reader);
            DeserializedCreateKOProjectileNetEvents(reader);
            DeserializedKOProjectHitPlayerNetEvents(reader);
            DeserializedDeactivateKOTalentNetEvents(reader);
            DeserializedPerformDashPulseNetEvents(reader);
            DeserializedActivateSentryGunTalentNetEvents(reader);
            DeserializedDeactivateSentryGunTalentNetEvents(reader);
            DeserializedUpdatePlayerTalentStocksNetEvents(reader);
            DeserializedPlayerSpinnedStartedEvents(reader);
            DeserializedPlayerSpinnedEndedEvents(reader);
            DeserializedDestroySwapFieldNetEvents(reader);
            DeserializedPlayerMaxShootCooldownChangedNetEvents(reader);
            DeserializedCreateGrapplingHookProjectileNetEvents(reader);
            DeserializedGrapplingHookHitWallNetEvents(reader);
            DeserializedDeactivateGrapplingHookTalentNetEvents(reader);
            DeserializedCreateMagneticPullFieldNetEvents(reader);
            DeserializedActivateUmbrellaTalentNetEvents(reader);
            DeserializedDeactivateUmbrellaTalentNetEvents(reader);
            DeserializedLayChickenEggNetEvents(reader);
            DeserializedChickenEggHitNetEvents(reader);
            DeserializedActivateYearsOfPainTalentNetEvents(reader);
        }

        private void DeserializedCreateKOProjectileNetEvents(NetDataReader reader)
        {
            CreateKOProjectileNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref CreateKOProjectileNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedKOProjectHitPlayerNetEvents(NetDataReader reader)
        {
            KOProjectHitPlayerNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref KOProjectHitPlayerNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedDeactivateSentryGunTalentNetEvents(NetDataReader reader)
        {
            DeactivateSentryGunTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateSentryGunTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedDeactivateKOTalentNetEvents(NetDataReader reader)
        {
            DeactivateKOTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateKOTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }
        private void DeserializedCreateSwapFieldNetEvents(NetDataReader reader)
        {
            CreateSwapFieldNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref CreateSwapFieldNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedDestroySwapFieldNetEvents(NetDataReader reader)
        {
            DestroySwapFieldNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref DestroySwapFieldNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedPreparationPhaseEndedEvents(NetDataReader reader)
        {
            PreparationPhaseEndedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PreparationPhaseEndedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedGainBoltsEvents(NetDataReader reader)
        {
            GainBoltsNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref GainBoltsNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedPlayerToEnvironmentTeleportGateCollisionEvents(NetDataReader reader)
        {
            PlayerToEnvironmentTeleportGateCollisionNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PlayerToEnvironmentTeleportGateCollisionNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedTalentSwitchEvents(NetDataReader reader)
        {
            TalentSwitchNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref TalentSwitchNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedEnvironmentSpringPlayerCollisionEvents(NetDataReader reader)
        {
            EnvironmentSpringPlayerCollisionNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref EnvironmentSpringPlayerCollisionNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedTeamLostEvents(NetDataReader reader)
        {
            TeamLostNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref TeamLostNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedStageEndEvents(NetDataReader reader)
        {
            StageEndNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                var netEvent = StageEndNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedPowerUpObtainedEvents(NetDataReader reader)
        {
            PowerUpObtainedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PowerUpObtainedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedPowerUpSpawnedEvents(NetDataReader reader)
        {
            PowerUpSpawnedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PowerUpSpawnedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedTalentCardHitEvents(NetDataReader reader)
        {
            TalentCardHitNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var talentCardHitEvent = ref TalentCardHitNetEvents.AddAndGet();
                talentCardHitEvent.Deserialize(reader);
            }
        }
        
        private void DeserializedTalentCardObtainedEvents(NetDataReader reader)
        {
            TalentCardObtainedNetEvents.Clear();
            var talentCardObtainedCount = reader.GetByte();
            for (var i = 0; i < talentCardObtainedCount; i++)
            {
                var talentCardObtainedEvent = TalentCardObtainedNetEvents.AddAndGet();
                talentCardObtainedEvent.Deserialize(reader);
            }
        }
        
        private void DeserializedPlayerSwapEvents(NetDataReader reader)
        {
            PlayerSwapNetEvents.Clear();
            var playerSwapEventsCount = reader.GetByte();
            for (var i = 0; i < playerSwapEventsCount; i++)
            {
                ref var playersSwapEvent = ref PlayerSwapNetEvents.AddAndGet();
                playersSwapEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerTakeDamageEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerTakeDamageNetEvents.Count);
            foreach (var playerTakeDamageEvent in PlayerTakeDamageNetEvents.AsSpan())
            {
                playerTakeDamageEvent.Serialize(writer);
            }
        }

        private void SerializedPlayerDiedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerDiedNetEvents.Count);
            foreach (var playerDiedNetEvent in PlayerDiedNetEvents.AsSpan())
            {
                playerDiedNetEvent.Serialize(writer);
            }
        }

        private void DeserializedPlayerTakeDamageEvents(NetDataReader reader)
        {
            PlayerTakeDamageNetEvents.Clear();
            var playerTakeDamageEventsCount = reader.GetByte();
            for (var i = 0; i < playerTakeDamageEventsCount; i++)
            {
                ref var playerTakeDamageEvent = ref PlayerTakeDamageNetEvents.AddAndGet();
                playerTakeDamageEvent.Deserialize(reader);
            }
        }

        private void DeserializedPlayerDiedEvents(NetDataReader reader)
        {
            PlayerDiedNetEvents.Clear();
            var playerDiedEventsCount = reader.GetByte();
            for (var i = 0; i < playerDiedEventsCount; i++)
            {
                ref var playerDiedEvent = ref PlayerDiedNetEvents.AddAndGet();
                playerDiedEvent.Deserialize(reader);
            }
        }

        private void SerializedBulletDestroyedEvents(NetDataWriter writer)
        {
            writer.Put((byte) BulletDestroyedNetEvents.Count);
            foreach (var bulletDestroyedEvent in BulletDestroyedNetEvents.AsSpan())
            {
                bulletDestroyedEvent.Serialize(writer);
            }
        }

        private void DeserializedBulletDestroyedEvents(NetDataReader reader)
        {
            BulletDestroyedNetEvents.Clear();
            var bulletDestroyedEventsCount = reader.GetByte();
            for (var i = 0; i < bulletDestroyedEventsCount; i++)
            {
                ref var bulletDestroyedEvent = ref BulletDestroyedNetEvents.AddAndGet();
                bulletDestroyedEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerJoinedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerJoinAcceptNetEvents.Count);
            foreach (var playerJoinAcceptNetEvent in PlayerJoinAcceptNetEvents.AsSpan())
            {
                playerJoinAcceptNetEvent.Serialize(writer);
            }
        }

        private void DeserializedPlayerJoinedEvents(NetDataReader reader)
        {
            PlayerJoinAcceptNetEvents.Clear();
            var playerJoinedNetEventsCount = reader.GetByte();
            for (var i = 0; i < playerJoinedNetEventsCount; i++)
            {
                var playerJoinAcceptPacket = PlayerJoinAcceptNetEvents.AddAndGet();
                playerJoinAcceptPacket.Deserialize(reader);
            }
        }

        private void SerializedBulletSpawnedEvents(NetDataWriter writer)
        {
            var bulletSpawnedAmount = BulletSpawnNetEvents.Count;
            writer.Put((byte) bulletSpawnedAmount);
            foreach (var bulletSpawnEvent in BulletSpawnNetEvents.AsSpan())
            {
                bulletSpawnEvent.Serialize(writer);
            }
        }

        private void DeserializedBulletSpawnedEvents(NetDataReader reader)
        {
            BulletSpawnNetEvents.Clear();
            var bulletSpawnNetEventsCount = reader.GetByte();
            for (var i = 0; i < bulletSpawnNetEventsCount; i++)
            {
                ref var bulletSpawnEvent = ref BulletSpawnNetEvents.AddAndGet();
                bulletSpawnEvent.Deserialize(reader);
            }
        }

        private void SerializedPerformDashPulseNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PerformDashPulseNetEvents.Count);
            foreach (var netEvent in PerformDashPulseNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedPerformDashPulseNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            PerformDashPulseNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref PerformDashPulseNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateSentryGunTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateSentryGunTalentNetEvents.Count);
            foreach (var netEvent in ActivateSentryGunTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedDeactivateSentryGunTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateSentryGunTalentNetEvents.Count);
            foreach (var netEvent in DeactivateSentryGunTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }
        
        private void DeserializedActivateSentryGunTalentNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            ActivateSentryGunTalentNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateSentryGunTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerSpinnedStartedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerSpinnedStartedNetEvents.Count);
            foreach (var evt in PlayerSpinnedStartedNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedPlayerSpinnedStartedEvents(NetDataReader reader)
        {
            PlayerSpinnedStartedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref PlayerSpinnedStartedNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void SerializedPlayerSpinnedEndedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerSpinnedEndedNetEvents.Count);
            foreach (var evt in PlayerSpinnedEndedNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedPlayerSpinnedEndedEvents(NetDataReader reader)
        {
            PlayerSpinnedEndedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref PlayerSpinnedEndedNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void SerializedUpdatePlayerTalentStocksNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)UpdatePlayerTalentStocksNetEvents.Count);
            foreach (var netEvent in UpdatePlayerTalentStocksNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedPlayerMaxShootCooldownChangedNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerMaxShootCooldownChangedNetEvents.Count);
            foreach (var netEvent in PlayerMaxShootCooldownChangedNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedUpdatePlayerTalentStocksNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            UpdatePlayerTalentStocksNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref UpdatePlayerTalentStocksNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedPlayerMaxShootCooldownChangedNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            PlayerMaxShootCooldownChangedNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref PlayerMaxShootCooldownChangedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedCreateGrapplingHookProjectileNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateGrapplingHookProjectileNetEvents.Count);
            foreach (var netEvent in CreateGrapplingHookProjectileNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedGrapplingHookHitWallNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)GrapplingHookHitWallNetEvents.Count);
            foreach (var netEvent in GrapplingHookHitWallNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedDeactivateGrapplingHookTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateGrapplingHookTalentNetEvents.Count);
            foreach (var netEvent in DeactivateGrapplingHookTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedCreateGrapplingHookProjectileNetEvents(NetDataReader reader)
        {
            CreateGrapplingHookProjectileNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref CreateGrapplingHookProjectileNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedGrapplingHookHitWallNetEvents(NetDataReader reader)
        {
            GrapplingHookHitWallNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref GrapplingHookHitWallNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedDeactivateGrapplingHookTalentNetEvents(NetDataReader reader)
        {
            DeactivateGrapplingHookTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateGrapplingHookTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateUmbrellaTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateUmbrellaTalentNetEvents.Count);
            foreach (var netEvent in ActivateUmbrellaTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedActivateUmbrellaTalentNetEvents(NetDataReader reader)
        {
            ActivateUmbrellaTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateUmbrellaTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedDeactivateUmbrellaTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateUmbrellaTalentNetEvents.Count);
            foreach (var netEvent in DeactivateUmbrellaTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedDeactivateUmbrellaTalentNetEvents(NetDataReader reader)
        {
            DeactivateUmbrellaTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateUmbrellaTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedCreateMagneticPullFieldNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateMagneticPullFieldNetEvents.Count);
            foreach (var netEvent in CreateMagneticPullFieldNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedCreateMagneticPullFieldNetEvents(NetDataReader reader)
        {
            CreateMagneticPullFieldNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref CreateMagneticPullFieldNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }
        
        private void SerializedLayChickenEggNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)LayChickenEggNetEvents.Count);

            foreach (var netEvent in LayChickenEggNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedLayChickenEggNetEvents(NetDataReader reader)
        {
            LayChickenEggNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref LayChickenEggNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedChickenEggHitNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ChickenEggHitNetEvents.Count);

            foreach (var netEvent in ChickenEggHitNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedChickenEggHitNetEvents(NetDataReader reader)
        {
            ChickenEggHitNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref ChickenEggHitNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateYearsOfPainTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateYearsOfPainTalentNetEvents.Count);
            foreach (var netEvent in ActivateYearsOfPainTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedActivateYearsOfPainTalentNetEvents(NetDataReader reader)
        {
            ActivateYearsOfPainTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateYearsOfPainTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerLockOnHeartTargetsChangedNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerLockOnHeartTargetsChangedNetEvents.Count);
            foreach (var netEvent in PlayerLockOnHeartTargetsChangedNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedPlayerLockOnHeartTargetsChangedNetEvents(NetDataReader reader)
        {
            for (int i = 0; i < PlayerLockOnHeartTargetsChangedNetEvents.Count; i++)
            {
                PlayerLockOnHeartTargetsChangedNetEvents[i].PlayerIdsLockedOnTarget.Clear();
            }
            
            PlayerLockOnHeartTargetsChangedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                var netEvent = PlayerLockOnHeartTargetsChangedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerLockedOnTargetHitNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerLockedOnTargetHitNetEvents.Count);
            foreach (var netEvent in PlayerLockedOnTargetHitNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedPlayerLockedOnTargetHitNetEvents(NetDataReader reader)
        {
            PlayerLockedOnTargetHitNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PlayerLockedOnTargetHitNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }
    }
}
