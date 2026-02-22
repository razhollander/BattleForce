using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.RotatingWheels.Scripts.Mvc
{
    public interface IMatchEnvironmentRotatingWheelControllers
    {
        void InitEntryPoint();
        void CreateRotatingWheels();
        void UpdateRotation();
        void DestroyAll();
    }

    public class MatchEnvironmentRotatingWheelControllers : IMatchEnvironmentRotatingWheelControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly MatchEnvironmentRotatingWheelView _wheelViewPrefab;
        private readonly IMatchEnvironmentWallsControllers _wallsControllers;
        private readonly IEnvironmentLavaWallsControllers _lavaWallsControllers;
        private readonly IEnvironmentSpringControllers _springControllers;

        private readonly List<MatchEnvironmentRotatingWheelController> _controllers = new List<MatchEnvironmentRotatingWheelController>();
        private GameObject _parent;

        public MatchEnvironmentRotatingWheelControllers(IMatchDataService matchDataService, MatchEnvironmentRotatingWheelView wheelViewPrefab,
            IMatchEnvironmentWallsControllers wallsControllers, IEnvironmentLavaWallsControllers lavaWallsControllers, IEnvironmentSpringControllers springControllers)
        {
            _matchDataService = matchDataService;
            _wheelViewPrefab = wheelViewPrefab;
            _wallsControllers = wallsControllers;
            _lavaWallsControllers = lavaWallsControllers;
            _springControllers = springControllers;
        }

        public void InitEntryPoint()
        {
            _parent = new GameObject("EnvironmentRotatingWheelsParent");
        }

        public void CreateRotatingWheels()
        {
            foreach (var model in _matchDataService.RotatingWheels)
            {
                var controller = new MatchEnvironmentRotatingWheelController(model);
                controller.CreateView(_wheelViewPrefab, _parent.transform);
                _controllers.Add(controller);

                var wheelTransform = controller.GetViewTransform();

                foreach (var wallId in model.WallIds)
                {
                    _wallsControllers.CreateWall(wallId, wheelTransform);
                }
                foreach (var wallId in model.LavaWallIds)
                {
                     _lavaWallsControllers.CreateLavaWall(wallId, wheelTransform);
                }
                foreach (var springId in model.SpringIds)
                {
                    _springControllers.CreateSpring(springId, wheelTransform);
                }
            }
        }

        public void UpdateRotation()
        {
            foreach (var controller in _controllers)
            {
                controller.UpdateView();
            }
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers)
            {
                controller.Destroy();
            }
            _controllers.Clear();
            if (_parent != null)
            {
                Object.Destroy(_parent);
                _parent = null;
            }
        }
    }
}
