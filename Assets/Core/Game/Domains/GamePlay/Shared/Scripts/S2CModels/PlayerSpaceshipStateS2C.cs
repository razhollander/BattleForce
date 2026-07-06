using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public class PlayerSpaceshipStateS2C : INetSerializable
    {
        public PlayerTransformStateS2C Transform;
        public PlayerShootStateS2C Shoot;
        public PlayerHealthS2C Health;
        public PlayerTalentsStateS2C TalentsState;
        public bool IsEngineOn = true;
        public bool IsAlive = true;
        public bool IsSpinned;
        public PlayerAssistArrowType AssistArrowType;
        public PowerUpType CurrentPowerUp;
        public bool IsPowerUpCurrentlyActive;
        public bool IsCurrentlyInGrantingPowerUpPhase;
        public readonly FixedUnorderedList<ObjectLockedOnTargetS2C> LockOnTargetObjects;

        public bool IsPlayerLockOnTargetSightShown => LockOnTargetObjects.Count > 0;

        public PlayerSpaceshipStateS2C(int maxTalents, int maxEnemiesAmount)
        {
            TalentsState = new PlayerTalentsStateS2C(maxTalents);
            LockOnTargetObjects = new FixedUnorderedList<ObjectLockedOnTargetS2C>(maxEnemiesAmount);
        }

        public PlayerSpaceshipStateS2C GetClone()
        {
            var clone = new PlayerSpaceshipStateS2C(TalentsState.Talents.Capacity, LockOnTargetObjects.Capacity)
            {
                Transform = this.Transform,
                Shoot = this.Shoot,
                Health = this.Health,
                IsEngineOn = this.IsEngineOn,
                IsAlive = this.IsAlive,
                IsSpinned = this.IsSpinned,
                AssistArrowType = this.AssistArrowType,
                CurrentPowerUp = this.CurrentPowerUp,
                IsPowerUpCurrentlyActive = this.IsPowerUpCurrentlyActive,
                IsCurrentlyInGrantingPowerUpPhase = this.IsCurrentlyInGrantingPowerUpPhase,
            };

            clone.LockOnTargetObjects.Clear();
            for (int i = 0; i < LockOnTargetObjects.Count; i++)
            {
                ref var lockOnTargetObject = ref clone.LockOnTargetObjects.AddAndGet();
                lockOnTargetObject = this.LockOnTargetObjects[i];
            }
            
            clone.TalentsState.CopyFrom(this.TalentsState);
            return clone;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            Transform.Serialize(writer);
            Shoot.Serialize(writer);
            Health.Serialize(writer);
            TalentsState.Serialize(writer);
            writer.Put(IsEngineOn);
            writer.Put(IsAlive);
            writer.Put((byte)AssistArrowType);
            writer.Put(IsSpinned);
            writer.Put((byte)CurrentPowerUp);
            writer.Put(IsPowerUpCurrentlyActive);
            writer.Put(IsCurrentlyInGrantingPowerUpPhase);

            var lockOnTargetObjectsAmount = LockOnTargetObjects.Count;
            writer.Put((byte) lockOnTargetObjectsAmount);
            for (int i = 0; i < lockOnTargetObjectsAmount; i++)
            {
                var lockOnTargetObject = LockOnTargetObjects[i];
                writer.Put((byte)lockOnTargetObject.TargetId);
                writer.Put(lockOnTargetObject.IsLockOnTargetShootable);
                writer.Put((byte)lockOnTargetObject.TargetType);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            Transform.Deserialize(reader);
            Shoot.Deserialize(reader);
            Health.Deserialize(reader);
            TalentsState.Deserialize(reader);
            IsEngineOn = reader.GetBool();
            IsAlive = reader.GetBool();
            AssistArrowType = (PlayerAssistArrowType) reader.GetByte();
            IsSpinned = reader.GetBool();
            CurrentPowerUp = (PowerUpType)reader.GetByte();
            IsPowerUpCurrentlyActive = reader.GetBool();
            IsCurrentlyInGrantingPowerUpPhase = reader.GetBool();

            var lockOnTargetObjectsAmount = reader.GetByte();
            LockOnTargetObjects.Clear();
            for (int i = 0; i < lockOnTargetObjectsAmount; i++)
            {
                ref var targetedEnemy = ref LockOnTargetObjects.AddAndGet();
                targetedEnemy.TargetId = reader.GetByte();
                targetedEnemy.IsLockOnTargetShootable = reader.GetBool();
                targetedEnemy.TargetType = (LockOnTargetType)reader.GetByte();
            }
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            Transform.SerializeDeltas(writer);
            Shoot.SerializeDeltas(writer);
            TalentsState.SerializeDeltas(writer);
            writer.Put((ushort)AssistArrowType);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            Transform.DeserializeDeltas(reader);
            Shoot.DeserializeDeltas(reader);
            TalentsState.DeserializeDeltas(reader);
            AssistArrowType = (PlayerAssistArrowType)reader.GetUShort();
        }
    }
}