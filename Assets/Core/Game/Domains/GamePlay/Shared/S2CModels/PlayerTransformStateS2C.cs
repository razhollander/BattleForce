using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct SimulationStateS2C : INetSerializable
    {
        public int Tick;
        public int PlayersCount;
        public PlayerStateS2C[] Players;
        //public int BulletsCount;
        //public PlayerBulletS2C[] Bullets;
        public StructPool<PlayerBulletS2C> Bullets;
        public List<BulletSpawnNetEventS2C> BulletSpawnNetEvents; // todo: remove events related to bullet when bullet id destroyed
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
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

            if (!BulletSpawnNetEvents.IsNullOrEmpty())
            {
                writer.Put((ushort)BulletSpawnNetEvents.Count);
                foreach (var bulletSpawnEvent in BulletSpawnNetEvents)
                {
                    bulletSpawnEvent.Serialize(writer);
                }
            }
            else
            {
                writer.Put((ushort)0);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
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
            var bulletSpawnNetEventsCount = reader.GetUShort();
            if (bulletSpawnNetEventsCount > 0)
            {
                var bulletSpawnNetEventsArray = new BulletSpawnNetEventS2C[bulletSpawnNetEventsCount];
                for (int i = 0; i < bulletSpawnNetEventsCount; i++)
                {
                    bulletSpawnNetEventsArray[i].Deserialize(reader);
                }

                BulletSpawnNetEvents = bulletSpawnNetEventsArray.ToList();
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
        public Vector2 Direction;
        public float AngularVelocity;
        public Vector2 AimVector;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Position);
            writer.Put(Velocity);
            writer.Put(Acceleration);
            writer.Put(Direction);
            writer.Put(AngularVelocity);
            writer.Put(AimVector);
        }

        public void Deserialize(NetDataReader reader)
        {
            Position = reader.GetVector2();
            Velocity = reader.GetVector2();
            Acceleration = reader.GetVector2();
            Direction = reader.GetVector2();
            AngularVelocity = reader.GetFloat();
            AimVector = reader.GetVector2();
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