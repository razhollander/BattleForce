using System;
using System.Numerics;
using CoreDomain.Scripts.Utils;
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
        public float Radius;
        
        public void UpdateRadiusForTick(int tick, float maxRadius)
        {
            Radius = MathUtils.RemapClamped(CreatedOnTick, EndTick, 0, maxRadius, tick);
        }
        
        public TalentSwapFieldS2C(ushort id, ushort playerCasterId, int createdOnTick, int endTick, float radius)
        {
            Id = id;
            PlayerCasterId = playerCasterId;
            CreatedOnTick = createdOnTick;
            EndTick = endTick;
            Radius = radius;
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