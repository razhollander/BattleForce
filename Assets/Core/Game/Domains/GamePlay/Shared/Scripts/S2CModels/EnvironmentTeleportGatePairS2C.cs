using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class EnvironmentTeleportGatePairS2C : IEquatable<ushort>
    {
        public ushort Id;
        public EnvironmentTeleportGateS2C GateA;
        public EnvironmentTeleportGateS2C GateB;

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }

    public struct EnvironmentTeleportGateS2C
    {
        public int Id;
        public Vector2 Position;
        public float NormalRotation;
    }
}
