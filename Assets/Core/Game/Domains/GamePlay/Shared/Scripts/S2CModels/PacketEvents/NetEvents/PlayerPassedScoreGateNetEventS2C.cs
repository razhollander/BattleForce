using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct PlayerPassedScoreGateNetEventS2C : INetSerializable, IComparable<PlayerPassedScoreGateNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort ScoreGateId;
        public ushort ByPlayerId;
        public byte ScoreGained;
        public ushort NextScoreMultiplier;
        public ushort TeamBonusScoreTotal;
        public ushort ByPlayerBonusScoreTotal;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)ScoreGateId);
            writer.Put((byte)ByPlayerId);
            writer.Put(ScoreGained);
            writer.Put((byte)NextScoreMultiplier);
            writer.Put(TeamBonusScoreTotal);
            writer.Put(ByPlayerBonusScoreTotal);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            ScoreGateId = reader.GetByte();
            ByPlayerId = reader.GetByte();
            ScoreGained = reader.GetByte();
            NextScoreMultiplier = reader.GetByte();
            TeamBonusScoreTotal = reader.GetUShort();
            ByPlayerBonusScoreTotal = reader.GetUShort();
        }

        public int CompareTo(PlayerPassedScoreGateNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
