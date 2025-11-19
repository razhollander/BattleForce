using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;
using UnityEngine;

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
    }

    public struct PlayerStateS2C : INetSerializable
    {
        public int Id;
        public string Name;
        public PlayerSpaceshipStateS2C Spaceship;

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
        public Vector2 CurrentPosition;
        public float CurrentRotation;
        public Vector2 CurrentVelocity;
        public float CurrentAimRotation;

        public Vector2 Velocity;
        public Vector2 AngularVelocity;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CurrentPosition);
            writer.Put(CurrentRotation);
            writer.Put(CurrentVelocity);
            writer.Put(CurrentAimRotation);
        }

        public void Deserialize(NetDataReader reader)
        {
            CurrentPosition = reader.GetVector2();
            CurrentRotation = reader.GetFloat();
            CurrentVelocity = reader.GetVector2();
            CurrentAimRotation = reader.GetFloat();
        }
    }

    public struct PlayerShootStateS2C : INetSerializable
    {
        public float ShootLoadingSecondsLeft;

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