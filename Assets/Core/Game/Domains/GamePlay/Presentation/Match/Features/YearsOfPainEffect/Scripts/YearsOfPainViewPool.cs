using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.YearsOfPainEffect.Scripts
{
    public class YearsOfPainViewPool : PrefabsPool<YearsOfPainView>
    {
        public YearsOfPainViewPool(YearsOfPainView prefab, DiContainer container) : base(prefab, container)
        {
        }
    }
}
