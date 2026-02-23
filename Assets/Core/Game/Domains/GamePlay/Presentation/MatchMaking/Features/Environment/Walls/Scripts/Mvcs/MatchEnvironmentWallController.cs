using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs
{
    public class MatchMakingEnvironmentWallController
    {
        private EnvironmentWallView _wallView;
        private readonly IMatchMakingDataService _matchDataService;
        public readonly ushort WallId;

        public MatchMakingEnvironmentWallController(ushort wallId, IMatchMakingDataService matchDataService)
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
            _wallView.transform.localPosition = wallModel.LocalPosition.ToUnityVector2();
        }
    }
}