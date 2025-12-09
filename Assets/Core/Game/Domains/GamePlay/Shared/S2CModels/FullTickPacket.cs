using System.Collections.Generic;
using System.Linq;
using CoreDomain.Scripts.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct FullTickPacket : INetSerializable
    {
        public int Tick;
        public SimulationStateS2C PreviousSimulationState;
        public SimulationStateS2C CurrentSimulationState;
        public List<BulletSpawnNetEventS2C> BulletSpawnNetEvents; // todo: remove events related to bullet when bullet id destroyed

        public FullTickPacket(int tick, SimulationStateS2C previousSimulationState, SimulationStateS2C currentSimulationState, List<BulletSpawnNetEventS2C> bulletSpawnNetEvents)
        {
            Tick = tick;
            PreviousSimulationState = previousSimulationState;
            CurrentSimulationState = currentSimulationState;
            BulletSpawnNetEvents = bulletSpawnNetEvents;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            CurrentSimulationState.Serialize(PreviousSimulationState, writer);
            if (!BulletSpawnNetEvents.IsNullOrEmpty())
            {
                writer.Put((ushort)BulletSpawnNetEvents.Count);
                foreach (var bulletSpawnEvent in BulletSpawnNetEvents)
                {
                    bulletSpawnEvent.Serialize(writer);
                }
            }
            else
            {
                writer.Put((ushort)0);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();

            var bulletSpawnNetEventsCount = reader.GetUShort();
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