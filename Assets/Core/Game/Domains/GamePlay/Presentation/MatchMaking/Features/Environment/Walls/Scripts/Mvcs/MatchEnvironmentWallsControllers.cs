using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs
{
    public class MatchMakingEnvironmentWallsControllers : IMatchMakingEnvironmentWallsControllers
    {
        private readonly IMatchMakingDataService _matchDataService;
        private readonly EnvironmentWallView _wallViewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<MatchMakingEnvironmentWallController> _wallControllers = new ();
        private GameObject _wallsParent;
        
        public MatchMakingEnvironmentWallsControllers(IMatchMakingDataService matchDataService, EnvironmentWallView wallViewPrefab)
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
            var wallController = new MatchMakingEnvironmentWallController(wallId, _matchDataService);
            wallController.CreateWallView(_wallViewPrefab, _wallsParent.transform);
            _wallControllers.Add(wallController);
        }
    }
}