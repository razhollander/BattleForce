using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public class MoleControllers : IMoleControllers
    {
        private readonly MolePool _pool;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly List<MoleController> _controllers = new List<MoleController>();
        private Transform _parent;

        public MoleControllers(MoleView moleViewPrefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _pool = new MolePool(moleViewPrefab, diContainer);
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
        }

        public void InitEntryPoint()
        {
            _parent = (new GameObject("MolesParent")).transform;
            _pool.InitPool();
        }

        public void CreateMole(ushort moleId, Vector2 position)
        {
            var controller = new MoleController(moleId, _pool, _parent, _stageCancellationTokenProvider);
            controller.CreateView(position);
            _controllers.Add(controller);
        }

        public Vector2 GetMolePosition(ushort moleId)
        {
            var controller = GetController(moleId);
            return controller == null ? Vector2.zero : controller.GetPosition();
        }

        public void DestroyMoleWithHitEffect(ushort moleId)
        {
            if (!TryTakeController(moleId, out var controller))
            {
                return;
            }

            controller.DestroyViewWithHitEffect();
        }

        public void DestroyMoleWithExpireEffect(ushort moleId)
        {
            if (!TryTakeController(moleId, out var controller))
            {
                return;
            }

            controller.DestroyViewWithExpireEffect();
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers)
            {
                controller.DestroyViewImmediately();
            }

            _controllers.Clear();
        }

        private bool TryTakeController(ushort moleId, out MoleController controller)
        {
            controller = GetController(moleId);

            if (controller == null)
            {
                LogService.LogError($"No mole controller to destroy with id {moleId}!");
                return false;
            }

            _controllers.Remove(controller);
            return true;
        }

        private MoleController GetController(ushort moleId)
        {
            return _controllers.Find(x => x.MoleId == moleId);
        }
    }
}
