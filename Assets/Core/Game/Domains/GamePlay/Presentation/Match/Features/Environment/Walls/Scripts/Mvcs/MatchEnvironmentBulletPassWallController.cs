using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs
{
    public class MatchEnvironmentBulletPassWallController
    {
        private EnvironmentBulletPassWallView _wallView;
        private readonly IMatchDataService _matchDataService;
        public readonly ushort WallId;

        public MatchEnvironmentBulletPassWallController(ushort wallId, IMatchDataService matchDataService)
        {
            _matchDataService = matchDataService;
            WallId = wallId;
        }

        public void CreateBulletPassWallView(EnvironmentBulletPassWallView wallViewPrefab, Transform parent)
        {
            var wallModel = _matchDataService.GetBulletPassWall(WallId);
            _wallView = Object.Instantiate(wallViewPrefab, parent);
            _wallView.name = "EnvironmentBulletPassWall_" + WallId;
            var pointsUnityVector2 = wallModel.Points.Select(x => x.ToUnityVector2()).ToArray();
            var mesh = MeshUtils.BuildMesh(pointsUnityVector2, 0);
            _wallView.SetMesh(mesh);
        }

        public void Destroy()
        {
            Object.Destroy(_wallView.gameObject);
        }
    }
}
