using System.Linq;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct SimulationStateS2C
    {
        public ushort PlayersCount;
        public PlayerStateS2C[] Players;
        public StructPool<PlayerBulletS2C> Bullets;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PlayersCount);
            for (int i = 0; i < PlayersCount; i++)
            {
                Players[i].Serialize(writer);
            }
        
            var bulletsCount = Bullets.UsedCount;
            writer.Put((byte)bulletsCount);
            if (bulletsCount > 0)
            {
                foreach (var bulletIndex in Bullets.UsedIndices())
                {
                    Bullets[bulletIndex].Serialize(writer);
                }
            }
        }
        
        public void Deserialize(NetDataReader reader)
        {
            PlayersCount = reader.GetByte();
            Players = new PlayerStateS2C[PlayersCount];
            for (int i = 0; i < PlayersCount; i++)
                Players[i].Deserialize(reader);
            var bulletsCount = (int)reader.GetByte();
            Bullets = new StructPool<PlayerBulletS2C>(bulletsCount);
            if (bulletsCount > 0)
            {
                for (int i = 0; i < bulletsCount; i++)
                {
                    Bullets.Rent(out int index);
                    Bullets[index].Deserialize(reader);
                }
            }
        }

        public PlayerStateS2C GetPlayer(int playerId)
        {
            return Players.First(x => x.Id == playerId);
        }
        
        public PlayerBulletS2C GetBullet(int bulletId)
        {
            foreach (var index in Bullets.UsedIndices())
            {
                var playerBullet = Bullets[index];
                if (playerBullet.Id == bulletId)
                {
                    return playerBullet;
                }
            }

            LogService.LogError($"No bullet for id {bulletId}!");
            return default;
        }

        public void SerializeTransforms(NetDataWriter writer)
        {
            writer.Put((byte)PlayersCount);
            for (var i = 0; i < PlayersCount; i++)
            {
                Players[i].SerializeDeltas(writer);
            }

            var bulletsCount = Bullets.UsedCount;
            writer.Put((byte)bulletsCount);
            if (bulletsCount > 0)
            {
                foreach (var bulletIndex in Bullets.UsedIndices())
                {
                    Bullets[bulletIndex].SerializeTransforms(writer);
                }
            }
        }

        public void DeserializeTransforms(NetDataReader reader)
        {
            PlayersCount = reader.GetByte();
            Players = new PlayerStateS2C[PlayersCount];
            for (var i = 0; i < PlayersCount; i++)
            {
                Players[i].DeserializeDeltas(reader);
            }

            var bulletsCount = (int)reader.GetByte();
            Bullets = new StructPool<PlayerBulletS2C>(bulletsCount);
            if (bulletsCount > 0)
            {
                for (int i = 0; i < bulletsCount; i++)
                {
                    Bullets.Rent(out int index);
                    Bullets[index].DeserializeTransforms(reader);
                }
            }
        }
    }

    public struct PlayerStateS2C
    {
        public ushort Id;
        public string Name;
        public PlayerSpaceshipStateS2C Spaceship;

        public PlayerStateS2C(ushort id, string name, PlayerSpaceshipStateS2C spaceship)
        {
            Id = id;
            Name = name;
            Spaceship = spaceship;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put(Name);
            Spaceship.Serialize(writer);
        }
        
        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            Name = reader.GetString();
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

    public struct PlayerSpaceshipStateS2C : INetSerializable
    {
        public PlayerTransformStateS2C Transform;
        public PlayerShootStateS2C Shoot;
        public PlayerHealthS2C Health;

        public PlayerSpaceshipStateS2C(PlayerTransformStateS2C transform, float shootCooldown, int health)
        {
            Transform = transform;
            Shoot = new PlayerShootStateS2C(shootCooldown);
            Health = new PlayerHealthS2C(health);
        }

        public void Serialize(NetDataWriter writer)
        {
            Transform.Serialize(writer);
            Shoot.Serialize(writer);
            Health.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            Transform.Deserialize(reader);
            Shoot.Deserialize(reader);
            Health.Deserialize(reader);
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

    public struct PlayerTransformStateS2C : INetSerializable
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Acceleration;
        public Vector2 Direction;
        public float Radius;
        public float AngularVelocity;
        public Vector2 AimVector;

        public Vector2 GetHeadPosition()
        {
            return Position + Direction * Radius;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Position);
            // writer.Put(Velocity);
            // writer.Put(Acceleration);
            writer.Put(Direction);
            writer.Put(Radius);
            // writer.Put(AngularVelocity);
            // writer.Put(AimVector);
        }

        public void Deserialize(NetDataReader reader)
        {
            Position = reader.GetVector2();
            // Velocity = reader.GetVector2();
            // Acceleration = reader.GetVector2();
            Direction = reader.GetVector2();
            Radius = reader.GetFloat();
            // AngularVelocity = reader.GetFloat();
            // AimVector = reader.GetVector2();
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            writer.Put(Position);
            writer.Put(Direction);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            Position = reader.GetVector2();
            Direction = reader.GetVector2();
        }
    }

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

    public struct PlayerHealthS2C : INetSerializable
    {
        public int MaxHealth;
        public int CurrentHealth;

        public PlayerHealthS2C(int maxHealth) : this()
        {
            MaxHealth = maxHealth;
            CurrentHealth = MaxHealth;
        }

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
        
        public void SerializeCurrentHealth(NetDataWriter writer)
        {
            writer.Put((byte)CurrentHealth);
        }

        public void DeserializeCurrentHealth(NetDataReader reader)
        {
            CurrentHealth = reader.GetByte();
        }
    }

    public struct PlayerBulletS2C : INetSerializable
    {
        public int Id;
        public ushort BelongToPlayerId;
        public Vector2 Position;
        public float MoveSpeed;
        public Vector2 Direction;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put((byte)BelongToPlayerId);
            writer.Put(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            BelongToPlayerId = reader.GetByte();
            Position = reader.GetVector2();
        }

        public void SerializeTransforms(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put(Position);
        }

        public void DeserializeTransforms(NetDataReader reader)
        {
            Id = reader.GetByte();
            Position = reader.GetVector2();
        }
    }

    public struct PlayerShootBulletEventS2C
    {
        public byte BulletId;
        public byte PlayerId;
        public Vector2 ShootPosition;
    }

    public struct BulletHitPlayerEventS2C
    {
        public byte BulletId;
        public byte PlayerId;
    }
}