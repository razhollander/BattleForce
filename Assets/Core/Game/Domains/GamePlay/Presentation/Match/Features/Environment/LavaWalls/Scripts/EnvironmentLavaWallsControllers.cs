using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions.Linq;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts
{
    public class EnvironmentLavaWallsControllers : IEnvironmentLavaWallsControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly EnvironmentLavaWallView _lavaWallViewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<EnvironmentLavaWallController> _lavaWallControllers = new ();
        private GameObject _lavaWallsParent;
        
        public EnvironmentLavaWallsControllers(IMatchDataService matchDataService, EnvironmentLavaWallView lavaWallViewPrefab)
        {
            _matchDataService = matchDataService;
            _lavaWallViewPrefab = lavaWallViewPrefab;
        }

        public void InitEntryPoint()
        {
            _lavaWallsParent = new GameObject("EnvironmentLavaWallsParent");
        }

        public void CreateLavaWall(ushort wallId, Transform parent = null)
        {
            var lavaWallController = new EnvironmentLavaWallController(wallId, _matchDataService);
            lavaWallController.CreateWallView(_lavaWallViewPrefab, parent != null ? parent : _lavaWallsParent.transform);
            _lavaWallControllers.Add(lavaWallController);
        }

        public void DestroyAll()
        {
            foreach (var lavaWallController in _lavaWallControllers)
            {
                lavaWallController.Destroy();
            }
            _lavaWallControllers.Clear();
        }

        public void UpdateLavaWallTransform(ushort lavaWallId)
        {
            var lavaWallModel = _matchDataService.GetEnvironmentLavaWall(lavaWallId);
            _lavaWallControllers.FindWithId(lavaWallId).UpdateTransform(lavaWallModel.WorldPosition, lavaWallModel.WorldRotationAngle);
        }
    }
}