using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
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

        public FullTickPacket(int tick, SimulationStateS2C previousSimulationState,
            SimulationStateS2C currentSimulationState, List<BulletSpawnNetEventS2C> bulletSpawnNetEvents,
            List<PlayerJoinAcceptPacketS2C> playerJoinAcceptNetEvents)
        {
            Tick = tick;
            //PreviousSimulationState = previousSimulationState;
            CurrentSimulationState = currentSimulationState;
            BulletSpawnNetEvents = bulletSpawnNetEvents;
            PlayerJoinAcceptNetEvents = playerJoinAcceptNetEvents;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            CurrentSimulationState.SerializeTransforms(writer);
            SerializedPlayerJoinedEvents(writer);
            SerializedBulletSpawnedEvents(writer);
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

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            CurrentSimulationState.DeserializeTransforms(reader);
            DeserializedPlayerJoinedEvents(reader);
            DeserializedBulletSpawnedEvents(reader);
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