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
        public ushort TeleportGatePairId;
        public Vector2 EnterPoint;
        public Vector2 DestinationPoint;

        public PlayerToEnvironmentTeleportGateCollisionNetEventS2C(int occuredOnTick, ushort teleportGatePairId, Vector2 enterPoint, Vector2 destinationPoint)
        {
            OccuredOnTick = occuredOnTick;
            TeleportGatePairId = teleportGatePairId;
            EnterPoint = enterPoint;
            DestinationPoint = destinationPoint;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(TeleportGatePairId);
            writer.PutVector2Quantized(EnterPoint);
            writer.PutVector2Quantized(DestinationPoint);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            TeleportGatePairId = reader.GetUShort();
            EnterPoint = reader.GetVector2Quantized();
            DestinationPoint = reader.GetVector2Quantized();
        }
    }
}
