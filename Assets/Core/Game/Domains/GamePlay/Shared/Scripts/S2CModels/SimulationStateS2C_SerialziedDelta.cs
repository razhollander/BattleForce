// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Numerics;
// using Core.Game.Domains.GamePlay.Shared.Extensions;
// using CoreDomain.Scripts.Services.Logger.Base;
// using LiteNetLib.Utils;
// using Sirenix.Utilities;
//
// namespace Core.Game.Domains.GamePlay.Shared.S2CModels
// {
//     #region Simulation state
//
//     public struct SimulationStateS2C
//     {
//         public int PlayersCount;
//         public PlayerStateS2C[] Players;
//         public StructPool<PlayerBulletS2C> Bullets;
//
//         /// <summary>
//         /// Delta-serialize against a previous snapshot (other).
//         /// Assumes same player ordering & count; if not, you’ll want to do ID-based matching.
//         /// </summary>
//         public void SerializeDelta(SimulationStateS2C other, NetDataWriter writer)
//         {
//             writer.Put((byte)PlayersCount);
//             var didPlayerCountChange = other.PlayersCount != PlayersCount;
//             if (didPlayerCountChange)
//             {
//                 foreach (var player in Players)
//                 {
//                     player.Serialize(writer);
//                 }
//             }
//             else
//             {
//                 for (int i = 0; i < PlayersCount; i++)
//                 {
//                     var current = Players[i];
//                     PlayerStateS2C previous = default;
//
//                     if (i < other.PlayersCount)
//                         previous = other.Players[i];
//
//                     current.SerializeDelta(previous, writer);
//                 }
//             }
//
//             // next time:
//             // need to serialize and desirialize only deltas, the hard part is what happens if the amount of bullets/players changes 
//             
//             // Bullets: for now, send full snapshot (no per-field delta)
//             // You can later move to bullet ID–based delta + spawn/despawn events.
//             var bulletsCount = Bullets.UsedCount;
//             writer.Put((byte)bulletsCount);
//             if (bulletsCount > 0)
//             {
//                 foreach (var bulletIndex in Bullets.UsedIndices())
//                 {
//                     Bullets[bulletIndex].Serialize(writer);
//                 }
//             }
//         }
//
//         public void Deserialize(NetDataReader reader)
//         {
//             PlayersCount = reader.GetByte();
//             Players = new PlayerStateS2C[PlayersCount];
//             for (int i = 0; i < PlayersCount; i++)
//                 Players[i].Deserialize(reader);
//
//             var bulletsCount = (int)reader.GetByte();
//             Bullets = new StructPool<PlayerBulletS2C>(bulletsCount);
//             if (bulletsCount > 0)
//             {
//                 for (int i = 0; i < bulletsCount; i++)
//                 {
//                     Bullets.Rent(out int index);
//                     Bullets[index].Deserialize(reader);
//                 }
//             }
//         }
//
//         public PlayerStateS2C GetPlayer(int playerId)
//         {
//             return Players.First(x => x.Id == playerId);
//         }
//
//         public PlayerBulletS2C GetBullet(int bulletId)
//         {
//             foreach (var index in Bullets.UsedIndices())
//             {
//                 var playerBullet = Bullets[index];
//                 if (playerBullet.Id == bulletId)
//                 {
//                     return playerBullet;
//                 }
//             }
//
//             LogService.LogError($"No bullet for id {bulletId}!");
//             return default;
//         }
//     }
//
//     #endregion
//
//     #region Player state
//
//     [Flags]
//     public enum PlayerStateDirty : byte
//     {
//         None = 0,
//         Name = 1 << 0,
//         // Spaceship is always written via its own delta, so no flag needed here.
//     }
//
//     public struct PlayerStateS2C : INetSerializable
//     {
//         public int Id;
//         public string Name;
//         public PlayerSpaceshipStateS2C Spaceship;
//
//         public PlayerStateS2C(int id, string name, PlayerSpaceshipStateS2C spaceship)
//         {
//             Id = id;
//             Name = name;
//             Spaceship = spaceship;
//         }
//
//         public void Serialize(NetDataWriter writer)
//         {
//             writer.Put((byte)Id);
//             Spaceship.Serialize(writer);
//         }
//
//         public void Deserialize(NetDataReader reader)
//         {
//             Id = reader.GetByte();
//             Spaceship.Deserialize(reader);
//         }
//
//         /// <summary>
//         /// Delta serialize vs previous PlayerStateS2C.
//         /// Always writes Id (identity), then dirty mask, then changed fields.
//         /// Spaceship is always delta-serialized.
//         /// </summary>
//         public void SerializeDelta(PlayerStateS2C other, NetDataWriter writer)
//         {
//             // Identity first so the client knows which player this belongs to.
//             writer.Put((byte)Id);
//
//             byte dirty = 0;
//
//             if (!string.Equals(Name, other.Name))
//                 dirty |= (byte)PlayerStateDirty.Name;
//
//             writer.Put(dirty);
//
//             if ((dirty & (byte)PlayerStateDirty.Name) != 0)
//                 writer.Put(Name);
//
//             // Delegate to spaceship components; they each have their own masks.
//             Spaceship.SerializeDelta(other.Spaceship, writer);
//         }
//     }
//
//     #endregion
//
//     #region Spaceship state + sub-components
//
//     public struct PlayerSpaceshipStateS2C : INetSerializable
//     {
//         public PlayerTransformStateS2C Transform;
//         public PlayerShootStateS2C Shoot;
//         public PlayerHealthS2C Health;
//
//         public PlayerSpaceshipStateS2C(PlayerTransformStateS2C transform, float shootCooldown, int health)
//         {
//             Transform = transform;
//             Shoot = new PlayerShootStateS2C(shootCooldown);
//             Health = new PlayerHealthS2C(health);
//         }
//
//         public void Serialize(NetDataWriter writer)
//         {
//             Transform.Serialize(writer);
//             Shoot.Serialize(writer);
//             Health.Serialize(writer);
//         }
//
//         public void Deserialize(NetDataReader reader)
//         {
//             Transform.Deserialize(reader);
//             Shoot.Deserialize(reader);
//             Health.Deserialize(reader);
//         }
//
//         public void SerializeDelta(PlayerSpaceshipStateS2C other, NetDataWriter writer)
//         {
//             Transform.SerializeDelta(other.Transform, writer);
//             Shoot.SerializeDelta(other.Shoot, writer);
//             Health.SerializeDelta(other.Health, writer);
//         }
//     }
//
//     [Flags]
//     public enum PlayerTransformDirty : byte
//     {
//         None      = 0,
//         Position  = 1 << 0,
//         Direction = 1 << 1,
//         // Velocity    = 1 << 2,
//         // Acceleration= 1 << 3,
//         // Radius      = 1 << 4,
//         // AngularVel  = 1 << 5,
//         // AimVector   = 1 << 6,
//     }
//
//     public struct PlayerTransformStateS2C : INetSerializable
//     {
//         public Vector2 Position;
//         public Vector2 Velocity;
//         public Vector2 Acceleration;
//         public Vector2 Direction;
//         public float   Radius;
//         public float   AngularVelocity;
//         public Vector2 AimVector;
//
//         public Vector2 GetHeadPosition()
//         {
//             return Position + Direction * Radius;
//         }
//
//         public void Serialize(NetDataWriter writer)
//         {
//             writer.Put(Position);
//             // writer.Put(Velocity);
//             // writer.Put(Acceleration);
//             writer.Put(Direction);
//             // writer.Put(AngularVelocity);
//             // writer.Put(AimVector);
//         }
//
//         public void Deserialize(NetDataReader reader)
//         {
//             Position = reader.GetVector2();
//             // Velocity = reader.GetVector2();
//             // Acceleration = reader.GetVector2();
//             Direction = reader.GetVector2();
//             // AngularVelocity = reader.GetFloat();
//             // AimVector = reader.GetVector2();
//         }
//
//         public void SerializeDelta(PlayerTransformStateS2C other, NetDataWriter writer)
//         {
//             byte dirty = 0;
//
//             if (Position != other.Position)
//                 dirty |= (byte)PlayerTransformDirty.Position;
//
//             if (Direction != other.Direction)
//                 dirty |= (byte)PlayerTransformDirty.Direction;
//
//             writer.Put(dirty);
//
//             if ((dirty & (byte)PlayerTransformDirty.Position) != 0)
//                 writer.Put(Position);
//
//             if ((dirty & (byte)PlayerTransformDirty.Direction) != 0)
//                 writer.Put(Direction);
//         }
//     }
//
//     [Flags]
//     public enum PlayerShootDirty : byte
//     {
//         None               = 0,
//         CooldownSecondsLeft = 1 << 0,
//         MaxCooldown         = 1 << 1,
//     }
//
//     public struct PlayerShootStateS2C : INetSerializable
//     {
//         public float CooldownSecondsLeft;
//         public float MaxCooldown;
//
//         public PlayerShootStateS2C(float maxCooldown)
//         {
//             MaxCooldown = maxCooldown;
//             CooldownSecondsLeft = MaxCooldown;
//         }
//
//         public void Serialize(NetDataWriter writer)
//         {
//             writer.Put(CooldownSecondsLeft);
//             writer.Put(MaxCooldown);
//         }
//
//         public void Deserialize(NetDataReader reader)
//         {
//             CooldownSecondsLeft = reader.GetFloat();
//             MaxCooldown = reader.GetFloat();
//         }
//
//         public void SerializeDelta(PlayerShootStateS2C other, NetDataWriter writer)
//         {
//             byte dirty = 0;
//
//             if (CooldownSecondsLeft != other.CooldownSecondsLeft)
//                 dirty |= (byte)PlayerShootDirty.CooldownSecondsLeft;
//
//             if (MaxCooldown != other.MaxCooldown)
//                 dirty |= (byte)PlayerShootDirty.MaxCooldown;
//
//             writer.Put(dirty);
//
//             if ((dirty & (byte)PlayerShootDirty.CooldownSecondsLeft) != 0)
//                 writer.Put(CooldownSecondsLeft);
//
//             if ((dirty & (byte)PlayerShootDirty.MaxCooldown) != 0)
//                 writer.Put(MaxCooldown);
//         }
//     }
//
//     [Flags]
//     public enum PlayerHealthDirty : byte
//     {
//         None         = 0,
//         MaxHealth    = 1 << 0,
//         CurrentHealth= 1 << 1,
//     }
//
//     public struct PlayerHealthS2C : INetSerializable
//     {
//         public int MaxHealth;
//         public int CurrentHealth;
//
//         public PlayerHealthS2C(int maxHealth) : this()
//         {
//             MaxHealth = maxHealth;
//             CurrentHealth = MaxHealth;
//         }
//
//         public void Serialize(NetDataWriter writer)
//         {
//             writer.Put((byte)CurrentHealth);
//         }
//
//         public void Deserialize(NetDataReader reader)
//         {
//             CurrentHealth = reader.GetByte();
//         }
//
//         public void SerializeDelta(PlayerHealthS2C other, NetDataWriter writer)
//         {
//             byte dirty = 0;
//
//             if (MaxHealth != other.MaxHealth)
//                 dirty |= (byte)PlayerHealthDirty.MaxHealth;
//
//             if (CurrentHealth != other.CurrentHealth)
//                 dirty |= (byte)PlayerHealthDirty.CurrentHealth;
//
//             writer.Put(dirty);
//
//             if ((dirty & (byte)PlayerHealthDirty.CurrentHealth) != 0)
//                 writer.Put((byte)CurrentHealth);
//
//             if ((dirty & (byte)PlayerHealthDirty.MaxHealth) != 0)
//                 writer.Put((byte)MaxHealth);
//         }
//     }
//
//     #endregion
//
//     #region Bullets + events
//
//     [Flags]
//     public enum PlayerBulletDirty : byte
//     {
//         None            = 0,
//         BelongToPlayerId= 1 << 0,
//         Position        = 1 << 1,
//         MoveSpeed       = 1 << 2,
//         Direction       = 1 << 3,
//     }
//
//     public struct PlayerBulletS2C : INetSerializable
//     {
//         public int Id;
//         public int BelongToPlayerId;
//         public Vector2 Position;
//         public float MoveSpeed;
//         public Vector2 Direction;
//
//         public void Serialize(NetDataWriter writer)
//         {
//             writer.Put((byte)Id);
//             writer.Put((byte)BelongToPlayerId);
//             writer.Put(Position);
//         }
//
//         public void Deserialize(NetDataReader reader)
//         {
//             Id = reader.GetByte();
//             BelongToPlayerId = reader.GetByte();
//             Position = reader.GetVector2();
//         }
//
//         /// <summary>
//         /// Delta serialization for bullet state.
//         /// Always writes Id (identity) + dirty mask, then changed fields.
//         /// </summary>
//         public void SerializeDelta(PlayerBulletS2C other, NetDataWriter writer)
//         {
//             // Identity
//             writer.Put((byte)Id);
//
//             byte dirty = 0;
//
//             if (BelongToPlayerId != other.BelongToPlayerId)
//                 dirty |= (byte)PlayerBulletDirty.BelongToPlayerId;
//
//             if (Position != other.Position)
//                 dirty |= (byte)PlayerBulletDirty.Position;
//
//             if (MoveSpeed != other.MoveSpeed)
//                 dirty |= (byte)PlayerBulletDirty.MoveSpeed;
//
//             if (Direction != other.Direction)
//                 dirty |= (byte)PlayerBulletDirty.Direction;
//
//             writer.Put(dirty);
//
//             if ((dirty & (byte)PlayerBulletDirty.BelongToPlayerId) != 0)
//                 writer.Put((byte)BelongToPlayerId);
//
//             if ((dirty & (byte)PlayerBulletDirty.Position) != 0)
//                 writer.Put(Position);
//
//             if ((dirty & (byte)PlayerBulletDirty.MoveSpeed) != 0)
//                 writer.Put(MoveSpeed);
//
//             if ((dirty & (byte)PlayerBulletDirty.Direction) != 0)
//                 writer.Put(Direction);
//         }
//     }
//
//     public struct PlayerShootBulletEventS2C
//     {
//         public byte BulletId;
//         public byte PlayerId;
//         public Vector2 ShootPosition;
//     }
//
//     public struct BulletHitPlayerEventS2C
//     {
//         public byte BulletId;
//         public byte PlayerId;
//     }
//
//     #endregion
// }
