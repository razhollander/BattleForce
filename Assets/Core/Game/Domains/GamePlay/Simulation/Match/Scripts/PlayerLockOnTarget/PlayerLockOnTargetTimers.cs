using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget
{
    public class PlayerLockOnTargetTimers
    {
        public ushort CasterId { get; }

        private readonly Dictionary<LockOnTargetKey, float> _targetTimers;
        private readonly List<LockOnTargetKey> _cachedTargetsToRemoveBuffer;

        public PlayerLockOnTargetTimers(ushort casterId)
        {
            CasterId = casterId;
            _targetTimers = new Dictionary<LockOnTargetKey, float>();
            _cachedTargetsToRemoveBuffer = new List<LockOnTargetKey>();
        }

        public void StepTimers(FixedUnorderedList<ObjectLockedOnTargetS2C> targetedIds, float deltaTime)
        {
            _cachedTargetsToRemoveBuffer.Clear();

            foreach (var targetKey in _targetTimers.Keys)
            {
                if (!targetedIds.ContainsTarget(targetKey))
                {
                    _cachedTargetsToRemoveBuffer.Add(targetKey);
                }
            }

            for (int i = 0; i < _cachedTargetsToRemoveBuffer.Count; i++)
            {
                _targetTimers.Remove(_cachedTargetsToRemoveBuffer[i]);
            }

            for (int i = 0; i < targetedIds.Count; i++)
            {
                var targetKey = targetedIds[i].GetKey();
                if (_targetTimers.TryGetValue(targetKey, out var timer))
                {
                    _targetTimers[targetKey] = timer + deltaTime;
                }
                else
                {
                    _targetTimers[targetKey] = deltaTime;
                }
            }
        }

        public bool IsTargetTimerEnded(LockOnTargetKey targetKey, float durationLimit)
        {
            return _targetTimers.TryGetValue(targetKey, out var timer) && timer >= durationLimit;
        }

        public void ResetTimer(LockOnTargetKey targetKey)
        {
            if (_targetTimers.ContainsKey(targetKey))
            {
                _targetTimers[targetKey] = 0f;
            }
        }

        public void ResetAllTimers()
        {
            _targetTimers.Clear();
        }
    }
}
