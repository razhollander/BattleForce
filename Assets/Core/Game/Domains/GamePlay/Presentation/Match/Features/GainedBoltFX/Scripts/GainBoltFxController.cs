using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FX.Scripts
{
    public class GainBoltFxController : IGainBoltFxController
    {
        private readonly GainBoltFxView _prefab;
        private readonly DiContainer _diContainer;
        private Transform _container;
        private readonly List<GainBoltFxView> _instances = new List<GainBoltFxView>();

        public GainBoltFxController(GainBoltFxView prefab, DiContainer diContainer)
        {
            _prefab = prefab;
            _diContainer = diContainer;
        }

        public void InitEntryPoint()
        {
            _container = new GameObject("GainBoltFxContainer").transform;
        }

        public void ShowFx(int amount, Vector2 position)
        {
            var view = GetView();
            view.Show(amount, position);
        }

        private GainBoltFxView GetView()
        {
            foreach (var instance in _instances)
            {
                if (!instance.gameObject.activeSelf)
                {
                    return instance;
                }
            }

            var newInstance = Object.Instantiate(_prefab, _container);
            _diContainer.Inject(newInstance);
            _instances.Add(newInstance);
            return newInstance;
        }
    }
}
