using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct PlayerToEnvironmentTeleportGateCollisionNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public ushort TeleportGatePairId;
        public Vector2 EnterPoint;
        public Vector2 ExitPoint;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
            writer.Put(TeleportGatePairId);
            writer.PutVector2Quantized(EnterPoint);
            writer.PutVector2Quantized(ExitPoint);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            TeleportGatePairId = reader.GetUShort();
            EnterPoint = reader.GetVector2Quantized();
            ExitPoint = reader.GetVector2Quantized();
        }
    }
}
