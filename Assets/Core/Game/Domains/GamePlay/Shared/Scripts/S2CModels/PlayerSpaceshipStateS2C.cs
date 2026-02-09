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
        public PlayerTalentsStateS2C TalentsState;
        public bool IsEngineOn = true;
        public bool IsAlive = true;

        public PlayerSpaceshipStateS2C(int maxTalents)
        {
            TalentsState = new PlayerTalentsStateS2C(maxTalents);
        }

        public void Serialize(NetDataWriter writer)
        {
            Transform.Serialize(writer);
            Shoot.Serialize(writer);
            Health.Serialize(writer);
            TalentsState.Serialize(writer);
            writer.Put(IsEngineOn);
            writer.Put(IsAlive);
        }

        public void Deserialize(NetDataReader reader)
        {
            Transform.Deserialize(reader);
            Shoot.Deserialize(reader);
            Health.Deserialize(reader);
            TalentsState.Deserialize(reader);
            IsEngineOn = reader.GetBool();
            IsAlive = reader.GetBool();
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            Transform.SerializeDeltas(writer);
            Shoot.SerializeDeltas(writer);
            TalentsState.SerializeDeltas(writer);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            Transform.DeserializeDeltas(reader);
            Shoot.DeserializeDeltas(reader);
            TalentsState.DeserializeDeltas(reader);
        }
    }
}