using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Network;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class PlayerRejoinAcceptPacketS2C : INetSerializable, IComparable<PlayerRejoinAcceptPacketS2C>
    {
        public int OccuredOnTick;
        public bool IsLocal;
        public List<PlayerStateS2C> PlayerStates;
        public MatchSimulationStateS2C SimulationState;

        public PlayerRejoinAcceptPacketS2C(MaxCap maxCap, int maxTalentsPerPlayer, int maxTeams)
        {
            PlayerStates = new List<PlayerStateS2C>(maxCap.ConcurrentPlayers);
            SimulationState = new MatchSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, maxTalentsPerPlayer, maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUpBalls, maxTeams, maxCap.ConcurrentChickenEggs);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(IsLocal);

            writer.Put((byte)PlayerStates.Count);
            foreach(var s in PlayerStates)
            {
                s.Serialize(writer);
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

            var count = reader.GetByte();
            PlayerStates.Clear();
            for(int i = 0; i < count; i++)
            {
                var state = new PlayerStateS2C(3, 10); // Using typical default values since MaxCap might not be available here, alternatively a wrapper can be used
                state.Deserialize(reader);
                PlayerStates.Add(state);
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
            PlayerStates.Clear();
        }
    }
}
