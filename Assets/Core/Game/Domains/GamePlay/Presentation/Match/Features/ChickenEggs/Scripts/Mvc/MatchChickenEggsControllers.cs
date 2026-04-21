using System;
using System.Collections.Generic;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc
{
    public class MatchChickenEggsControllers : IMatchChickenEggsControllers
    {
        private readonly ChickenEggPool _chickenEggPool;
        private readonly List<MatchChickenEggController> _eggControllers = new ();
        private Transform _eggsParent;

        public MatchChickenEggsControllers(ChickenEggView chickenEggViewPrefab, DiContainer diContainer)
        {
            _chickenEggPool = new ChickenEggPool(chickenEggViewPrefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _eggsParent = (new GameObject("ChickenEggsParent")).transform;
            _chickenEggPool.InitPool();
        }

        public void CreateEgg(ushort eggId, Vector2 position)
        {
            var controller = new MatchChickenEggController(eggId, _chickenEggPool, _eggsParent);
            controller.CreateEggView(position.ToUnityVector2());
            _eggControllers.Add(controller);
        }

        private MatchChickenEggController GetEgg(ushort eggId)
        {
            return _eggControllers.Find(x => x.EggId == eggId);
        }

        public async Awaitable BreakAndDestroyEgg(ushort eggId, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                await GetEgg(eggId).PlayBreakAnimation(cancellationTokenSource);
                LogService.LogError("reach break end!");
            }
            finally
            {
                LogService.LogError("destroy in finally!");
                DestroyEgg(eggId);
            }
        }

        public void DestroyEgg(ushort eggId)
        {
            var controller = GetEgg(eggId);
            if (controller == null)
            {
                LogService.LogError($"Tried destroy chieck egg {eggId} but it wasn't found!");
                return;
            }
            
            controller.Destroy();
            _eggControllers.Remove(controller);
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
