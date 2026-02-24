using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Models
{
    public class MatchEnvironmentTeleportPairModel
    {
        public ushort Id;
        public MatchEnvironmentTeleportGateModel GateA;
        public MatchEnvironmentTeleportGateModel GateB;
        public Vector2 Size;
        
        public MatchEnvironmentTeleportPairModel(ushort pairId, ushort gateAId, Vector2 gateAPosition, float gateANormalRotation, ushort gateBId, Vector2 gateBPosition, float gateBNormalRotation, Vector2 gateAWorldPosition, float gateAWorldRotation, Vector2 gateBWorldPosition, float gateBWorldRotation, Vector2 size)
        {
            Id = pairId;
            GateA = new MatchEnvironmentTeleportGateModel(gateAId, gateAPosition, gateANormalRotation, gateAWorldPosition, gateAWorldRotation);
            GateB = new MatchEnvironmentTeleportGateModel(gateBId, gateBPosition, gateBNormalRotation, gateBWorldPosition, gateBWorldRotation);
            Size = size;
        }
    }

    public class MatchEnvironmentTeleportGateModel
    {
        public ushort Id;
        public Vector2 LocalPosition;
        public float LocalRotation;
        public Vector2 WorldPosition;
        public float WorldRotation;

        public MatchEnvironmentTeleportGateModel(ushort id, Vector2 localPosition, float localRotation, Vector2 worldPosition, float worldRotation)
        {
            Id = id;
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            WorldPosition = worldPosition;
            WorldRotation = worldRotation;
        }
    }
}