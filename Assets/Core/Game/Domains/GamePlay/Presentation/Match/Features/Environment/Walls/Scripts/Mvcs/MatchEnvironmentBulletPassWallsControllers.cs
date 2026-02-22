using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs
{
    public class MatchEnvironmentBulletPassWallsControllers : IMatchEnvironmentBulletPassWallsControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly EnvironmentBulletPassWallView _wallViewPrefab;
        private readonly List<MatchEnvironmentBulletPassWallController> _wallControllers = new ();
        private GameObject _wallsParent;

        public MatchEnvironmentBulletPassWallsControllers(IMatchDataService matchDataService, EnvironmentBulletPassWallView wallViewPrefab)
        {
            _matchDataService = matchDataService;
            _wallViewPrefab = wallViewPrefab;
        }

        public void InitEntryPoint()
        {
            _wallsParent = new GameObject("EnvironmentBulletPassWallsParent");
        }

        public void CreateBulletPassWall(ushort wallId)
        {
            var wallController = new MatchEnvironmentBulletPassWallController(wallId, _matchDataService);
            wallController.CreateBulletPassWallView(_wallViewPrefab, _wallsParent.transform);
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
    }
}
