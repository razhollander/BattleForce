using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;
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

        public PlayerSpaceshipStateS2C(int maxTalents)
        {
            TalentsState = new PlayerTalentsStateS2C(maxTalents);
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