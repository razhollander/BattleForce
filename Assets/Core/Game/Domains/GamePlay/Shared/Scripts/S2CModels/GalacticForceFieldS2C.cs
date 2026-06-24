using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct GalacticForceFieldS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public ushort CasterPlayerId;
        public ushort CasterTeamId;
        public int EndTick;

        public GalacticForceFieldS2C(ushort id, ushort casterPlayerId, ushort casterTeamId, int endTick)
        {
            Id = id;
            CasterPlayerId = casterPlayerId;
            CasterTeamId = casterTeamId;
            EndTick = endTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put((byte)CasterPlayerId);
            writer.Put((byte)CasterTeamId);
            writer.Put(EndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            CasterPlayerId = reader.GetByte();
            CasterTeamId = reader.GetByte();
            EndTick = reader.GetInt();
        }

        public bool Equals(ushort otherId) => Id == otherId;
    }
}
