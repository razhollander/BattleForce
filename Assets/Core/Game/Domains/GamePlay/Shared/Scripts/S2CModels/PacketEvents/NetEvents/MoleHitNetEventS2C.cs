using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct MoleHitNetEventS2C : INetSerializable, IComparable<MoleHitNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort MoleId;
        public ushort MoleHoleId;
        public ushort ByPlayerId;
        public ushort ByTeamId;
        public byte ScoreGained;
        public int TeamMolesHitTotal;
        public int ByPlayerMolesHitScoreTotal;
        public bool IsGolden;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)MoleId);
            writer.Put((byte)MoleHoleId);
            writer.Put((byte)ByPlayerId);
            writer.Put((byte)ByTeamId);
            writer.Put(ScoreGained);
            writer.Put(TeamMolesHitTotal);
            writer.Put(ByPlayerMolesHitScoreTotal);
            writer.Put(IsGolden);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            MoleId = reader.GetByte();
            MoleHoleId = reader.GetByte();
            ByPlayerId = reader.GetByte();
            ByTeamId = reader.GetByte();
            ScoreGained = reader.GetByte();
            TeamMolesHitTotal = reader.GetInt();
            ByPlayerMolesHitScoreTotal = reader.GetInt();
            IsGolden = reader.GetBool();
        }

        public int CompareTo(MoleHitNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
