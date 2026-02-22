using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Models
{
    public class MatchEnvironmentTeleportPairModel
    {
        public ushort Id;
        public MatchEnvironmentTeleportGateModel GateA;
        public MatchEnvironmentTeleportGateModel GateB;
        public Vector2 Size;
        
        public MatchEnvironmentTeleportPairModel(ushort pairId, ushort gateAId, Vector2 gateAPosition, float gateANormalRotation, ushort gateBId, Vector2 gateBPosition, float gateBNormalRotation, Vector2 size)
        {
            Id = pairId;
            GateA = new MatchEnvironmentTeleportGateModel(gateAId, gateAPosition, gateANormalRotation);
            GateB = new MatchEnvironmentTeleportGateModel(gateBId, gateBPosition, gateBNormalRotation);
            Size = size;
        }
    }

    public class MatchEnvironmentTeleportGateModel
    {
        public ushort Id;
        public Vector2 Position;
        public float NormalRotation;

        public MatchEnvironmentTeleportGateModel(ushort id, Vector2 position, float normalRotation)
        {
            Id = id;
            Position = position;
            NormalRotation = normalRotation;
        }
    }
}