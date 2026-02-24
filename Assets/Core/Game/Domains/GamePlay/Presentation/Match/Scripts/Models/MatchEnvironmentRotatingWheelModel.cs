using System.Collections.Generic;
using System.Numerics;

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
        public readonly List<ushort> TeleportGatePairIds;

        public MatchEnvironmentRotatingWheelModel(ushort id, Vector2 centerPosition, float rotationSpeed, List<ushort> wallIds, List<ushort> lavaWallIds, List<ushort> springIds, List<ushort> teleportGatePairIds)
        {
            Id = id;
            CenterPosition = centerPosition;
            RotationSpeed = rotationSpeed;
            WallIds = wallIds;
            LavaWallIds = lavaWallIds;
            SpringIds = springIds;
            TeleportGatePairIds = teleportGatePairIds;
        }
    }
}
