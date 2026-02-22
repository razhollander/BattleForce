using System;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct EnvironmentTeleportGatePairS2C : INetSerializable, IEquatable<ushort>

    {
        public ushort Id;
        public Vector2 GateAPosition;
        public float GateARotation;
        public Vector2 GateBPosition;
        public float GateBRotation;
        public Color Color; // x=r, y=g, z=b
        public Vector2 Size; // x=width, y=height
        public ushort GateAId => (ushort) (Id * 2);
        public ushort GateBId => (ushort) (Id * 2 + 1);
        public EnvironmentTeleportGatePairS2C(ushort id, Vector2 gateAPosition, float gateARotation, Vector2 gateBPosition, float gateBRotation, Color color, Vector2 size)
        {
            Id = id;
            GateAPosition = gateAPosition;
            GateARotation = gateARotation;
            GateBPosition = gateBPosition;
            GateBRotation = gateBRotation;
            Color = color;
            Size = size;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(GateAPosition);
            writer.Put(GateARotation);
            writer.PutVector2Quantized(GateBPosition);
            writer.Put(GateBRotation);
            writer.PutVector2Quantized(Size);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            GateAPosition = reader.GetVector2Quantized();
            GateARotation = reader.GetFloat();
            GateBPosition = reader.GetVector2Quantized();
            GateBRotation = reader.GetFloat();
            Size = reader.GetVector2Quantized();
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
