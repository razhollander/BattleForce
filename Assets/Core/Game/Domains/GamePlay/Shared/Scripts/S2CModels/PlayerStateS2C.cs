using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public class PlayerStateS2C
    {
        public ushort Id;
        public string Name;
        public PlayerSpaceshipStateS2C Spaceship;
        public ushort TeamId;
        public bool IsAlive;

        public PlayerStateS2C(int maxTalents)
        {
            Spaceship = new PlayerSpaceshipStateS2C(maxTalents);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put(Name);
            writer.Put((byte)TeamId);
            Spaceship.Serialize(writer);
        }
        
        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            Name = reader.GetString();
            TeamId = reader.GetByte();
            Spaceship.Deserialize(reader);
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            Spaceship.SerializeDeltas(writer);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            Id = reader.GetByte();
            Spaceship.DeserializeDeltas(reader);
        }
    }
}