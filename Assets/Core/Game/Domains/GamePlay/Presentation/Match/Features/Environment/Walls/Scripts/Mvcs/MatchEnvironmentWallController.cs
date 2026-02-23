using System;
using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs
{
    public class MatchEnvironmentWallController : IEquatable<ushort>
    {
        private EnvironmentWallView _wallView;
        private readonly IMatchDataService _matchDataService;
        public readonly ushort WallId;

        public MatchEnvironmentWallController(ushort wallId, IMatchDataService matchDataService)
        {
            _matchDataService = matchDataService;
            WallId = wallId;
        }
        
        public void CreateWallView(EnvironmentWallView wallViewPrefab, Transform parent)
        {
            var wallModel = _matchDataService.GetEnvironmentWall(WallId);
            _wallView = Object.Instantiate(wallViewPrefab, parent);
            _wallView.name = "EnvironmentWall_" + WallId;
            var pointsUnityVector2 = wallModel.Points.Select(x => x.ToUnityVector2()).ToArray();
            var mesh = MeshUtils.BuildMesh(pointsUnityVector2, 0);
            _wallView.SetMesh(mesh);
            UpdateTransform(wallModel.WorldPosition, wallModel.WorldRotationAngle);
        }

        public void Destroy()
        {
            Object.Destroy(_wallView.gameObject);
        }

        public bool Equals(ushort otherId)
        {
            return WallId == otherId;
        }

        public void UpdateTransform(Vector2 position, float rotationDegrees)
        {
            _wallView.transform.position = position.ToUnityVector2();
            _wallView.transform.rotation = rotationDegrees.AngleToQuaternion();
        }
    }
}