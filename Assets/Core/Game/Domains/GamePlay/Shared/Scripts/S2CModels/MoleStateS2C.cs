using System;
using System.Numerics;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    [Serializable]
    public struct MoleStateS2C : INetSerializable, IEquatable<ushort>
    {
        public const int NEVER_EXPIRES_TICK = 0;
        public const int NOT_HIDING_TICK = 0;

        public ushort Id;
        public ushort MoleHoleId;
        public Vector2 Position; // server only, the hole's position scaled by the map size multiplier
        public int EmergeOnTick; // until this tick the mole is still hidden in its shaking hole, so it cannot be targeted or hit
        public bool IsEmerged; // server only, tells whether the mole already got its physics body
        public int DisappearOnTick; // server only, zero means this mole never expires on its own
        public bool HasLifetimeEnd => DisappearOnTick != NEVER_EXPIRES_TICK;
        public bool IsShakingBeforeHiding => HideOnTick != NOT_HIDING_TICK;
        public int HideOnTick; // once its lifetime ends the mole shakes in place until this tick and then goes back into its hole, zero means it is not expiring yet
        public bool IsGolden;
        public byte RemainingLives; // a normal mole has a single life, a golden mole starts with MaxLives
        public byte MaxLives;

        public MoleStateS2C(ushort id, ushort moleHoleId, Vector2 position, int emergeOnTick, int disappearOnTick, bool isGolden, byte lives)
        {
            Id = id;
            MoleHoleId = moleHoleId;
            Position = position;
            EmergeOnTick = emergeOnTick;
            IsEmerged = false;
            DisappearOnTick = disappearOnTick;
            HideOnTick = NOT_HIDING_TICK;
            IsGolden = isGolden;
            RemainingLives = lives;
            MaxLives = lives;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put((byte)MoleHoleId);
            writer.Put(EmergeOnTick);
            writer.Put(HideOnTick);
            writer.Put(IsGolden);
            writer.Put(RemainingLives);
            writer.Put(MaxLives);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            MoleHoleId = reader.GetByte();
            EmergeOnTick = reader.GetInt();
            HideOnTick = reader.GetInt();
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
