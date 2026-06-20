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
        public readonly FixedUnorderedList<PlayerOnTargetS2C> TargetedEnemyIds;

        public bool IsPlayerLockOnTargetSightShown => TargetedEnemyIds.Count > 0;

        public PlayerSpaceshipStateS2C(int maxTalents, int maxEnemiesAmount)
        {
            TalentsState = new PlayerTalentsStateS2C(maxTalents);
            TargetedEnemyIds = new FixedUnorderedList<PlayerOnTargetS2C>(maxEnemiesAmount);
        }

        public PlayerSpaceshipStateS2C GetClone()
        {
            var clone = new PlayerSpaceshipStateS2C(TalentsState.Talents.Capacity, TargetedEnemyIds.Capacity)
            {
                Transform = this.Transform,
                Shoot = this.Shoot,
                Health = this.Health,
                IsEngineOn = this.IsEngineOn,
                IsAlive = this.IsAlive,
                IsSpinned = this.IsSpinned,
                AssistArrowType = this.AssistArrowType,
                CurrentPowerUp = this.CurrentPowerUp,
            };

            clone.TargetedEnemyIds.Clear();
            for (int i = 0; i < TargetedEnemyIds.Count; i++)
            {
                ref var targetedEnemyId = ref clone.TargetedEnemyIds.AddAndGet();
                targetedEnemyId = this.TargetedEnemyIds[i];
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
            writer.Put((ushort)AssistArrowType);
            writer.Put(IsSpinned);
            writer.Put((byte)CurrentPowerUp);

            var targetedEnemyIdsAmount = TargetedEnemyIds.Count;
            writer.Put((byte) targetedEnemyIdsAmount);
            for (int i = 0; i < targetedEnemyIdsAmount; i++)
            {
                var targetedEnemy = TargetedEnemyIds[i];
                writer.Put((byte)targetedEnemy.PlayerTargetId);
                writer.Put(targetedEnemy.IsLockOnTargetShootable);
                writer.Put((byte)targetedEnemy.TargetType);
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
            AssistArrowType = (PlayerAssistArrowType)reader.GetUShort();
            IsSpinned = reader.GetBool();
            CurrentPowerUp = (PowerUpType)reader.GetByte();

            var targetedEnemyIdsAmount = reader.GetByte();
            TargetedEnemyIds.Clear();
            for (int i = 0; i < targetedEnemyIdsAmount; i++)
            {
                ref var targetedEnemy = ref TargetedEnemyIds.AddAndGet();
                targetedEnemy.PlayerTargetId = reader.GetByte();
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