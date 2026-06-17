using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.WaterGunStream.Scripts.Mvc
{
    public class WaterGunStreamControllers : IWaterGunStreamControllers
    {
        private readonly WaterGunStreamPool _pool;
        private readonly Dictionary<ushort, WaterGunStreamView> _activeViews = new();

        public WaterGunStreamControllers(WaterGunStreamView prefab, DiContainer diContainer)
        {
            _pool = new WaterGunStreamPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _pool.InitPool();
        }

        public void Spawn(ushort playerId)
        {
            if (_activeViews.ContainsKey(playerId))
            {
                return;
            }

            var view = _pool.Spawn();
            _activeViews[playerId] = view;
        }

        public void Despawn(ushort playerId)
        {
            if (!_activeViews.TryGetValue(playerId, out var view))
            {
                return;
            }

            view.Despawn();
            _activeViews.Remove(playerId);
        }

        public void Tick(ushort playerId, Vector2 aimDirection, float angularVelocity)
        {
            if (_activeViews.TryGetValue(playerId, out var view))
            {
                view.UpdateStream(aimDirection, angularVelocity);
            }
        }
    }
}
