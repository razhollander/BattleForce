using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.PlayerTeleportFX
{
    public class PlayerTeleportFXController
    {
        private readonly PlayerTeleportFXPool _pool;

        public PlayerTeleportFXController(PlayerTeleportFXView prefab, DiContainer container)
        {
            _pool = new PlayerTeleportFXPool(prefab, container);
        }

        public void InitEntryPoint()
        {
            _pool.InitPool();
        }

        public void PlayFX(Vector3 position)
        {
            var view = _pool.Spawn();
            view.transform.position = position;
            view.Play();
        }
    }
}
