using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct PerformGalacticPullNetEventS2C : INetSerializable, IComparable<PerformGalacticPullNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort FieldId;
        public ushort CasterPlayerId;
        public ushort CasterTeamId;
        public int EndTick;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(FieldId);
            writer.Put((byte)CasterPlayerId);
            writer.Put((byte)CasterTeamId);
            writer.Put(EndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            FieldId = reader.GetUShort();
            CasterPlayerId = reader.GetByte();
            CasterTeamId = reader.GetByte();
            EndTick = reader.GetInt();
        }

        public int CompareTo(PerformGalacticPullNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
