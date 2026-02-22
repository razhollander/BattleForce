using System;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Scripts.Network;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents
{
    public class MatchMakingPlayerJoinAcceptPacketS2C : INetSerializable, IComparable<MatchMakingPlayerJoinAcceptPacketS2C>
    {
        public int OccuredOnTick;
        public bool IsLocal;
        public MatchMakingPlayerStateS2C PlayerState;
        public MatchMakingSimulationStateS2C SimulationState;

        public MatchMakingPlayerJoinAcceptPacketS2C(MaxCap maxCap)
        {
            PlayerState = new MatchMakingPlayerStateS2C();
            SimulationState = new MatchMakingSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(IsLocal);
            PlayerState.Serialize(writer);

            if (IsLocal)
            {
                SimulationState.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            IsLocal = reader.GetBool();
            PlayerState.Deserialize(reader);
            if (IsLocal)
            {
                SimulationState.Deserialize(reader);
            }
        }

        public int CompareTo(MatchMakingPlayerJoinAcceptPacketS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}