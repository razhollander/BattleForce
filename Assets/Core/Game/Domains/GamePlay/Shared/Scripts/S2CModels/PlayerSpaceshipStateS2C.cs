using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;
using LiteNetLib.Utils;
using Core.Scripts.Utils.CustomCollections;


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
        public FixedUnorderedList<ushort> PlayerHeartsOnTarget;

        public PlayerSpaceshipStateS2C(int maxTalents)
        {
            TalentsState = new PlayerTalentsStateS2C(maxTalents);
            PlayerHeartsOnTarget = new FixedUnorderedList<ushort>(10);
        }

        public void PushAndSpin(Vector2 pushForce, float spinAmount, bool shouldTurnOffEngine = true)
        {
            Transform.Velocity += pushForce;
            Transform.AngularVelocity += spinAmount;
            Transform.Direction = pushForce.Normalize();

            if (shouldTurnOffEngine)
            {
                IsEngineOn = false;
            }
        }
        
        public PlayerSpaceshipStateS2C GetClone()
        {
            var clone = new PlayerSpaceshipStateS2C(TalentsState.Talents.Capacity)
            {
                IsEngineOn = this.IsEngineOn,
                IsAlive = this.IsAlive,
                Shoot = this.Shoot,
                Transform = this.Transform,
                Health = this.Health,
            };

            clone.TalentsState.CopyFrom(this.TalentsState);
            var count = this.PlayerHeartsOnTarget.Count;
            for (var i = 0; i < count; i++)
            {
                ref var target = ref clone.PlayerHeartsOnTarget.AddAndGet();
                target = this.PlayerHeartsOnTarget[i];
            }

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
            writer.Put((byte)PlayerHeartsOnTarget.Count);
            foreach (var target in PlayerHeartsOnTarget.AsSpan())
            {
                writer.Put(target);
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
            PlayerHeartsOnTarget.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var target = ref PlayerHeartsOnTarget.AddAndGet();
                target = reader.GetUShort();
            }

        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            Transform.SerializeDeltas(writer);
            Shoot.SerializeDeltas(writer);
            TalentsState.SerializeDeltas(writer);
            writer.Put((ushort)AssistArrowType);
            writer.Put((byte)PlayerHeartsOnTarget.Count);
            foreach (var target in PlayerHeartsOnTarget.AsSpan())
            {
                writer.Put(target);
            }

        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            Transform.DeserializeDeltas(reader);
            Shoot.DeserializeDeltas(reader);
            TalentsState.DeserializeDeltas(reader);
            AssistArrowType = (PlayerAssistArrowType)reader.GetUShort();
            PlayerHeartsOnTarget.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var target = ref PlayerHeartsOnTarget.AddAndGet();
                target = reader.GetUShort();
            }

        }
    }
}