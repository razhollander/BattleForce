using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc
{
    public class ScoreGatesControllers : IScoreGatesControllers
    {
        private const string PARENT_GAME_OBJECT_NAME = "ScoreGatesParent";

        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly ScoreGatePool _pool;
        private readonly Dictionary<ushort, ScoreGateController> _controllers = new Dictionary<ushort, ScoreGateController>();
        private Transform _parentTransform;

        public ScoreGatesControllers(ScoreGateView prefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _gamePlayConfig = gamePlayConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _pool = new ScoreGatePool(prefab, diContainer);
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

        public bool HasScoreGate(ushort id)
        {
            return _controllers.ContainsKey(id);
        }

        public void CreateScoreGate(ushort id, Vector2 position, Quaternion rotation, ushort lastScoredTeamId, byte scoreMultiplier, float mapSizeMultiplier)
        {
            if (_controllers.ContainsKey(id))
            {
                return;
            }

            var postSize = _sharedGamePlayConfig.ScoreGatePostSize * mapSizeMultiplier;
            var gapWidth = _sharedGamePlayConfig.ScoreGateGapWidth * mapSizeMultiplier;

            var controller = new ScoreGateController(id, _pool, _parentTransform);
            controller.CreateView(position, rotation, postSize, gapWidth);
            _controllers.Add(id, controller);

            ApplyTeamColor(controller, lastScoredTeamId);
            controller.SetScoreMultiplier(scoreMultiplier, shouldPunchOnIncrease: false);
        }

        public void InterpolateScoreGateTransform(ushort id, Vector2 position, Quaternion rotation)
        {
            if (_controllers.TryGetValue(id, out var controller))
            {
                controller.InterpolateTransform(position, rotation, _gamePlayConfig.ExponentialDecay);
            }
        }

        public void SetTeamColor(ushort id, ushort teamId)
        {
            if (_controllers.TryGetValue(id, out var controller))
            {
                ApplyTeamColor(controller, teamId);
            }
        }

        public void SetScoreMultiplier(ushort id, byte scoreMultiplier)
        {
            if (_controllers.TryGetValue(id, out var controller))
            {
                controller.SetScoreMultiplier(scoreMultiplier, shouldPunchOnIncrease: true);
            }
        }

        public void PlayScoreGatePassedAnimation(ushort id)
        {
            if (_controllers.TryGetValue(id, out var controller))
            {
                controller.PlayPassAnimation();
            }
        }

        public bool TryGetScoreGatePosition(ushort id, out Vector2 position)
        {
            if (_controllers.TryGetValue(id, out var controller))
            {
                position = controller.Position;
                return true;
            }

            position = default;
            return false;
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers.Values)
            {
                controller.Destroy();
            }

            _controllers.Clear();
        }

        // teamId 0 means the gate has not been scored on yet, so it has no team colour.
        public bool TryGetTeamColor(ushort teamId, out Color color)
        {
            if (teamId != 0 && _gamePlayConfig.ColorPerTeamId.TryGetValue(teamId, out color))
            {
                return true;
            }

            color = default;
            return false;
        }

        // teamId 0 means the gate has not been scored on yet, so it keeps the prefab's neutral colour.
        private void ApplyTeamColor(ScoreGateController controller, ushort teamId)
        {
            if (TryGetTeamColor(teamId, out var color))
            {
                controller.SetTeamColor(color);
            }
        }
    }
}
