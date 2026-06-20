using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents
{
    public class PlayerRejoinAcceptPacketS2C : INetSerializable, IComparable<PlayerRejoinAcceptPacketS2C>
    {
        public int OccuredOnTick;
        public bool IsLocal;
        public FixedClassUnorderedList<PlayerStateS2C> Players;
        public MatchSimulationStateS2C SimulationState;

        public PlayerRejoinAcceptPacketS2C(MaxCap maxCap, int maxTalentsPerPlayer, int maxTeams)
        {
            Players = new FixedClassUnorderedList<PlayerStateS2C>(maxCap.ConcurrentPlayers, ()=>new PlayerStateS2C(maxTalentsPerPlayer, maxCap.ConcurrentLockOnTargets));
            SimulationState = new MatchSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, maxTalentsPerPlayer, maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUpBalls, maxTeams, maxCap.ConcurrentChickenEggs);
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

        public int CompareTo(PlayerRejoinAcceptPacketS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }

        public void Clear()
        {
            OccuredOnTick = 0;
            IsLocal = false;
            Players.Clear();
        }
    }
}
