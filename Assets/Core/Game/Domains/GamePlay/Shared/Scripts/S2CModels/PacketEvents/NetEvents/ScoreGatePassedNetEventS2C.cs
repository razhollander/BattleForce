using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct ScoreGatePassedNetEventS2C : INetSerializable, IComparable<ScoreGatePassedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort ScoreGateId;
        public ushort ByPlayerId;
        public ushort ByTeamId;
        public byte ScoreGained;
        public int TeamBonusScoreTotal;
        public int ByPlayerBonusScoreTotal;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)ScoreGateId);
            writer.Put((byte)ByPlayerId);
            writer.Put((byte)ByTeamId);
            writer.Put(ScoreGained);
            writer.Put(TeamBonusScoreTotal);
            writer.Put(ByPlayerBonusScoreTotal);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            ScoreGateId = reader.GetByte();
            ByPlayerId = reader.GetByte();
            ByTeamId = reader.GetByte();
            ScoreGained = reader.GetByte();
            TeamBonusScoreTotal = reader.GetInt();
            ByPlayerBonusScoreTotal = reader.GetInt();
        }

        public int CompareTo(ScoreGatePassedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
