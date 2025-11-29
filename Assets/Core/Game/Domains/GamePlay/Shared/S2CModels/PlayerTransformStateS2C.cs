using System.Linq;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.ServerToClientModels
{
    public struct SimulationStateS2C : INetSerializable
    {
        public int Tick;
        public int PlayersCount;
        public PlayerStateS2C[] Players;
        public int BulletsCount;
        public PlayerBulletS2C[] Bullets;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            writer.Put((byte)PlayersCount);
            for (int i = 0; i < PlayersCount; i++)
            {
                Players[i].Serialize(writer);
            }
            writer.Put((byte)BulletsCount);
            for (int i = 0; i < BulletsCount; i++)
            {
                Bullets[i].Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            PlayersCount = reader.GetByte();
            Players = new PlayerStateS2C[PlayersCount];
            for (int i = 0; i < PlayersCount; i++)
                Players[i].Deserialize(reader);
            BulletsCount = reader.GetByte();
            Bullets = new PlayerBulletS2C[BulletsCount];
            for (int i = 0; i < BulletsCount; i++)
            {
                Bullets[i].Deserialize(reader);
            }
        }

        public PlayerStateS2C GetPlayer(int playerId)
        {
            return Players.First(x => x.Id == playerId);
        }
    }

    public struct PlayerStateS2C : INetSerializable
    {
        public int Id;
        public string Name;
        public PlayerSpaceshipStateS2C Spaceship;

        public PlayerStateS2C(int id, string name, PlayerSpaceshipStateS2C spaceship)
        {
            Id = id;
            Name = name;
            Spaceship = spaceship;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            Spaceship.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            Spaceship.Deserialize(reader);
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
    }

    public struct PlayerTransformStateS2C : INetSerializable
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Acceleration;
        public Vector2 RotationVector;
        public float AngularVelocity;
        public Vector2 AimVector;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Position);
            writer.Put(Velocity);
            writer.Put(Acceleration);
            writer.Put(RotationVector);
            writer.Put(AngularVelocity);
            writer.Put(AimVector);
        }

        public void Deserialize(NetDataReader reader)
        {
            Position = reader.GetVector2();
            Velocity = reader.GetVector2();
            Acceleration = reader.GetVector2();
            RotationVector = reader.GetVector2();
            AngularVelocity = reader.GetFloat();
            AimVector = reader.GetVector2();
        }
    }

    public struct PlayerShootStateS2C : INetSerializable
    {
        public float ShootLoadingSecondsLeft;
        public float ShootCooldown;

        public PlayerShootStateS2C(float shootCooldown)
        {
            ShootCooldown = shootCooldown;
            ShootLoadingSecondsLeft = ShootCooldown;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ShootLoadingSecondsLeft);
        }

        public void Deserialize(NetDataReader reader)
        {
            ShootLoadingSecondsLeft = reader.GetFloat();
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
            writer.Put((byte)CurrentHealth);
        }

        public void Deserialize(NetDataReader reader)
        {
            CurrentHealth = reader.GetByte();
        }
    }

    public struct PlayerBulletS2C : INetSerializable
    {
        public int Id;
        public int BelongToPlayerId;
        public Vector2 CurrentPosition;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put((byte)BelongToPlayerId);
            writer.Put(CurrentPosition);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            BelongToPlayerId = reader.GetByte();
            CurrentPosition = reader.GetVector2();
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