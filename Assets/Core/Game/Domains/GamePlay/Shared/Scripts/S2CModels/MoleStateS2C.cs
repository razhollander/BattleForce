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
        public int EmergeOnTick; // until this tick the mole is still hidden in its shaking hole, so it cannot be targeted or hit
        public bool IsEmerged; // server only, tells whether the mole already got its physics body
        public int DisappearOnTick; // server only, zero means this mole never expires on its own
        public bool IsGolden;
        public byte RemainingLives; // a normal mole has a single life, a golden mole starts with MaxLives
        public byte MaxLives;

        public MoleStateS2C(ushort id, Vector2 position, int emergeOnTick, int disappearOnTick, bool isGolden, byte lives)
        {
            Id = id;
            Position = position;
            EmergeOnTick = emergeOnTick;
            IsEmerged = false;
            DisappearOnTick = disappearOnTick;
            IsGolden = isGolden;
            RemainingLives = lives;
            MaxLives = lives;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.PutVector2Quantized(Position);
            writer.Put(EmergeOnTick);
            writer.Put(IsGolden);
            writer.Put(RemainingLives);
            writer.Put(MaxLives);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            Position = reader.GetVector2Quantized();
            EmergeOnTick = reader.GetInt();
            IsGolden = reader.GetBool();
            RemainingLives = reader.GetByte();
            MaxLives = reader.GetByte();
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
