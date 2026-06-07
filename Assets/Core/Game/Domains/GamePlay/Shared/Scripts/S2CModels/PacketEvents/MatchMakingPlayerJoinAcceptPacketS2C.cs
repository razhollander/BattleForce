using System;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents
{
    public class MatchMakingPlayerJoinAcceptPacketS2C : INetSerializable, IComparable<MatchMakingPlayerJoinAcceptPacketS2C>
    {
        public int OccuredOnTick;
        public bool IsLocal;
        public FixedClassUnorderedList<MatchMakingPlayerStateS2C> Players;
        public MatchMakingSimulationStateS2C SimulationState;

        public MatchMakingPlayerJoinAcceptPacketS2C(MaxCap maxCap)
        {
            Players = new FixedClassUnorderedList<MatchMakingPlayerStateS2C>(maxCap.ConcurrentPlayers, () => new MatchMakingPlayerStateS2C());
            SimulationState = new MatchMakingSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(IsLocal);
            
            writer.Put((byte)Players.Count);
            foreach (var player in Players.AsSpan())
            {
                player.Serialize(writer);
            }
            
            if (IsLocal)
            {
                SimulationState.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            IsLocal = reader.GetBool();
            
            var playersCount = reader.GetByte();
            Players.Clear();
            for (var i = 0; i < playersCount; i++)
            {
                var player = Players.AddAndGet();
                player.Deserialize(reader);;
            }
            
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