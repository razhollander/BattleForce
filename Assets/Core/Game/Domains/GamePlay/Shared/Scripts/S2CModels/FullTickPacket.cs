using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public class FullTickPacket : INetSerializable
    {
        public int Tick;
        //public SimulationStateS2C PreviousSimulationState; // not sure if gonna need this
        public SimulationStateS2C CurrentSimulationState;
        public FixedUnorderedList<BulletSpawnNetEventS2C> BulletSpawnNetEvents; // todo: remove events related to bullet when bullet id destroyed
        public FixedClassUnorderedList<PlayerJoinAcceptPacketS2C> PlayerJoinAcceptNetEvents;
        public FixedUnorderedList<PlayerTakeDamageNetEventS2C> PlayerTakeDamageNetEvents;
        public FixedUnorderedList<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents;
        public FixedUnorderedList<PlayersSwapNetEventS2C> PlayerSwapNetEvents;
        public FixedUnorderedList<TalentCardObtainedNetEventS2C> TalentCardObtainedNetEvents;
        public FixedUnorderedList<TalentCardHitNetEventS2C> TalentCardHitNetEvents;
        public FixedUnorderedList<PowerUpSpawnedNetEventsS2C> PowerUpSpawnedNetEvents;
        public FixedUnorderedList<PowerUpObtainedNetEventS2C> PowerUpObtainedNetEvents;

        public FullTickPacket()
        {
        }
        
        public FullTickPacket(MaxCap maxCap, SharedGamePlayConfig sharedGamePlayConfig)
        {
            CurrentSimulationState = new SimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUps);
            BulletSpawnNetEvents = new FixedUnorderedList<BulletSpawnNetEventS2C>(maxCap.BulletSpawnNetEvents);
            PlayerJoinAcceptNetEvents = new FixedClassUnorderedList<PlayerJoinAcceptPacketS2C>(maxCap.PlayerJoinAcceptNetEvents, () => new PlayerJoinAcceptPacketS2C(maxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer));
            PlayerTakeDamageNetEvents = new FixedUnorderedList<PlayerTakeDamageNetEventS2C>(maxCap.PlayerTakeDamageNetEvents);
            BulletDestroyedNetEvents = new FixedUnorderedList<BulletDestroyedNetEventS2C>(maxCap.BulletDestroyedNetEvents);
            PlayerSwapNetEvents = new FixedUnorderedList<PlayersSwapNetEventS2C>(maxCap.PlayerSwapNetEvents);
            TalentCardObtainedNetEvents = new FixedUnorderedList<TalentCardObtainedNetEventS2C>(maxCap.TalentCardObtainedNetEvent);
            TalentCardHitNetEvents = new FixedUnorderedList<TalentCardHitNetEventS2C>(maxCap.TalentCardHitNetEvents);
            PowerUpSpawnedNetEvents = new FixedUnorderedList<PowerUpSpawnedNetEventsS2C>(maxCap.PowerUpSpawnedNetEvents);
            PowerUpObtainedNetEvents = new FixedUnorderedList<PowerUpObtainedNetEventS2C>(maxCap.PowerUpObtainedNetEvents);
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
            CurrentSimulationState.SerializeTransforms(writer);
            SerializedPlayerJoinedEvents(writer);
            SerializedBulletSpawnedEvents(writer);
            SerializedPlayerTakeDamageEvents(writer);
            SerializedBulletDestroyedEvents(writer);
            SerializedPlayerSwapEvents(writer);
            SerializedTalentCardObtainedEvents(writer);
            SerializedTalentCardHitEvents(writer);
            SerializedPowerUpSpawnedEvents(writer);
            SerializedPowerUpObtainedEvents(writer);
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
            DeserializedBulletDestroyedEvents(reader);
            DeserializedPlayerSwapEvents(reader);
            DeserializedTalentCardObtainedEvents(reader);
            DeserializedTalentCardHitEvents(reader);
            DeserializedPowerUpSpawnedEvents(reader);
            DeserializedPowerUpObtainedEvents(reader);
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
                ref var talentCardObtainedEvent = ref TalentCardObtainedNetEvents.AddAndGet();
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
    }
}