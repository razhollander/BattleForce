using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc
{
    public class MatchChickenEggController
    {
        private ChickenEggView _chickenEggView;
        private readonly ChickenEggPool _chickenEggPool;
        public readonly ushort EggId;
        private readonly Transform _eggsParent;

        public MatchChickenEggController(ushort eggId, ChickenEggPool chickenEggPool, Transform eggsParent)
        {
            _chickenEggPool = chickenEggPool;
            EggId = eggId;
            _eggsParent = eggsParent;
        }

        public void CreateEggView(Vector2 position)
        {
            _chickenEggView = _chickenEggPool.Spawn();
            _chickenEggView.name = "ChickenEgg_" + EggId;
            _chickenEggView.transform.SetParent(_eggsParent);
            _chickenEggView.SetPosition(position);
        }

        public async Awaitable PlayBreakAnimation(CancellationTokenSource cancellationTokenSource)
        {
            await _chickenEggView.PlayBreakAnimation(cancellationTokenSource);
        }

        public void Destroy()
        {
            _chickenEggView.Despawn();
        }
    }
}
