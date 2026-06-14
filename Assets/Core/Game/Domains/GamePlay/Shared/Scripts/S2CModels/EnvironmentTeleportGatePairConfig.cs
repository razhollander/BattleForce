using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class EnvironmentTeleportGatePairConfig
    {
        private const ushort GateCount = 2;
        
        public ushort Id;
        public EnvironmentTeleportGateConfig GateA;
        public EnvironmentTeleportGateConfig GateB;
        public ushort GateAId => (ushort) (Id * GateCount);
        public ushort GateBId => (ushort) (Id * GateCount + 1);
    }

    [Serializable]
    public struct EnvironmentTeleportGateConfig
    {
        private const ushort NO_ATTACH_TO_ROTATION_WHEEL_ID = 0;
        
        public Vector2 Position;
        public float NormalRotation;
        public ushort AttachToRotationWheelId;
        public bool IsAttachedToRotationWheel => AttachToRotationWheelId != NO_ATTACH_TO_ROTATION_WHEEL_ID;
        public EnvironmentTeleportGateConfig(Vector2 position, float normalRotation, ushort attachToRotationWheelId = NO_ATTACH_TO_ROTATION_WHEEL_ID)
        {
            Position = position;
            NormalRotation = normalRotation;
            AttachToRotationWheelId = attachToRotationWheelId;
        }
    }
}