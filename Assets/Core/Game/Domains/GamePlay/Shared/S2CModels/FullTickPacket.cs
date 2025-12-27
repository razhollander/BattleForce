using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Network;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public class FullTickPacket : INetSerializable
    {
        public int Tick;
        //public SimulationStateS2C PreviousSimulationState; // not sure if gonna need this
        public SimulationStateS2C CurrentSimulationState;
        public FixedUnorderedList<BulletSpawnNetEventS2C> BulletSpawnNetEvents; // todo: remove events related to bullet when bullet id destroyed
        public FixedUnorderedList<PlayerJoinAcceptPacketS2C> PlayerJoinAcceptNetEvents;
        public FixedUnorderedList<PlayerTakeDamageNetEventS2C> PlayerTakeDamageNetEvents;
        public FixedUnorderedList<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents;

        public FullTickPacket(MaxCap maxCap)
        {
            CurrentSimulationState = new SimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, maxCap.ConcurrentEvironmentWalls, maxCap.PointsInEvironmentWall);
            BulletSpawnNetEvents = new FixedUnorderedList<BulletSpawnNetEventS2C>(maxCap.BulletSpawnNetEvents);
            PlayerJoinAcceptNetEvents = new FixedUnorderedList<PlayerJoinAcceptPacketS2C>(maxCap.PlayerJoinAcceptNetEvents);
            
            for (int i = 0; i < PlayerJoinAcceptNetEvents.RawArray.Length; i++)
            {
                PlayerJoinAcceptNetEvents.RawArray[i] = new PlayerJoinAcceptPacketS2C() {SimulationState = new SimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, maxCap.ConcurrentEvironmentWalls, maxCap.PointsInEvironmentWall)};
            }

            PlayerTakeDamageNetEvents = new FixedUnorderedList<PlayerTakeDamageNetEventS2C>(maxCap.PlayerTakeDamageNetEvents);
            BulletDestroyedNetEvents = new FixedUnorderedList<BulletDestroyedNetEventS2C>(maxCap.BulletDestroyedNetEvents);
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
        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            CurrentSimulationState.DeserializeTransforms(reader);
            DeserializedPlayerJoinedEvents(reader);
            DeserializedBulletSpawnedEvents(reader);
            DeserializedPlayerTakeDamageEvents(reader);
            DeserializedBulletDestroyedEvents(reader);
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
                PlayerTakeDamageNetEvents.AddAndGet().Deserialize(reader);
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
                BulletDestroyedNetEvents.AddAndGet().Deserialize(reader);
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
                ref var playerJoinAcceptPacket = ref PlayerJoinAcceptNetEvents.AddAndGet();
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
                BulletSpawnNetEvents.AddAndGet().Deserialize(reader);
            }
        }
    }
}