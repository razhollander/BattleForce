using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls
{
    public class EnvironmentWallsControllers : IEnvironmentWallsControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly EnvironmentWallView _wallViewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<EnvironmentWallController> _wallControllers = new ();
        private GameObject _wallsParent;
        
        public EnvironmentWallsControllers(IMatchDataService matchDataService, EnvironmentWallView wallViewPrefab)
        {
            _matchDataService = matchDataService;
            _wallViewPrefab = wallViewPrefab;
        }

        public void InitEntryPoint()
        {
            _wallsParent = new GameObject("EnvironmentWallsParent");
        }

        public void CreateWall(ushort wallId)
        {
            var wallController = new EnvironmentWallController(wallId, _matchDataService);
            wallController.CreateWallView(_wallViewPrefab, _wallsParent.transform);
            _wallControllers.Add(wallController);
        }
    }
}