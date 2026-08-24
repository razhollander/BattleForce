using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions.Linq;
using UnityEngine;
using Core.Game.Domains.GamePlay.Presentation.Scripts.DataService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs
{
    public class MatchEnvironmentWallsControllers : IMatchEnvironmentWallsControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly EnvironmentWallView _wallViewPrefab;
        private readonly IInterpolationDecayService _interpolationDecayService;
        private readonly List<MatchEnvironmentWallController> _wallControllers = new ();
        private GameObject _wallsParent;
        
        public MatchEnvironmentWallsControllers(IMatchDataService matchDataService, EnvironmentWallView wallViewPrefab, IInterpolationDecayService interpolationDecayService)
        {
            _matchDataService = matchDataService;
            _wallViewPrefab = wallViewPrefab;
            _interpolationDecayService = interpolationDecayService;
        }

        public void InitEntryPoint()
        {
            _wallsParent = new GameObject("EnvironmentWallsParent");
        }

        public void CreateWall(ushort wallId)
        {
            var wallController = new MatchEnvironmentWallController(wallId, _matchDataService, _interpolationDecayService);
            wallController.CreateWallView(_wallViewPrefab, _wallsParent.transform);
            _wallControllers.Add(wallController);
        }

        public void DestroyAll()
        {
            foreach (var wallController in _wallControllers)
            {
                wallController.Destroy();
            }
            _wallControllers.Clear();
        }

        public void UpdateWallTransform(ushort wallId)
        {
            var wallModel = _matchDataService.GetEnvironmentWall(wallId);
            _wallControllers.FindWithId(wallId).InterpulateTransform(wallModel.WorldPosition, wallModel.WorldRotationAngle);
        }
    }
}