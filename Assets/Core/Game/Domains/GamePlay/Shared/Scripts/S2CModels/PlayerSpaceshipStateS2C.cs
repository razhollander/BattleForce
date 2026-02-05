using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public class PlayerSpaceshipStateS2C : INetSerializable
    {
        public PlayerTransformStateS2C Transform;
        public PlayerShootStateS2C Shoot;
        public PlayerHealthS2C Health;
        public PlayerTalentsStateS2C Talents;
        public bool IsEngineOn = true;
        public bool IsAlive = true;

        public PlayerSpaceshipStateS2C(int maxTalents)
        {
            Talents = new PlayerTalentsStateS2C(maxTalents);
        }

        public void Serialize(NetDataWriter writer)
        {
            Transform.Serialize(writer);
            Shoot.Serialize(writer);
            Health.Serialize(writer);
            Talents.Serialize(writer);
            writer.Put(IsEngineOn);
            writer.Put(IsAlive);
        }

        public void Deserialize(NetDataReader reader)
        {
            Transform.Deserialize(reader);
            Shoot.Deserialize(reader);
            Health.Deserialize(reader);
            Talents.Deserialize(reader);
            IsEngineOn = reader.GetBool();
            IsAlive = reader.GetBool();
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