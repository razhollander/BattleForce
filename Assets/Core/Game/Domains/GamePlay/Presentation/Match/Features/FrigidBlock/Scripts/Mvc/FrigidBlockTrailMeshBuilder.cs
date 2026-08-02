using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc
{
    public class FrigidBlockTrailMeshBuilder
    {
        private const int VERTICES_PER_COLUMN = 2;
        private const int INDICES_PER_QUAD = 6;
        private const int MIN_COLUMNS_FOR_RIBBON = 2;
        private const byte FULL_COLOR_CHANNEL = 255;
        private const float LEFT_EDGE_HORIZONTAL_UV = 0f;
        private const float RIGHT_EDGE_HORIZONTAL_UV = 1f;
        private const int MAIN_SUB_MESH_INDEX = 0;
        private const int MAIN_UV_CHANNEL_INDEX = 0;

        private readonly Mesh _mesh;
        private readonly Transform _meshTransform;
        private readonly List<Vector3> _localPositionsList;
        private readonly List<Color32> _colorsList;
        private readonly List<Vector2> _uvsList;
        private readonly List<int> _trianglesList;

        private Matrix4x4 _worldToLocalMatrix;
        private int _expectedColumnsCount;
        private int _addedColumnsCount;

        public FrigidBlockTrailMeshBuilder(Mesh mesh, Transform meshTransform, int maxColumnsCount)
        {
            _mesh = mesh;
            _meshTransform = meshTransform;
            _localPositionsList = new List<Vector3>(maxColumnsCount * VERTICES_PER_COLUMN);
            _colorsList = new List<Color32>(maxColumnsCount * VERTICES_PER_COLUMN);
            _uvsList = new List<Vector2>(maxColumnsCount * VERTICES_PER_COLUMN);
            _trianglesList = new List<int>((maxColumnsCount - 1) * INDICES_PER_QUAD);
        }

        public void StartBuilding(int expectedColumnsCount)
        {
            _expectedColumnsCount = expectedColumnsCount;
            _addedColumnsCount = 0;
            _worldToLocalMatrix = _meshTransform.worldToLocalMatrix;
            _localPositionsList.Clear();
            _colorsList.Clear();
            _uvsList.Clear();
            _trianglesList.Clear();
        }

        public void AddColumn(Vector3 leftEdgeWorldPosition, Vector3 rightEdgeWorldPosition, float alpha01)
        {
            var color = new Color32(FULL_COLOR_CHANNEL, FULL_COLOR_CHANNEL, FULL_COLOR_CHANNEL, (byte)(alpha01 * FULL_COLOR_CHANNEL));
            var lengthProgress01 = _addedColumnsCount / (float)GetColumnSpansCount(_expectedColumnsCount);

            AddVertex(leftEdgeWorldPosition, color, LEFT_EDGE_HORIZONTAL_UV, lengthProgress01);
            AddVertex(rightEdgeWorldPosition, color, RIGHT_EDGE_HORIZONTAL_UV, lengthProgress01);
            _addedColumnsCount++;
        }

        public void FinishBuilding()
        {
            _mesh.Clear();

            if (_addedColumnsCount < MIN_COLUMNS_FOR_RIBBON)
            {
                return;
            }

            AddQuadsBetweenColumns();
            _mesh.SetVertices(_localPositionsList);
            _mesh.SetColors(_colorsList);
            _mesh.SetUVs(MAIN_UV_CHANNEL_INDEX, _uvsList);
            _mesh.SetTriangles(_trianglesList, MAIN_SUB_MESH_INDEX);
        }

        private void AddVertex(Vector3 worldPosition, Color32 color, float horizontalUv, float verticalUv)
        {
            _localPositionsList.Add(_worldToLocalMatrix.MultiplyPoint3x4(worldPosition));
            _colorsList.Add(color);
            _uvsList.Add(new Vector2(horizontalUv, verticalUv));
        }

        private void AddQuadsBetweenColumns()
        {
            var quadsCount = GetColumnSpansCount(_addedColumnsCount);

            for (var quadIndex = 0; quadIndex < quadsCount; quadIndex++)
            {
                var leftVertexIndex = quadIndex * VERTICES_PER_COLUMN;
                var rightVertexIndex = leftVertexIndex + 1;
                var nextLeftVertexIndex = leftVertexIndex + VERTICES_PER_COLUMN;
                var nextRightVertexIndex = nextLeftVertexIndex + 1;

                _trianglesList.Add(leftVertexIndex);
                _trianglesList.Add(nextLeftVertexIndex);
                _trianglesList.Add(nextRightVertexIndex);

                _trianglesList.Add(leftVertexIndex);
                _trianglesList.Add(nextRightVertexIndex);
                _trianglesList.Add(rightVertexIndex);
            }
        }

        private int GetColumnSpansCount(int columnsCount)
        {
            return Mathf.Max(1, columnsCount - 1);
        }
    }
}
