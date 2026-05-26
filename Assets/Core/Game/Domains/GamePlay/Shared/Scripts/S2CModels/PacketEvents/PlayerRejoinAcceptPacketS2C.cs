using System;
using Core.Scripts.Network;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents
{
    public class PlayerRejoinAcceptPacketS2C : INetSerializable, IComparable<PlayerRejoinAcceptPacketS2C>
    {
        public int OccuredOnTick;
        public bool IsLocal;
        public PlayerStateS2C PlayerState;
        public MatchSimulationStateS2C SimulationState;

        public PlayerRejoinAcceptPacketS2C(MaxCap maxCap, int maxTalentsPerPlayer, int maxTeams)
        {
            PlayerState = new PlayerStateS2C(maxTalentsPerPlayer, maxCap.ConcurrentPlayers-1);
            SimulationState = new MatchSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, maxTalentsPerPlayer, maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUpBalls, maxTeams, maxCap.ConcurrentChickenEggs);
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

        public int CompareTo(PlayerRejoinAcceptPacketS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}