using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc
{
    public class PowerUpBallPool : PrefabsPool<PowerUpBallView>
    {
        protected override string ParentGameObjectName => "PowerUpBallsPool";

        public PowerUpBallPool(PowerUpBallView view, DiContainer diContainer) : base(
            new PoolData(5, 2), diContainer, view)
        {
        }
    }
}
