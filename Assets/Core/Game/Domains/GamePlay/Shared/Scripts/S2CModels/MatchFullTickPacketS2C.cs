using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
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

        public MatchFullTickPacketS2C()
        {
            // use this from the server?
        }
        
        public MatchFullTickPacketS2C(MaxCap maxCap, SharedGamePlayConfig sharedGamePlayConfig)
        {
            CurrentSimulationState = new MatchSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUpBalls, sharedGamePlayConfig.MaxTeamsAmount);
            BulletSpawnNetEvents = new FixedUnorderedList<BulletSpawnNetEventS2C>(maxCap.BulletSpawnNetEvents);
            PlayerJoinAcceptNetEvents = new FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>(maxCap.PlayerJoinAcceptNetEvents, () => new PlayerRejoinAcceptPacketS2C(maxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, sharedGamePlayConfig.MaxTeamsAmount));
            PlayerTakeDamageNetEvents = new FixedUnorderedList<PlayerTakeDamageNetEventS2C>(maxCap.PlayerTakeDamageNetEvents);
            PlayerDiedNetEvents = new FixedUnorderedList<PlayerDiedNetEventS2C>(maxCap.PlayerDiedNetEvents);
            BulletDestroyedNetEvents = new FixedUnorderedList<BulletDestroyedNetEventS2C>(maxCap.BulletDestroyedNetEvents);
            PlayerSwapNetEvents = new FixedUnorderedList<PlayersSwapNetEventS2C>(maxCap.PlayerSwapNetEvents);
            TalentCardObtainedNetEvents = new FixedClassUnorderedList<TalentCardObtainedNetEventS2C>(maxCap.TalentCardObtainedNetEvent, () => new TalentCardObtainedNetEventS2C(sharedGamePlayConfig.MaxConcurrentTalentsForPlayer));
            TalentCardHitNetEvents = new FixedUnorderedList<TalentCardHitNetEventS2C>(maxCap.TalentCardHitNetEvents);
            PowerUpSpawnedNetEvents = new FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>(maxCap.PowerUpSpawnedNetEvents);
            PowerUpObtainedNetEvents = new FixedUnorderedList<PowerUpBallObtainedNetEventS2C>(maxCap.PowerUpObtainedNetEvents);
            StageEndNetEvents = new FixedClassUnorderedList<StageEndNetEventS2C>(maxCap.StageEndNetEvents, () => new StageEndNetEventS2C(sharedGamePlayConfig.MaxTeamsAmount));
            TeamLostNetEvents = new FixedUnorderedList<TeamLostNetEventS2C>(sharedGamePlayConfig.MaxTeamsAmount);
            TalentSwitchNetEvents = new FixedUnorderedList<TalentSwitchNetEventS2C>(maxCap.TalentSwitchNetEvents);
            EnvironmentSpringPlayerCollisionNetEvents = new FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>(maxCap.EnvironmentSpringPlayerCollisionNetEvents);
            GainBoltsNetEvents = new FixedUnorderedList<GainBoltsNetEventS2C>(maxCap.GainBoltsNetEvents);
            PlayerToEnvironmentTeleportGateCollisionNetEvents = new FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>(maxCap.PlayerToEnvironmentTeleportGateCollisionNetEvents);
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
        }
        
        // public FullTickPacket(int tick, SimulationStateS2C previousSimulationState,
        //     SimulationStateS2C currentSimulationState, List<BulletSpawnNetEventS2C> bulletSpawnNetEvents,
        //     List<PlayerJoinAcceptPacketS2C> playerJoinAcceptNetEvents, List<PlayerTakeDamageNetEventS2C> playerTakeDamageNetEvents,
        //     List<BulletDestroyedNetEventS2C> bulletDestroyedNetEvents)
        // {
        //     Tick = tick;
        //     //PreviousSimulationState = previousSimulationState;
        //     CurrentSimulationState = currentSimulationState;
        //     BulletSpawnNetEvents = bulletSpawnNetEvents;
        //     PlayerJoinAcceptNetEvents = playerJoinAcceptNetEvents;
        //     PlayerTakeDamageNetEvents = playerTakeDamageNetEvents;
        //     BulletDestroyedNetEvents = bulletDestroyedNetEvents;
        // }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            CurrentSimulationState.SerializeDeltas(writer);
            SerializedPlayerJoinedEvents(writer);
            SerializedBulletSpawnedEvents(writer);
            SerializedPlayerTakeDamageEvents(writer);
            SerializedPlayerDiedEvents(writer);
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
            SerializedDestroySwapFieldNetEvents(writer);
        }

        private void SerializedKOProjectHitPlayerNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)KOProjectHitPlayerNetEvents.Count);
            foreach (var evt in KOProjectHitPlayerNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedCreateKOProjectileNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateKOProjectileNetEvents.Count);
            foreach (var evt in CreateKOProjectileNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedDeactivateKOTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateKOTalentNetEvents.Count);
            foreach (var evt in DeactivateKOTalentNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }
        
        private void SerializedCreateSwapFieldNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateSwapFieldNetEvents.Count);
            foreach (var evt in CreateSwapFieldNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedDestroySwapFieldNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DestroySwapFieldNetEvents.Count);
            foreach (var evt in DestroySwapFieldNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedPreparationPhaseEndedEvents(NetDataWriter writer)
        {
            writer.Put((byte)PreparationPhaseEndedNetEvents.Count);
            foreach (var evt in PreparationPhaseEndedNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedGainBoltsEvents(NetDataWriter writer)
        {
            writer.Put((byte) GainBoltsNetEvents.Count);
            foreach (var evt in GainBoltsNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedPlayerToEnvironmentTeleportGateCollisionEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerToEnvironmentTeleportGateCollisionNetEvents.Count);
            foreach (var evt in PlayerToEnvironmentTeleportGateCollisionNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedTalentSwitchEvents(NetDataWriter writer)
        {
            writer.Put((byte) TalentSwitchNetEvents.Count);
            foreach (var evt in TalentSwitchNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedEnvironmentSpringPlayerCollisionEvents(NetDataWriter writer)
        {
            writer.Put((byte)EnvironmentSpringPlayerCollisionNetEvents.Count);
            foreach (var evt in EnvironmentSpringPlayerCollisionNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedTeamLostEvents(NetDataWriter writer)
        {
            writer.Put((byte) TeamLostNetEvents.Count);
            foreach (var evt in TeamLostNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedStageEndEvents(NetDataWriter writer)
        {
            writer.Put((byte) StageEndNetEvents.Count);
            foreach (var evt in StageEndNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedPowerUpObtainedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PowerUpObtainedNetEvents.Count);
            foreach (var evt in PowerUpObtainedNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedPowerUpSpawnedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PowerUpSpawnedNetEvents.Count);
            foreach (var evt in PowerUpSpawnedNetEvents.AsSpan())
            {
                evt.Serialize(writer);
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
            DeserializedDestroySwapFieldNetEvents(reader);
        }

        private void DeserializedCreateKOProjectileNetEvents(NetDataReader reader)
        {
            CreateKOProjectileNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref CreateKOProjectileNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedKOProjectHitPlayerNetEvents(NetDataReader reader)
        {
            KOProjectHitPlayerNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref KOProjectHitPlayerNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedDeactivateKOTalentNetEvents(NetDataReader reader)
        {
            DeactivateKOTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref DeactivateKOTalentNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }
        private void DeserializedCreateSwapFieldNetEvents(NetDataReader reader)
        {
            CreateSwapFieldNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref CreateSwapFieldNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedDestroySwapFieldNetEvents(NetDataReader reader)
        {
            DestroySwapFieldNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref DestroySwapFieldNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedPreparationPhaseEndedEvents(NetDataReader reader)
        {
            PreparationPhaseEndedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref PreparationPhaseEndedNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedGainBoltsEvents(NetDataReader reader)
        {
            GainBoltsNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref GainBoltsNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedPlayerToEnvironmentTeleportGateCollisionEvents(NetDataReader reader)
        {
            PlayerToEnvironmentTeleportGateCollisionNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref PlayerToEnvironmentTeleportGateCollisionNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedTalentSwitchEvents(NetDataReader reader)
        {
            TalentSwitchNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref TalentSwitchNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedEnvironmentSpringPlayerCollisionEvents(NetDataReader reader)
        {
            EnvironmentSpringPlayerCollisionNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref EnvironmentSpringPlayerCollisionNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedTeamLostEvents(NetDataReader reader)
        {
            TeamLostNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref TeamLostNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedStageEndEvents(NetDataReader reader)
        {
            StageEndNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                var evt = StageEndNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedPowerUpObtainedEvents(NetDataReader reader)
        {
            PowerUpObtainedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref PowerUpObtainedNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedPowerUpSpawnedEvents(NetDataReader reader)
        {
            PowerUpSpawnedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref PowerUpSpawnedNetEvents.AddAndGet();
                evt.Deserialize(reader);
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
            foreach (var evt in PerformDashPulseNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedPerformDashPulseNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            PerformDashPulseNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var evt = ref PerformDashPulseNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void SerializedActivateSentryGunTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateSentryGunTalentNetEvents.Count);
            foreach (var evt in ActivateSentryGunTalentNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedActivateSentryGunTalentNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            ActivateSentryGunTalentNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var evt = ref ActivateSentryGunTalentNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void SerializedDeactivateSentryGunTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateSentryGunTalentNetEvents.Count);
            foreach (var evt in DeactivateSentryGunTalentNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedDeactivateSentryGunTalentNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            DeactivateSentryGunTalentNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var evt = ref DeactivateSentryGunTalentNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void SerializedUpdatePlayerTalentStocksNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)UpdatePlayerTalentStocksNetEvents.Count);
            foreach (var evt in UpdatePlayerTalentStocksNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedUpdatePlayerTalentStocksNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            UpdatePlayerTalentStocksNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var evt = ref UpdatePlayerTalentStocksNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }
    }
}
