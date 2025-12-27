using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerStateS2C
    {
        public ushort Id;
        public string Name;
        public PlayerSpaceshipStateS2C Spaceship;
        public ushort TeamId;
        public bool IsAlive;
        //
        // public PlayerStateS2C(ushort id, string name, PlayerSpaceshipStateS2C spaceship)
        // {
        //     Id = id;
        //     Name = name;
        //     Spaceship = spaceship;
        //     TeamId = id;
        //     IsAlive = true;
        // }

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