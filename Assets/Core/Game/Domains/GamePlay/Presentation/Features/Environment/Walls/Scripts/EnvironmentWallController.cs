using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls
{
    public class EnvironmentWallController
    {
        private EnvironmentWallView _wallView;
        private readonly IMatchDataService _matchDataService;
        public readonly ushort WallId;

        public EnvironmentWallController(ushort wallId, IMatchDataService matchDataService)
        {
            _matchDataService = matchDataService;
            WallId = wallId;
        }
        
        public void CreateWallView(EnvironmentWallView wallViewPrefab, Transform parent)
        {
            var wallModel = _matchDataService.GetEnvironmentWall(WallId);
            _wallView = Object.Instantiate(wallViewPrefab, parent);
            _wallView.name = "EnvironmentWall_" + WallId;
            var mesh = MeshUtils.BuildMesh(wallModel.Points, 0);
            _wallView.SetMesh(mesh);
        }
    }
}