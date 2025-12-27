using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerHealthS2C : INetSerializable
    {
        public ushort MaxHealth;
        public ushort CurrentHealth;

        // public PlayerHealthS2C(ushort maxHealth) : this()
        // {
        //     MaxHealth = maxHealth;
        //     CurrentHealth = MaxHealth;
        // }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MaxHealth);
            writer.Put((byte)CurrentHealth);
        }

        public void Deserialize(NetDataReader reader)
        {
            MaxHealth = reader.GetByte();
            CurrentHealth = reader.GetByte();
        }
        
        // public void SerializeCurrentHealth(NetDataWriter writer)
        // {
        //     writer.Put((byte)CurrentHealth);
        // }
        //
        // public void DeserializeCurrentHealth(NetDataReader reader)
        // {
        //     CurrentHealth = reader.GetByte();
        // }
    }
}