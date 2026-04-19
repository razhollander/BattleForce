using Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
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

        public void CreateEggView(System.Numerics.Vector2 position)
        {
            _chickenEggView = _chickenEggPool.Spawn();
            _chickenEggView.name = "ChickenEgg_" + EggId;
            _chickenEggView.transform.SetParent(_eggsParent);
            _chickenEggView.SetPosition(position.ToUnityVector2());
        }

        public void InterpolatePosition(System.Numerics.Vector2 position, float decay)
        {
            _chickenEggView.InterpolatePosition(position.ToUnityVector2(), decay);
        }

        public void PlayBreakAnimation()
        {
            _chickenEggView.PlayBreakAnimation();
        }

        public void Destroy()
        {
            _chickenEggView.Despawn();
        }
    }
}
