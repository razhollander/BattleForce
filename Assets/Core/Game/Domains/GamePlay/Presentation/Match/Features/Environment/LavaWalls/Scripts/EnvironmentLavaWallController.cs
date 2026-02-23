using System;
using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.LayerOrders;
using Core.Scripts.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts
{
    public class EnvironmentLavaWallController : IEquatable<ushort>
    {
        private EnvironmentLavaWallView _lavaWallView;
        private readonly IMatchDataService _matchDataService;
        public readonly ushort LavaWallId;

        public EnvironmentLavaWallController(ushort lavaWallId, IMatchDataService matchDataService)
        {
            _matchDataService = matchDataService;
            LavaWallId = lavaWallId;
        }
        
        public void CreateWallView(EnvironmentLavaWallView wallViewPrefab, Transform parent)
        {
            var lavaWallModel = _matchDataService.GetEnvironmentLavaWall(LavaWallId);
            _lavaWallView = Object.Instantiate(wallViewPrefab, parent);
            _lavaWallView.name = "EnvironmentLavaWall_" + LavaWallId;
            var pointsUnityVector2 = lavaWallModel.Points.Select(x => x.ToUnityVector2()).ToArray();
            var mesh = MeshUtils.BuildMesh(pointsUnityVector2, LayerOrder.EnvironmentWall);
            _lavaWallView.SetMesh(mesh);
            UpdateTransform(lavaWallModel.WorldPosition, lavaWallModel.WorldRotationAngle);
        }

        public bool Equals(ushort otherId)
        {
            return LavaWallId == otherId;
        }

        public void UpdateTransform(Vector2 position, float rotationDegrees)
        {
            _lavaWallView.transform.position = position.ToUnityVector2();
            _lavaWallView.transform.rotation = rotationDegrees.AngleToQuaternion();
        }

        public void Destroy()
        {
            Object.Destroy(_lavaWallView.gameObject);
        }
    }
}