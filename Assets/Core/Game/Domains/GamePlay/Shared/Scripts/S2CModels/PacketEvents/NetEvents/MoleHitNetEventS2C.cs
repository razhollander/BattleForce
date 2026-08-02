using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct MoleHitNetEventS2C : INetSerializable, IComparable<MoleHitNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort MoleId;
        public ushort ByPlayerId;
        public ushort ByTeamId;
        public int TeamMolesHitTotal;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)MoleId);
            writer.Put((byte)ByPlayerId);
            writer.Put((byte)ByTeamId);
            writer.Put(TeamMolesHitTotal);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            MoleId = reader.GetByte();
            ByPlayerId = reader.GetByte();
            ByTeamId = reader.GetByte();
            TeamMolesHitTotal = reader.GetInt();
        }

        public int CompareTo(MoleHitNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
