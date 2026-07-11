using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts
{
    public class SecondCastEffectController : ISecondCastEffectController
    {
        private const float ShownDirectionThresholdSqr = 0.0001f;

        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SecondCastAimArrowPool _pool;
        private readonly Dictionary<ushort, SecondCastAimArrowView> _arrowPerTipId = new();
        private Transform _parentTransform;

        public SecondCastEffectController(SecondCastAimArrowView prefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig)
        {
            _gamePlayConfig = gamePlayConfig;
            _pool = new SecondCastAimArrowPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _parentTransform = new GameObject("SecondCastArrowsParent").transform;
            _pool.InitPool();
        }

        public void SetArrow(ushort tipId, Vector2 position, Vector2 direction)
        {
            var isShown = direction.sqrMagnitude > ShownDirectionThresholdSqr;
            if (!isShown)
            {
                RemoveArrow(tipId);
                return;
            }

            var rotation = direction.ToQuaternion();

            if (!_arrowPerTipId.TryGetValue(tipId, out var view))
            {
                view = _pool.Spawn();
                view.transform.SetParent(_parentTransform);
                view.Setup(position, rotation);
                _arrowPerTipId[tipId] = view;
                return;
            }

            var decay = _gamePlayConfig.ExponentialDecay;
            var lerpedPosition = MathUtils.ExpDecay(view.Position, position, decay, Time.deltaTime);
            var lerpedRotation = MathUtils.ExpDecay(view.Rotation, rotation, decay, Time.deltaTime);
            view.SetTransform(lerpedPosition, lerpedRotation);
        }

        public void RemoveArrow(ushort tipId)
        {
            if (_arrowPerTipId.TryGetValue(tipId, out var view))
            {
                view.Despawn();
                _arrowPerTipId.Remove(tipId);
            }
        }

        public void DestroyAll()
        {
            foreach (var view in _arrowPerTipId.Values)
            {
                view.Despawn();
            }

            _arrowPerTipId.Clear();
        }
    }
}
