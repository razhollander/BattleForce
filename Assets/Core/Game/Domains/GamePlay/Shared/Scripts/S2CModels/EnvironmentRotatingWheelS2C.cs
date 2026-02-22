using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct EnvironmentRotatingWheelS2C : INetSerializable
    {
        public ushort Id;
        public Vector2 CenterPosition;
        public float RotationSpeed;

        public EnvironmentRotatingWheelS2C(ushort id, Vector2 centerPosition, float rotationSpeed)
        {
            Id = id;
            CenterPosition = centerPosition;
            RotationSpeed = rotationSpeed;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2Quantized(CenterPosition);
            writer.Put(RotationSpeed);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            CenterPosition = reader.GetVector2Quantized();
            RotationSpeed = reader.GetFloat();
        }
    }
}
