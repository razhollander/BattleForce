using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Nuke.Scripts
{
    public class NukeShockwaveEffectPool : PrefabsPool<NukeShockwaveEffectView>
    {
        protected override string ParentGameObjectName => "NukeShockwaveEffectPool";

        public NukeShockwaveEffectPool(NukeShockwaveEffectView view, DiContainer diContainer) : base(
            new PoolData(6, 1), diContainer, view)
        {
        }
    }
}
