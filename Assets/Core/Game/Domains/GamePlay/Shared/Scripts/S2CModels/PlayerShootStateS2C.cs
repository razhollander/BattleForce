using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerShootStateS2C : INetSerializable
    {
        public float CooldownSecondsLeft;
        public float MaxCooldown;

        // public PlayerShootStateS2C(float maxCooldown)
        // {
        //     MaxCooldown = maxCooldown;
        //     CooldownSecondsLeft = MaxCooldown;
        // }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutFloat16(CooldownSecondsLeft);
            writer.PutFloat16(MaxCooldown);
        }

        public void Deserialize(NetDataReader reader)
        {
            CooldownSecondsLeft = reader.GetFloat16();
            MaxCooldown = reader.GetFloat16();
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            writer.PutFloat16(CooldownSecondsLeft);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            CooldownSecondsLeft = reader.GetFloat16();
        }
    }
}