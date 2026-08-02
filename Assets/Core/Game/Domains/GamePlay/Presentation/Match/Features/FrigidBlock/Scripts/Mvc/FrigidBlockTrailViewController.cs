using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc
{
    public class FrigidBlockTrailViewController
    {
        private const float MIN_COLUMN_SPAWN_DISTANCE = 0.0001f;
        private const float MIN_COLUMN_LIFETIME_IN_SECONDS = 0.0001f;
        private const int MIN_COLUMNS_CAPACITY = 2;
        private const int LIVE_LEADING_COLUMNS_COUNT = 1;
        private const float FULLY_OPAQUE_ALPHA = 1f;
        private const float EMITTERS_MIDPOINT_WEIGHT = 0.5f;

        private readonly Transform _leftEdgeEmitterTransform;
        private readonly Transform _rightEdgeEmitterTransform;
        private readonly FrigidBlockTrailColumnsBuffer _columnsBuffer;
        private readonly FrigidBlockTrailMeshBuilder _meshBuilder;
        private readonly float _columnSpawnDistanceSquared;
        private readonly float _columnLifetimeInSeconds;

        private Vector3 _lastCommittedColumnCenterWorldPosition;

        public FrigidBlockTrailViewController(FrigidBlockView view)
        {
            _leftEdgeEmitterTransform = view.TrailLeftEdgeEmitter;
            _rightEdgeEmitterTransform = view.TrailRightEdgeEmitter;

            var columnSpawnDistance = Mathf.Max(MIN_COLUMN_SPAWN_DISTANCE, view.TrailColumnSpawnDistance);
            _columnSpawnDistanceSquared = columnSpawnDistance * columnSpawnDistance;
            _columnLifetimeInSeconds = Mathf.Max(MIN_COLUMN_LIFETIME_IN_SECONDS, view.TrailColumnLifetimeInSeconds);

            var columnsCapacity = Mathf.Max(MIN_COLUMNS_CAPACITY, view.MaxTrailColumns);
            _columnsBuffer = new FrigidBlockTrailColumnsBuffer(columnsCapacity);
            _meshBuilder = new FrigidBlockTrailMeshBuilder(view.TrailMesh, view.TrailTransform, columnsCapacity + LIVE_LEADING_COLUMNS_COUNT);
        }

        public void CollapseTrailOntoEmitters(float currentTimeInSeconds)
        {
            var leftEdgeWorldPosition = _leftEdgeEmitterTransform.position;
            var rightEdgeWorldPosition = _rightEdgeEmitterTransform.position;

            _columnsBuffer.Clear();
            _lastCommittedColumnCenterWorldPosition = GetCenterWorldPosition(leftEdgeWorldPosition, rightEdgeWorldPosition);
            CommitColumn(leftEdgeWorldPosition, rightEdgeWorldPosition, currentTimeInSeconds);
            RebuildMesh(leftEdgeWorldPosition, rightEdgeWorldPosition, currentTimeInSeconds);
        }

        public void UpdateTrail(float currentTimeInSeconds)
        {
            var leftEdgeWorldPosition = _leftEdgeEmitterTransform.position;
            var rightEdgeWorldPosition = _rightEdgeEmitterTransform.position;

            _columnsBuffer.RemoveColumnsSpawnedBefore(currentTimeInSeconds - _columnLifetimeInSeconds);
            TryCommitColumn(leftEdgeWorldPosition, rightEdgeWorldPosition, currentTimeInSeconds);
            RebuildMesh(leftEdgeWorldPosition, rightEdgeWorldPosition, currentTimeInSeconds);
        }

        private void TryCommitColumn(Vector3 leftEdgeWorldPosition, Vector3 rightEdgeWorldPosition, float currentTimeInSeconds)
        {
            var centerWorldPosition = GetCenterWorldPosition(leftEdgeWorldPosition, rightEdgeWorldPosition);
            var distanceSinceLastColumnSquared = (centerWorldPosition - _lastCommittedColumnCenterWorldPosition).sqrMagnitude;

            if (distanceSinceLastColumnSquared < _columnSpawnDistanceSquared)
            {
                return;
            }

            CommitColumn(leftEdgeWorldPosition, rightEdgeWorldPosition, currentTimeInSeconds);
            _lastCommittedColumnCenterWorldPosition = centerWorldPosition;
        }

        private void CommitColumn(Vector3 leftEdgeWorldPosition, Vector3 rightEdgeWorldPosition, float spawnTimeInSeconds)
        {
            _columnsBuffer.AddNewestColumn(new FrigidBlockTrailColumn(leftEdgeWorldPosition, rightEdgeWorldPosition, spawnTimeInSeconds));
        }

        private void RebuildMesh(Vector3 liveLeftEdgeWorldPosition, Vector3 liveRightEdgeWorldPosition, float currentTimeInSeconds)
        {
            _meshBuilder.StartBuilding(_columnsBuffer.ColumnsCount + LIVE_LEADING_COLUMNS_COUNT);

            for (var orderFromOldest = 0; orderFromOldest < _columnsBuffer.ColumnsCount; orderFromOldest++)
            {
                var column = _columnsBuffer.GetColumn(orderFromOldest);
                _meshBuilder.AddColumn(column.LeftEdgeWorldPosition, column.RightEdgeWorldPosition, GetFadeOutAlpha01(column, currentTimeInSeconds));
            }

            _meshBuilder.AddColumn(liveLeftEdgeWorldPosition, liveRightEdgeWorldPosition, FULLY_OPAQUE_ALPHA);
            _meshBuilder.FinishBuilding();
        }

        private float GetFadeOutAlpha01(FrigidBlockTrailColumn column, float currentTimeInSeconds)
        {
            var columnAgeInSeconds = currentTimeInSeconds - column.SpawnTimeInSeconds;

            return Mathf.Clamp01(1f - columnAgeInSeconds / _columnLifetimeInSeconds);
        }

        private Vector3 GetCenterWorldPosition(Vector3 leftEdgeWorldPosition, Vector3 rightEdgeWorldPosition)
        {
            return (leftEdgeWorldPosition + rightEdgeWorldPosition) * EMITTERS_MIDPOINT_WEIGHT;
        }
    }
}
