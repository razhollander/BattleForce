using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts
{
    /// <summary>
    /// Note: Since currently the second cast arrows are only used by the FishingRod then their id is the FishingRod id.
    /// In the future if more features use the second arrow then need to solve conflicts between id's. 
    /// </summary>
    public class SecondCastAimArrowControllers : ISecondCastAimArrowControllers
    {
        private const string PARENT_GAME_OBJECT_NAME = "SecondCastArrowsParent";

        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SecondCastAimArrowPool _pool;
        private readonly Dictionary<ushort, SecondCastAimArrowView> _arrowViewPerId = new();
        private Transform _parentTransform;

        public SecondCastAimArrowControllers(SecondCastAimArrowView prefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig)
        {
            _gamePlayConfig = gamePlayConfig;
            _pool = new SecondCastAimArrowPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _parentTransform = new GameObject(PARENT_GAME_OBJECT_NAME).transform;
            _pool.InitPool();
        }

        public void InitExitPoint()
        {
            if (_parentTransform == null)
            {
                return;
            }

            DestroyAll();
            _pool.DisposePool();
            Object.Destroy(_parentTransform.gameObject);
            _parentTransform = null;
        }

        public void AddArrow(ushort id, Vector2 position, Vector2 direction)
        {
            var view = _pool.Spawn();
            view.transform.SetParent(_parentTransform);
            view.Setup(position, direction.ToQuaternion());
            _arrowViewPerId[id] = view;
        }

        public void SetArrow(ushort id, Vector2 position, Vector2 direction)
        {
            if (!_arrowViewPerId.TryGetValue(id, out var view))
            {
                Debug.LogError($"SecondCastAimArrowView with id {id} not found");
                return;
            }

            var rotation = direction.ToQuaternion();
            var decay = _gamePlayConfig.ExponentialDecay;
            var lerpedPosition = MathUtils.ExpDecay(view.Position, position, decay, Time.deltaTime);
            var lerpedRotation = MathUtils.ExpDecay(view.Rotation, rotation, decay, Time.deltaTime);
            view.SetTransform(lerpedPosition, lerpedRotation);
        }
        
        public void TryRemoveArrow(ushort id)
        {
            if (!_arrowViewPerId.TryGetValue(id, out var view)) // A caster can lose its second cast before an arrow was ever shown, so a missing arrow is a valid case here.
            {
                return;
            }

            view.Despawn();
            _arrowViewPerId.Remove(id);
        }

        public void DestroyAll()
        {
            foreach (var view in _arrowViewPerId.Values)
            {
                view.Despawn();
            }

            _arrowViewPerId.Clear();
        }
    }
}
