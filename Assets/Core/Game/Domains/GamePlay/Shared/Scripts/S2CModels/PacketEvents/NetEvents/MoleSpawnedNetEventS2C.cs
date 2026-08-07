using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    [Serializable]
    public struct MoleSpawnedNetEventS2C : INetSerializable, IComparable<MoleSpawnedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort MoleId;
        public Vector2 Position;
        public bool IsGolden;
        public byte MaxLives;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)MoleId);
            writer.PutVector2Quantized(Position);
            writer.Put(IsGolden);
            writer.Put(MaxLives);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            MoleId = reader.GetByte();
            Position = reader.GetVector2Quantized();
            IsGolden = reader.GetBool();
            MaxLives = reader.GetByte();
        }

        public int CompareTo(MoleSpawnedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
