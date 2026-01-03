using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerSpaceshipStateS2C : INetSerializable
    {
        public Color Color;
        public PlayerTransformStateS2C Transform;
        public PlayerShootStateS2C Shoot;
        public PlayerHealthS2C Health;
        public PlayerTalentsStateS2C Talents;
        // public PlayerSpaceshipStateS2C(PlayerTransformStateS2C transform, float shootCooldown, ushort health)
        // {
        //     Transform = transform;
        //     Shoot = new PlayerShootStateS2C(shootCooldown);
        //     Health = new PlayerHealthS2C(health);
        // }

        public void Serialize(NetDataWriter writer)
        {
            Transform.Serialize(writer);
            Shoot.Serialize(writer);
            Health.Serialize(writer);
            writer.Put(Color);
        }

        public void Deserialize(NetDataReader reader)
        {
            Transform.Deserialize(reader);
            Shoot.Deserialize(reader);
            Health.Deserialize(reader);
            Color = reader.GetColor();
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            Transform.SerializeDeltas(writer);
            Shoot.SerializeDeltas(writer);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            Transform.DeserializeDeltas(reader);
            Shoot.DeserializeDeltas(reader);
        }
    }
}