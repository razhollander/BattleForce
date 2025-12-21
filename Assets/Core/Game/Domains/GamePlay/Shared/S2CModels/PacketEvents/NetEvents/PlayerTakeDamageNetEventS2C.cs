using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public class PlayerTakeDamageNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public int PlayerHealth;
        public int HitDamage;
        public bool IsAlive;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
            writer.Put((byte)PlayerHealth);
            writer.Put((byte)HitDamage);
            writer.Put(IsAlive);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            PlayerHealth = reader.GetByte();
            HitDamage = reader.GetByte();
            IsAlive = reader.GetBool();
        }
    }
}