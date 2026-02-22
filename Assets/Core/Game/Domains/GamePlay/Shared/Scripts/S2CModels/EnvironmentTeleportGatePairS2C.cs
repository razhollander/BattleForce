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
        public float GateANormalRotation;
        public Vector2 GateBPosition;
        public float GateBNormalRotation;
        public Color Color; // x=r, y=g, z=b
        public Vector2 Size; // x=width, y=height
        public ushort GateAId => (ushort) (Id * 2);
        public ushort GateBId => (ushort) (Id * 2 + 1);
        public EnvironmentTeleportGatePairS2C(ushort id, Vector2 gateAPosition, float gateANormalRotation, Vector2 gateBPosition, float gateBNormalRotation, Color color, Vector2 size)
        {
            Id = id;
            GateAPosition = gateAPosition;
            GateANormalRotation = gateANormalRotation;
            GateBPosition = gateBPosition;
            GateBNormalRotation = gateBNormalRotation;
            Color = color;
            Size = size;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(GateAPosition);
            writer.Put(GateANormalRotation);
            writer.PutVector2Quantized(GateBPosition);
            writer.Put(GateBNormalRotation);
            writer.PutVector2Quantized(Size);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            GateAPosition = reader.GetVector2Quantized();
            GateANormalRotation = reader.GetFloat();
            GateBPosition = reader.GetVector2Quantized();
            GateBNormalRotation = reader.GetFloat();
            Size = reader.GetVector2Quantized();
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
