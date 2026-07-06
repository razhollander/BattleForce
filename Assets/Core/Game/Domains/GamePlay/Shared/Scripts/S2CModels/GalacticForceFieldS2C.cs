using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct GalacticForceFieldS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public ushort CasterTeamId;
        public int EndTick;

        public GalacticForceFieldS2C(ushort id, ushort casterTeamId, int endTick)
        {
            Id = id;
            CasterTeamId = casterTeamId;
            EndTick = endTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put((byte)CasterTeamId);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            CasterTeamId = reader.GetByte();
        }

        public bool Equals(ushort otherId) => Id == otherId;
    }
}
