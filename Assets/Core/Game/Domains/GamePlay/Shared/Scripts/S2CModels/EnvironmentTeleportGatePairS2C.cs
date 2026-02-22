using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct EnvironmentTeleportGatePairS2C : INetSerializable, IEquatable<ushort>
    {
        private const ushort GateCount = 2;
        public ushort Id;
        public EnvironmentTeleportGateS2C GateA;
        public EnvironmentTeleportGateS2C GateB;
        public ushort GateAId => (ushort) (Id * GateCount);
        public ushort GateBId => (ushort) (Id * GateCount + 1);

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(GateA);
            writer.Put(GateB);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            GateA.Deserialize(reader);
            GateB.Deserialize(reader);
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }

    public struct EnvironmentTeleportGateS2C : INetSerializable
    {
        public Vector2 Position;
        public float NormalRotation;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.PutVector2Quantized(Position);
            writer.Put(NormalRotation);
        }

        public void Deserialize(NetDataReader reader)
        {
            Position = reader.GetVector2Quantized();
            NormalRotation = reader.GetFloat();
        }
    }
}
