using System;
using System.Numerics;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public class PlayerStateS2C : IEquatable<ushort>
    {
        public ushort Id;
        public string Name;
        public PlayerSpaceshipStateS2C Spaceship;
        public ushort TeamId;
        public int MolesHitScore; // this player's contribution to his team's WhacAMole score, reset each stage

        public PlayerStateS2C(int maxTalents, int maxEnemiesAmount)
        {
            Spaceship = new PlayerSpaceshipStateS2C(maxTalents, maxEnemiesAmount);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put(Name);
            writer.Put((byte)TeamId);
            writer.Put(MolesHitScore);
            Spaceship.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            Name = reader.GetString();
            TeamId = reader.GetByte();
            MolesHitScore = reader.GetInt();
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

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}