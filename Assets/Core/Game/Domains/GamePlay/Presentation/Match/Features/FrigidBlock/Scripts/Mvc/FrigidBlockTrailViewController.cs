using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc
{
    // Builds a world-space ribbon mesh from two emitter transforms. A new column (one point per
    // emitter) is committed to a ring buffer every time the emitters travel _spawnDistance world
    // units; columns older than _pointLifetime seconds are discarded from the tail, and the buffer
    // is hard-capped at _capacity columns. The current live emitter positions are always appended as
    // the leading edge so the head tracks the block smoothly between commits.
    //
    // Points are stored in world space and converted to the trail object's local space when the mesh
    // is written, so the ribbon stays anchored in the world while the block GameObject moves.
    // All buffers are allocated once and reused every frame (no per-frame garbage).
    public class FrigidBlockTrailViewController
    {
        private const int VertsPerColumn = 2;
        private const int IndicesPerQuad = 6;

        private readonly Transform _trailTransform;
        private readonly Transform _emitterA;
        private readonly Transform _emitterB;
        private readonly Mesh _mesh;
        private readonly float _spawnDistanceSqr;
        private readonly float _pointLifetime;

        // Ring buffer of committed columns.
        private readonly Vector3[] _worldA;
        private readonly Vector3[] _worldB;
        private readonly float[] _spawnTime;
        private readonly int _capacity;
        private int _head;
        private int _count;
        private Vector3 _lastCommitReference;

        // Reused mesh build buffers (capacity includes the live leading column).
        private readonly List<Vector3> _vertices;
        private readonly List<Color32> _colors;
        private readonly List<Vector2> _uvs;
        private readonly List<int> _triangles;

        public FrigidBlockTrailViewController(FrigidBlockView view)
        {
            _trailTransform = view.TrailTransform;
            _emitterA = view.TrailEmitterA;
            _emitterB = view.TrailEmitterB;
            _mesh = view.TrailMesh;
            var spawnDistance = Mathf.Max(0.0001f, view.PointSpawnDistance);
            _spawnDistanceSqr = spawnDistance * spawnDistance;
            _pointLifetime = Mathf.Max(0f, view.PointLifetime);

            _capacity = Mathf.Max(2, view.MaxTrailPoints);
            _worldA = new Vector3[_capacity];
            _worldB = new Vector3[_capacity];
            _spawnTime = new float[_capacity];

            var maxColumns = _capacity + 1;
            _vertices = new List<Vector3>(maxColumns * VertsPerColumn);
            _colors = new List<Color32>(maxColumns * VertsPerColumn);
            _uvs = new List<Vector2>(maxColumns * VertsPerColumn);
            _triangles = new List<int>((maxColumns - 1) * IndicesPerQuad);
        }

        // Collapses the whole trail onto the current emitter positions so a freshly pooled block does
        // not draw a streak from wherever it was last used.
        public void Reset(float time)
        {
            _head = 0;
            _count = 0;
            var liveA = _emitterA.position;
            var liveB = _emitterB.position;
            _lastCommitReference = (liveA + liveB) * 0.5f;
            Commit(liveA, liveB, time);
            RebuildMesh(liveA, liveB, time);
        }

        public void Advance(float time)
        {
            var liveA = _emitterA.position;
            var liveB = _emitterB.position;

            // Drop columns that have outlived _pointLifetime (from the tail / oldest end).
            while (_count > 0 && time - _spawnTime[_head] > _pointLifetime)
            {
                _head = (_head + 1) % _capacity;
                _count--;
            }

            // Commit a new column once the emitters have travelled far enough since the last one.
            var reference = (liveA + liveB) * 0.5f;
            if ((reference - _lastCommitReference).sqrMagnitude >= _spawnDistanceSqr)
            {
                Commit(liveA, liveB, time);
                _lastCommitReference = reference;
            }

            RebuildMesh(liveA, liveB, time);
        }

        private void Commit(Vector3 worldA, Vector3 worldB, float time)
        {
            if (_count == _capacity)
            {
                // Full: overwrite the oldest column.
                _head = (_head + 1) % _capacity;
                _count--;
            }

            var tail = (_head + _count) % _capacity;
            _worldA[tail] = worldA;
            _worldB[tail] = worldB;
            _spawnTime[tail] = time;
            _count++;
        }

        private void RebuildMesh(Vector3 liveA, Vector3 liveB, float time)
        {
            _vertices.Clear();
            _colors.Clear();
            _uvs.Clear();
            _triangles.Clear();

            var totalColumns = _count + 1;
            if (totalColumns < 2)
            {
                _mesh.Clear();
                return;
            }

            var lastColumnIndex = totalColumns - 1;
            for (var column = 0; column < totalColumns; column++)
            {
                Vector3 worldA;
                Vector3 worldB;
                float alpha01;

                if (column < _count)
                {
                    var index = (_head + column) % _capacity;
                    worldA = _worldA[index];
                    worldB = _worldB[index];
                    // Oldest column fades to zero, newest committed column stays opaque.
                    alpha01 = Mathf.Clamp01(1f - (time - _spawnTime[index]) / _pointLifetime);
                }
                else
                {
                    // Leading live edge tracks the emitters every frame.
                    worldA = liveA;
                    worldB = liveB;
                    alpha01 = 1f;
                }

                var color = new Color32(255, 255, 255, (byte)(alpha01 * 255f));
                var v = column / (float)lastColumnIndex;

                _vertices.Add(_trailTransform.InverseTransformPoint(worldA));
                _colors.Add(color);
                _uvs.Add(new Vector2(0f, v));

                _vertices.Add(_trailTransform.InverseTransformPoint(worldB));
                _colors.Add(color);
                _uvs.Add(new Vector2(1f, v));
            }

            for (var column = 0; column < lastColumnIndex; column++)
            {
                var a0 = column * VertsPerColumn;
                var b0 = a0 + 1;
                var a1 = a0 + VertsPerColumn;
                var b1 = a1 + 1;

                _triangles.Add(a0);
                _triangles.Add(a1);
                _triangles.Add(b1);

                _triangles.Add(a0);
                _triangles.Add(b1);
                _triangles.Add(b0);
            }

            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetColors(_colors);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetTriangles(_triangles, 0);
        }
    }
}
