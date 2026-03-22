using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts
{
    public class KOProjectilePool : PrefabsPool<KOProjectileView>
    {
        public KOProjectilePool(KOProjectileView prefab, Transform parent) : base(prefab, parent)
        {
        }
    }
}
