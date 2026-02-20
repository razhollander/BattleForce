using Core.Scripts.Utils.Pools;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.PlayerTeleportFX
{
    public class PlayerTeleportFXController
    {
        private readonly PlayerTeleportFXView.Pool _pool;

        public PlayerTeleportFXController(PlayerTeleportFXView.Pool pool)
        {
            _pool = pool;
        }

        public void PlayFX(Vector3 position)
        {
            var view = _pool.Spawn();
            view.transform.position = position;
            view.Init((v) => _pool.Despawn(v));
            view.Play();
        }
    }
}
