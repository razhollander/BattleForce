using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts
{
    public class EnvironmentLavaWallController
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
            var mesh = MeshUtils.BuildMesh(pointsUnityVector2, 2);
            _lavaWallView.SetMesh(mesh);
        }
    }
}