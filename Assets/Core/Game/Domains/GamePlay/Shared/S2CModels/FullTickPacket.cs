using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using CoreDomain.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct FullTickPacket : INetSerializable
    {
        public int Tick;
        //public SimulationStateS2C PreviousSimulationState; // not sure if gonna need this
        public SimulationStateS2C CurrentSimulationState;
        public List<BulletSpawnNetEventS2C> BulletSpawnNetEvents; // todo: remove events related to bullet when bullet id destroyed
        public List<PlayerJoinAcceptPacketS2C> PlayerJoinAcceptNetEvents;
        public List<PlayerTakeDamageNetEventS2C> PlayerTakeDamageNetEvents;
        public List<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents;

        public FullTickPacket(int tick, SimulationStateS2C previousSimulationState,
            SimulationStateS2C currentSimulationState, List<BulletSpawnNetEventS2C> bulletSpawnNetEvents,
            List<PlayerJoinAcceptPacketS2C> playerJoinAcceptNetEvents, List<PlayerTakeDamageNetEventS2C> playerTakeDamageNetEvents,
            List<BulletDestroyedNetEventS2C> bulletDestroyedNetEvents)
        {
            Tick = tick;
            //PreviousSimulationState = previousSimulationState;
            CurrentSimulationState = currentSimulationState;
            BulletSpawnNetEvents = bulletSpawnNetEvents;
            PlayerJoinAcceptNetEvents = playerJoinAcceptNetEvents;
            PlayerTakeDamageNetEvents = playerTakeDamageNetEvents;
            BulletDestroyedNetEvents = bulletDestroyedNetEvents;
        }

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
            if (PlayerTakeDamageNetEvents.IsNullOrEmpty())
            {
                writer.Put((byte)0);
            }
            else
            {
                writer.Put((byte)PlayerTakeDamageNetEvents.Count);
                foreach (var playerTakeDamageEvent in PlayerTakeDamageNetEvents)
                {
                    playerTakeDamageEvent.Serialize(writer);
                }
            }
        }

        private void DeserializedPlayerTakeDamageEvents(NetDataReader reader)
        {
            var playerTakeDamageEventsCount = reader.GetByte();
            if (playerTakeDamageEventsCount > 0)
            {
                var array = new PlayerTakeDamageNetEventS2C[playerTakeDamageEventsCount];
                for (var i = 0; i < playerTakeDamageEventsCount; i++)
                {
                    array[i].Deserialize(reader);
                }
                
                PlayerTakeDamageNetEvents = array.ToList();
            }
        }

        private void SerializedBulletDestroyedEvents(NetDataWriter writer)
        {
            if (BulletDestroyedNetEvents.IsNullOrEmpty())
            {
                writer.Put((byte)0);
            }
            else
            {
                writer.Put((byte)BulletDestroyedNetEvents.Count);
                foreach (var bulletDestroyedEvent in BulletDestroyedNetEvents)
                {
                    bulletDestroyedEvent.Serialize(writer);
                }
            }
        }

        private void DeserializedBulletDestroyedEvents(NetDataReader reader)
        {
            var bulletDestroyedEventsCount = reader.GetByte();
            if (bulletDestroyedEventsCount > 0)
            {
                var array = new BulletDestroyedNetEventS2C[bulletDestroyedEventsCount];
                for (var i = 0; i < bulletDestroyedEventsCount; i++)
                {
                    array[i].Deserialize(reader);
                }
                
                BulletDestroyedNetEvents = array.ToList();
            }
        }
        
        private void SerializedPlayerJoinedEvents(NetDataWriter writer)
        {
            if (PlayerJoinAcceptNetEvents.IsNullOrEmpty())
            {
                writer.Put((byte)0);
            }
            else
            {
                writer.Put((byte)PlayerJoinAcceptNetEvents.Count);
                foreach (var playerJoinAcceptNetEvent in PlayerJoinAcceptNetEvents)
                {
                    playerJoinAcceptNetEvent.Serialize(writer);
                }
            }
        }

        private void DeserializedPlayerJoinedEvents(NetDataReader reader)
        {
            var playerJoinedNetEventsCount = reader.GetByte();
            if (playerJoinedNetEventsCount > 0)
            {
                var array = new PlayerJoinAcceptPacketS2C[playerJoinedNetEventsCount];
                for (var i = 0; i < playerJoinedNetEventsCount; i++)
                {
                    array[i].Deserialize(reader);
                }

                PlayerJoinAcceptNetEvents = array.ToList();
            }
        }
        
        private void SerializedBulletSpawnedEvents(NetDataWriter writer)
        {
            if (BulletSpawnNetEvents.IsNullOrEmpty())
            {
                writer.Put((byte)0);
            }
            else
            {
                var bulletSpawnedAmount = BulletSpawnNetEvents.Count;
                if (bulletSpawnedAmount > 255)
                {
                    LogService.LogError($"Too many bullet were spawned! Amount {bulletSpawnedAmount}");
                }
            
                writer.Put((byte)bulletSpawnedAmount);
                foreach (var bulletSpawnEvent in BulletSpawnNetEvents)
                {
                    bulletSpawnEvent.Serialize(writer);
                }
            }
        }

        private void DeserializedBulletSpawnedEvents(NetDataReader reader)
        {
            var bulletSpawnNetEventsCount = reader.GetByte();
            if (bulletSpawnNetEventsCount > 0)
            {
                var bulletSpawnNetEventsArray = new BulletSpawnNetEventS2C[bulletSpawnNetEventsCount];
                for (int i = 0; i < bulletSpawnNetEventsCount; i++)
                {
                    bulletSpawnNetEventsArray[i].Deserialize(reader);
                }

                BulletSpawnNetEvents = bulletSpawnNetEventsArray.ToList();
            }
        }
    }
}