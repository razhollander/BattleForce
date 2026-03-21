using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct TalentSwapFieldS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public ushort PlayerCasterId;
        public int CreatedOnTick;
        public int EndTick;
        
        public TalentSwapFieldS2C(ushort id, ushort playerCasterId, int createdOnTick, int endTick)
        {
            Id = id;
            PlayerCasterId = playerCasterId;
            CreatedOnTick = createdOnTick;
            EndTick = endTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(PlayerCasterId);
            writer.Put(CreatedOnTick);
            writer.Put(EndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            PlayerCasterId = reader.GetUShort();
            CreatedOnTick = reader.GetInt();
            EndTick = reader.GetInt();
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}