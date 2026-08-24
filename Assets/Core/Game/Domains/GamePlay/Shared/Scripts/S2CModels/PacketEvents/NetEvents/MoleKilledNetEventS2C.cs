using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct MoleKilledNetEventS2C : INetSerializable, IComparable<MoleKilledNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort MoleId;
        public ushort MoleHoleId;
        public ushort ByPlayerId;
        public byte ScoreGained;
        public ushort TeamMolesKilledTotal;
        public ushort ByPlayerMolesKilledScoreTotal;
        public bool IsGolden;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)MoleId);
            writer.Put((byte)MoleHoleId);
            writer.Put((byte)ByPlayerId);
            writer.Put(ScoreGained);
            writer.Put(TeamMolesKilledTotal);
            writer.Put(ByPlayerMolesKilledScoreTotal);
            writer.Put(IsGolden);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            MoleId = reader.GetByte();
            MoleHoleId = reader.GetByte();
            ByPlayerId = reader.GetByte();
            ScoreGained = reader.GetByte();
            TeamMolesKilledTotal = reader.GetUShort();
            ByPlayerMolesKilledScoreTotal = reader.GetUShort();
            IsGolden = reader.GetBool();
        }

        public int CompareTo(MoleKilledNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
