using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerShootStateS2C : INetSerializable
    {
        public float CooldownSecondsLeft;
        public float MaxCooldown;

        public PlayerShootStateS2C(float maxCooldown)
        {
            MaxCooldown = maxCooldown;
            CooldownSecondsLeft = MaxCooldown;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CooldownSecondsLeft);
            writer.Put(MaxCooldown);
        }

        public void Deserialize(NetDataReader reader)
        {
            CooldownSecondsLeft = reader.GetFloat();
            MaxCooldown = reader.GetFloat();
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            writer.Put(CooldownSecondsLeft);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            CooldownSecondsLeft = reader.GetFloat();
        }
    }
}