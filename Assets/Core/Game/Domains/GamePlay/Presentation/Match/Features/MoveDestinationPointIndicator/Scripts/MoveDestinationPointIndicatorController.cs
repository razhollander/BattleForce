using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.LayerOrders;
using Core.Scripts.Utils;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MoveDestinationPointIndicator.Scripts
{
    public class MoveDestinationPointIndicatorController : IMoveDestinationPointIndicatorController
    {
        private readonly MoveDestinationPointIndicatorPool _pool;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;

        public MoveDestinationPointIndicatorController(MoveDestinationPointIndicatorView prefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _pool = new MoveDestinationPointIndicatorPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _pool.InitPool();
        }

        public void ShowIndicator(Vector2 destinationPoint)
        {
            var view = _pool.Spawn();
            view.transform.position = new Vector3(destinationPoint.x, destinationPoint.y, LayerOrder.Effects);
            view.PlayAndDespawn(_stageCancellationTokenProvider.CancellationTokenSource).Forget();
        }
    }
}
