using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.LayerOrders;
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

        public void CreateEggView(Vector2 position, Color outlineColor)
        {
            _chickenEggView = _chickenEggPool.Spawn();
            _chickenEggView.name = "ChickenEgg_" + EggId;
            _chickenEggView.transform.SetParent(_eggsParent);
            _chickenEggView.Setup(new Vector3(position.x, position.y, LayerOrder.ChickenEgg), outlineColor);
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
