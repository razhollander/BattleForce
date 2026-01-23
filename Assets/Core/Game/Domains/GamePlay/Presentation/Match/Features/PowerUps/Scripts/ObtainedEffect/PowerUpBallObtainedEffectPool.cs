using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.ObtainedEffect
{
    public class PowerUpBallObtainedEffectPool : PrefabsPool<PowerUpBallObtainedEffectView>
    {
        protected override string ParentGameObjectName => "PowerUpBallObtainedEffect";

        public PowerUpBallObtainedEffectPool(PowerUpBallObtainedEffectView view, DiContainer diContainer) : base(
            new PoolData(3, 1), diContainer, view)
        {
        }
    }
}