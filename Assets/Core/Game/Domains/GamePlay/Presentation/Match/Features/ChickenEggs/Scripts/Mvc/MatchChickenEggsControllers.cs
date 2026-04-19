using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;
using Zenject;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc
{
    public class MatchChickenEggsControllers : IMatchChickenEggsControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly ChickenEggPool _chickenEggPool;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<MatchChickenEggController> _eggControllers = new ();
        private Transform _eggsParent;

        public MatchChickenEggsControllers(IMatchDataService matchDataService, ChickenEggView chickenEggViewPrefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;

            _chickenEggPool = new ChickenEggPool(chickenEggViewPrefab, diContainer);
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _eggsParent = (new GameObject("ChickenEggsParent")).transform;
            _chickenEggPool.InitPool();
        }

        public void CreateEgg(ushort eggId, Vector2 position)
        {
            var controller = new MatchChickenEggController(eggId, _chickenEggPool, _eggsParent);
            controller.CreateEggView(position);
            _eggControllers.Add(controller);
        }

        public void UpdateEggsTransform()
        {
            foreach (var controller in _eggControllers)
            {
                var model = _matchDataService.GetChickenEgg(controller.EggId);
                controller.InterpolatePosition(model.Position, _gamePlayConfig.ExponentialDecay);
            }
        }

        public void BreakEgg(ushort eggId)
        {
            var controller = GetEgg(eggId);
            if (controller != null)
            {
                controller.PlayBreakAnimation();
            }
        }

        private MatchChickenEggController GetEgg(ushort eggId)
        {
            return _eggControllers.Find(x => x.EggId == eggId);
        }

        public void DestroyEgg(ushort eggId)
        {
            var controller = GetEgg(eggId);
            if (controller != null)
            {
                controller.Destroy();
                _eggControllers.Remove(controller);
            }
        }

        public void DestroyAll()
        {
            foreach (var controller in _eggControllers)
            {
                controller.Destroy();
            }
            _eggControllers.Clear();
        }
    }
}
