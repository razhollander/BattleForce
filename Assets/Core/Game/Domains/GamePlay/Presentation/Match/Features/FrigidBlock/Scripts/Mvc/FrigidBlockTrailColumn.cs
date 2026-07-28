using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc
{
    public readonly struct FrigidBlockTrailColumn
    {
        public readonly Vector3 LeftEdgeWorldPosition;
        public readonly Vector3 RightEdgeWorldPosition;
        public readonly float SpawnTimeInSeconds;

        public FrigidBlockTrailColumn(Vector3 leftEdgeWorldPosition, Vector3 rightEdgeWorldPosition, float spawnTimeInSeconds)
        {
            LeftEdgeWorldPosition = leftEdgeWorldPosition;
            RightEdgeWorldPosition = rightEdgeWorldPosition;
            SpawnTimeInSeconds = spawnTimeInSeconds;
        }
    }
}
