using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    [Serializable]
    public struct MoleStateS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public Vector2 Position;
        public int DisappearOnTick; // server only, zero means this mole never expires on its own

        public MoleStateS2C(ushort id, Vector2 position, int disappearOnTick)
        {
            Id = id;
            Position = position;
            DisappearOnTick = disappearOnTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.PutVector2Quantized(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            Position = reader.GetVector2Quantized();
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
