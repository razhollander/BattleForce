using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchEnvironmentRotatingWheelModel
    {
        public ushort Id;
        public Vector2 CenterPosition;
        public readonly float RotationSpeed;
        public readonly List<ushort> WallIds;
        public readonly List<ushort> LavaWallIds;
        public readonly List<ushort> SpringIds;
        public readonly List<ushort> SpikeIds;
        public readonly List<RotatingTeleportGate> TeleportGates;

        public MatchEnvironmentRotatingWheelModel(ushort id, Vector2 centerPosition, float rotationSpeed, List<ushort> wallIds, List<ushort> lavaWallIds, List<ushort> springIds, List<ushort> spikeIds, List<RotatingTeleportGate> teleportGates)
        {
            Id = id;
            CenterPosition = centerPosition;
            RotationSpeed = rotationSpeed;
            WallIds = wallIds;
            LavaWallIds = lavaWallIds;
            SpringIds = springIds;
            SpikeIds = spikeIds;
            TeleportGates = teleportGates;
        }
    }
}
