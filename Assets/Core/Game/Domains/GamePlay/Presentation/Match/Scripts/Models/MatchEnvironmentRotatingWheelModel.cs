using System.Collections.Generic;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchEnvironmentRotatingWheelModel
    {
        public ushort Id { get; private set; }
        public Vector2 CenterPosition { get; private set; }
        public float RotationSpeed { get; private set; }
        public List<ushort> WallIds { get; private set; } 
        public List<ushort> LavaWallIds { get; private set; }
        public List<ushort> SpringIds { get; private set; }

        public MatchEnvironmentRotatingWheelModel(ushort id, Vector2 centerPosition, float rotationSpeed, List<ushort> wallIds, List<ushort> lavaWallIds, List<ushort> springIds)
        {
            Id = id;
            CenterPosition = centerPosition;
            RotationSpeed = rotationSpeed;
            WallIds = wallIds;
            LavaWallIds = lavaWallIds;
            SpringIds = springIds;
        }
    }
}
